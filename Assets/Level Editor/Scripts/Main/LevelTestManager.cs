#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for LINQ filtering
using System.Text;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class LevelTestManager : SerializedManager<LevelTestManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private LevelABTestType currentABType;
        [SerializeField] private LevelVariantSO currentVariant;
        [SerializeField] private LevelDataSO currentLevelDataSO;

        [ShowInInspector, ReadOnly] private bool isTestingInProgress = false;
        [ShowInInspector, ReadOnly] private int currentTestingLevelIndex = 0; // Will show the actual level number being tested
        [ShowInInspector, ReadOnly] private bool isTestingSpecialLevels = false;
        [ShowInInspector, ReadOnly] private int totalLevelsToTest = 0; // Total levels in the current test run (all, range, etc.)
        [ShowInInspector, ReadOnly] private int currentLevelTestCount = 0; // How many levels have been tested in the current run
        [ProgressBar(0, 100), ShowInInspector, ReadOnly] private int currentTestingProgress = 0; // Percentage progress


        [ShowInInspector, ReadOnly] private List<LevelTestResult> testResults = new List<LevelTestResult>();
        [ShowInInspector] private bool exportReportToCSV = true;
        [ShowInInspector, FilePath(AbsolutePath = false)] private string csvExportPath = "LevelSolverTest_Report.csv"; // Made relative path default

        private Coroutine testingCoroutine;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public LevelDataSO CurrentLevelDataSO => currentLevelDataSO;
        public LevelVariantSO CurrentVariant
        {
            get
            {
                // Attempt to get variant if null
                if (currentVariant == null && ResourceManager.Instance != null)
                {
                    currentVariant = ResourceManager.Instance.GetLevelVariant(currentABType);
                    if (currentVariant == null)
                    {
                        Debug.LogWarning($"Could not find Level Variant for AB Type: {currentABType}");
                    }
                }
                return currentVariant;
            }
        }

        public bool IsTestingInProgress => isTestingInProgress;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            // Ensure GameplayManager and its Instance are correctly implemented
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.StartEditorGamePlay();
            }
            else
            {
                Debug.LogError("GameplayManager instance not found during LevelTestManager Awake.");
            }
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region LEVEL SOLVER TESTING METHODS

        // --- Existing Test Methods ---

        [Button("Start Testing All Levels")]
        [EnableIf("@!IsTestingInProgress")]
        [PropertyOrder(1)] // Control button order
        public void StartTestingAllLevels()
        {
            if (isTestingInProgress) return;
            if (CurrentVariant == null)
            {
                Debug.LogError("Current Level Variant is not set or cannot be found.");
                return;
            }
            testResults.Clear();
            testingCoroutine = StartCoroutine(TestAllLevelsCoroutine());
        }

        [Button("Start Testing Special Levels Only")]
        [EnableIf("@!IsTestingInProgress")]
        [PropertyOrder(2)]
        public void StartTestingSpecialLevelsOnly()
        {
            if (isTestingInProgress) return;
            if (CurrentVariant == null)
            {
                Debug.LogError("Current Level Variant is not set or cannot be found.");
                return;
            }
            testResults.Clear();
            testingCoroutine = StartCoroutine(TestSpecialLevelsCoroutine());
        }

        [Button("Start Testing Normal Levels Only")]
        [EnableIf("@!IsTestingInProgress")]
        [PropertyOrder(3)]
        public void StartTestingNormalLevelsOnly()
        {
            if (isTestingInProgress) return;
            if (CurrentVariant == null)
            {
                Debug.LogError("Current Level Variant is not set or cannot be found.");
                return;
            }
            testResults.Clear();
            testingCoroutine = StartCoroutine(TestNormalLevelsCoroutine());
        }

        // --- New Test Level Range Method ---

        [Button("Test Level Range")]
        [EnableIf("@!IsTestingInProgress")]
        [PropertyOrder(4)]
        public void TestLevelRange(
            [MinValue(1), SuffixLabel("Start Level (Inclusive)", true)] int startLevel,
            [MinValue(1), SuffixLabel("End Level (Inclusive)", true)] int endLevel,
            [SuffixLabel("Test Special Levels?", true)] bool testSpecialLevels)
        {
            if (isTestingInProgress) return;
            if (CurrentVariant == null)
            {
                Debug.LogError("Current Level Variant is not set or cannot be found.");
                return;
            }
            if (startLevel > endLevel)
            {
                Debug.LogError($"Invalid range: Start Level ({startLevel}) cannot be greater than End Level ({endLevel}).");
                return;
            }

            testResults.Clear();
            testingCoroutine = StartCoroutine(TestLevelRangeCoroutine(startLevel, endLevel, testSpecialLevels));
        }

        // --- Single Level Test Methods ---

        [Button("Test Current Level")]
        [EnableIf("@!IsTestingInProgress && CurrentLevelDataSO != null")]
        [PropertyOrder(10)] // Put single tests later
        public void TestCurrentLevel()
        {
            if (isTestingInProgress) return;
            if (CurrentLevelDataSO == null)
            {
                Debug.LogError("No Current Level Data SO is selected.");
                return;
            }

            testResults.Clear();
            // Set the testing type based on the selected level
            isTestingSpecialLevels = (CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL);
            testingCoroutine = StartCoroutine(TestSingleLevelCoroutine(CurrentLevelDataSO));
        }

        [Button("Test Specific Level")]
        [EnableIf("@!IsTestingInProgress")]
        [PropertyOrder(11)]
        public void TestSpecificLevel(
            [MinValue(1), SuffixLabel("Level Number", true)] int levelNumber,
            [SuffixLabel("Is Special Level?", true)] bool isSpecialLevel = false)
        {
            if (isTestingInProgress) return;
            if (CurrentVariant == null)
            {
                Debug.LogError("Current Level Variant is not set or cannot be found.");
                return;
            }

            LevelDataSO levelToTest = null;
            if (isSpecialLevel)
                levelToTest = CurrentVariant.GetSpecialLevel(levelNumber);
            else
                levelToTest = CurrentVariant.GetNormalLevel(levelNumber);

            if (levelToTest == null)
            {
                Debug.LogError($"Could not find {(isSpecialLevel ? "Special" : "Normal")} Level {levelNumber} in the current variant.");
                return;
            }

            testResults.Clear();
            isTestingSpecialLevels = isSpecialLevel; // Set testing type flag
            testingCoroutine = StartCoroutine(TestSingleLevelCoroutine(levelToTest));
        }

        // --- Control Methods ---

        [Button("Cancel Testing")]
        [EnableIf("IsTestingInProgress")]
        [PropertyOrder(20)] // Put controls last
        public void CancelTesting()
        {
            if (!isTestingInProgress || testingCoroutine == null) return;

            Debug.Log("Cancelling testing...");
            StopCoroutine(testingCoroutine);
            testingCoroutine = null;
            // isTestingInProgress = false; // Set in the coroutine or finally block for safety

            // Ensure LevelSolver and its Instance are correctly implemented
            if (LevelSolver.Instance != null && LevelSolver.Instance.IsAISolverRunning())
            {
                LevelSolver.Instance.StopAISolver();
            }

            // Ensure LevelManager and its Instance are correctly implemented
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UnLoadLevel();
            }
            Debug.Log("Testing cancelled by user.");
            // Reset state immediately on cancel
            isTestingInProgress = false;
            currentTestingProgress = 0;
            currentLevelTestCount = 0;
            totalLevelsToTest = 0;
        }

        [Button("Export Results to CSV")]
        [EnableIf("@testResults != null && testResults.Count > 0")]
        [PropertyOrder(21)]
        public void ExportResultsToCSV()
        {
            if (testResults == null || testResults.Count == 0)
            {
                Debug.LogWarning("No test results available to export.");
                return;
            }
            SaveReportToCSV(csvExportPath);
        }

        // --- Coroutines ---

        private IEnumerator TestAllLevelsCoroutine()
        {
            isTestingInProgress = true;
            currentTestingProgress = 0;
            currentLevelTestCount = 0;
            Debug.Log($"Starting to test all levels for variant: {CurrentVariant.name}...");

            // Test Normal Levels first
            List<LevelDataSO> normalLevels = CurrentVariant.GetAllNormalLevels();
            List<LevelDataSO> specialLevels = CurrentVariant.GetAllSpecialLevels();
            int totalNormalLevels = normalLevels?.Count ?? 0;
            int totalSpecialLevels = specialLevels?.Count ?? 0;
            totalLevelsToTest = totalNormalLevels + totalSpecialLevels;

            if (totalLevelsToTest == 0)
            {
                Debug.LogWarning("No levels found in the variant to test.");
                isTestingInProgress = false;
                yield break;
            }

            // --- Test Normal Levels ---
            isTestingSpecialLevels = false;
            Debug.Log($"Starting to test {totalNormalLevels} normal levels...");
            if (normalLevels != null)
            {
                for (int i = 0; i < normalLevels.Count; i++)
                {
                    if (!isTestingInProgress) { Debug.Log("Testing aborted (normal)."); GenerateTestReport(); yield break; }

                    LevelDataSO levelToTest = normalLevels[i];
                    if (levelToTest == null)
                    {
                        Debug.LogWarning($"Skipping null normal level data at index {i}.");
                        continue;
                    }
                    currentTestingLevelIndex = levelToTest.level; // Use actual level number
                    currentLevelTestCount++;
                    currentTestingProgress = (int)(((float)currentLevelTestCount / totalLevelsToTest) * 100f);

                    Debug.Log($"Testing Normal Level {levelToTest.level} ({currentLevelTestCount}/{totalLevelsToTest})");
                    yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false)); // Don't generate report yet
                }
            }
            Debug.Log($"Completed testing of {totalNormalLevels} normal levels.");

            // --- Test Special Levels ---
            isTestingSpecialLevels = true;
            Debug.Log($"Starting to test {totalSpecialLevels} special levels...");
            if (specialLevels != null)
            {
                for (int i = 0; i < specialLevels.Count; i++)
                {
                    if (!isTestingInProgress) { Debug.Log("Testing aborted (special)."); GenerateTestReport(); yield break; }

                    LevelDataSO levelToTest = specialLevels[i];
                    if (levelToTest == null)
                    {
                        Debug.LogWarning($"Skipping null special level data at index {i}.");
                        continue;
                    }
                    currentTestingLevelIndex = levelToTest.level; // Use actual level number
                    currentLevelTestCount++;
                    currentTestingProgress = (int)(((float)currentLevelTestCount / totalLevelsToTest) * 100f);

                    Debug.Log($"Testing Special Level {levelToTest.level} ({currentLevelTestCount}/{totalLevelsToTest})");
                    yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false)); // Don't generate report yet
                }
            }
            Debug.Log($"Completed testing of {totalSpecialLevels} special levels.");

            // --- Final ---
            if (isTestingInProgress) // Check if not cancelled before final report
            {
                GenerateTestReport();
                isTestingInProgress = false;
                Debug.Log("Completed testing of all levels.");
            }
        }

        private IEnumerator TestNormalLevelsCoroutine(bool generateReport = true)
        {
            isTestingInProgress = true;
            isTestingSpecialLevels = false;
            currentTestingProgress = 0;
            currentLevelTestCount = 0;

            List<LevelDataSO> normalLevels = CurrentVariant.GetAllNormalLevels();
            totalLevelsToTest = normalLevels?.Count ?? 0;

            Debug.Log($"Starting to test {totalLevelsToTest} normal levels for variant: {CurrentVariant.name}...");

            if (totalLevelsToTest == 0)
            {
                Debug.LogWarning("No normal levels found to test.");
                isTestingInProgress = false;
                yield break;
            }

            if (normalLevels != null)
            {
                for (int i = 0; i < normalLevels.Count; i++)
                {
                    if (!isTestingInProgress)
                    {
                        Debug.Log("Testing aborted during normal levels.");
                        if (generateReport) GenerateTestReport();
                        yield break;
                    }

                    LevelDataSO levelToTest = normalLevels[i];
                    if (levelToTest == null)
                    {
                        Debug.LogWarning($"Skipping null normal level data at index {i}.");
                        continue;
                    }

                    currentTestingLevelIndex = levelToTest.level; // Actual level number
                    currentLevelTestCount++;
                    currentTestingProgress = (int)(((float)currentLevelTestCount / totalLevelsToTest) * 100f);

                    Debug.Log($"Testing Normal Level {levelToTest.level} ({currentLevelTestCount}/{totalLevelsToTest})");
                    yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false));
                }
            }

            if (isTestingInProgress) // Check if not cancelled
            {
                if (generateReport) GenerateTestReport();
                isTestingInProgress = false;
                Debug.Log($"Completed testing of {totalLevelsToTest} normal levels.");
            }
        }

        private IEnumerator TestSpecialLevelsCoroutine(bool generateReport = true)
        {
            isTestingInProgress = true;
            isTestingSpecialLevels = true;
            currentTestingProgress = 0;
            currentLevelTestCount = 0;

            List<LevelDataSO> specialLevels = CurrentVariant.GetAllSpecialLevels();
            totalLevelsToTest = specialLevels?.Count ?? 0;

            Debug.Log($"Starting to test {totalLevelsToTest} special levels for variant: {CurrentVariant.name}...");

            if (totalLevelsToTest == 0)
            {
                Debug.LogWarning("No special levels found to test.");
                isTestingInProgress = false;
                yield break;
            }

            if (specialLevels != null)
            {
                for (int i = 0; i < specialLevels.Count; i++)
                {
                    if (!isTestingInProgress)
                    {
                        Debug.Log("Testing aborted during special levels.");
                        if (generateReport) GenerateTestReport();
                        yield break;
                    }

                    LevelDataSO levelToTest = specialLevels[i];
                    if (levelToTest == null)
                    {
                        Debug.LogWarning($"Skipping null special level data at index {i}.");
                        continue;
                    }

                    currentTestingLevelIndex = levelToTest.level; // Actual level number
                    currentLevelTestCount++;
                    currentTestingProgress = (int)(((float)currentLevelTestCount / totalLevelsToTest) * 100f);

                    Debug.Log($"Testing Special Level {levelToTest.level} ({currentLevelTestCount}/{totalLevelsToTest})");
                    yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false));
                }
            }

            if (isTestingInProgress) // Check if not cancelled
            {
                if (generateReport) GenerateTestReport();
                isTestingInProgress = false;
                Debug.Log($"Completed testing of {totalLevelsToTest} special levels.");
            }
        }

        // --- New Coroutine for Range Testing ---
        private IEnumerator TestLevelRangeCoroutine(int startLevel, int endLevel, bool testSpecialLevels)
        {
            isTestingInProgress = true;
            isTestingSpecialLevels = testSpecialLevels;
            currentTestingProgress = 0;
            currentLevelTestCount = 0;
            string levelTypeName = testSpecialLevels ? "Special" : "Normal";

            Debug.Log($"Starting to test {levelTypeName} levels from {startLevel} to {endLevel} for variant: {CurrentVariant.name}...");

            // 1. Get all levels of the specified type
            List<LevelDataSO> allLevelsOfType = testSpecialLevels
                ? CurrentVariant.GetAllSpecialLevels()
                : CurrentVariant.GetAllNormalLevels();

            if (allLevelsOfType == null || allLevelsOfType.Count == 0)
            {
                Debug.LogWarning($"No {levelTypeName} levels found in the variant.");
                isTestingInProgress = false;
                yield break;
            }

            // 2. Filter levels within the specified range (inclusive)
            List<LevelDataSO> levelsToTestInRange = allLevelsOfType
                .Where(level => level != null && level.level >= startLevel && level.level <= endLevel)
                .OrderBy(level => level.level) // Ensure order for consistent testing
                .ToList();

            totalLevelsToTest = levelsToTestInRange.Count;

            if (totalLevelsToTest == 0)
            {
                Debug.LogWarning($"No {levelTypeName} levels found within the specified range [{startLevel}-{endLevel}].");
                isTestingInProgress = false;
                yield break;
            }

            Debug.Log($"Found {totalLevelsToTest} {levelTypeName} levels in range [{startLevel}-{endLevel}] to test.");

            // 3. Iterate and test the filtered levels
            for (int i = 0; i < levelsToTestInRange.Count; i++)
            {
                if (!isTestingInProgress)
                {
                    Debug.Log($"Testing aborted during range test ({levelTypeName}).");
                    GenerateTestReport(); // Generate report with partial results
                    yield break;
                }

                LevelDataSO levelToTest = levelsToTestInRange[i];
                // Null check already done in Where clause, but good practice
                if (levelToTest == null) continue;

                currentTestingLevelIndex = levelToTest.level; // Actual level number
                currentLevelTestCount++;
                currentTestingProgress = (int)(((float)currentLevelTestCount / totalLevelsToTest) * 100f);

                Debug.Log($"Testing {levelTypeName} Level {levelToTest.level} ({currentLevelTestCount}/{totalLevelsToTest})");
                yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false)); // Don't generate report after each
            }

            // 4. Final steps (if not cancelled)
            if (isTestingInProgress)
            {
                GenerateTestReport();
                isTestingInProgress = false;
                Debug.Log($"Completed testing of {totalLevelsToTest} {levelTypeName} levels in range [{startLevel}-{endLevel}].");
            }
        }


        private IEnumerator TestSingleLevelCoroutine(LevelDataSO levelToTest, bool generateReport = true)
        {
            if (levelToTest == null)
            {
                Debug.LogError("Cannot test a null level!");
                // If this was part of a larger test run, we should probably just skip
                // If it was the *only* level requested, then maybe set isTestingInProgress = false?
                // For now, just return. The calling coroutine will continue.
                yield break;
            }

            // --- Setup ---
            currentTestingLevelIndex = levelToTest.level; // Ensure index is set at the start
            GameplayManager.Instance.LoadLevel(levelToTest);
            float startTime = Time.realtimeSinceStartup;
            bool solverStarted = false;
            bool solverFinished = false;
            int finalMoveCount = 0;
            bool levelSolvedState = false;
            float elapsedTime = 0f;


            if (LevelSolver.Instance == null)
            {
                throw new NullReferenceException("LevelSolver.Instance is null. Cannot start AI solver.");
            }

            LevelSolver.Instance.StartAISolver();
            solverStarted = true;

            // Wait for solver to finish
            while (LevelSolver.Instance.IsAISolverRunning())
            {
                // Check for cancellation *inside* the loop
                if (!isTestingInProgress)
                {
                    Debug.Log($"Testing cancelled while solving level {levelToTest.level}. Stopping solver.");
                    if (LevelSolver.Instance.IsAISolverRunning())
                    {
                        LevelSolver.Instance.StopAISolver();
                    }
                    // Don't generate report here, the CancelTesting or calling coroutine handles it.
                    yield break; // Exit the coroutine for this level
                }
                yield return null; // More responsive than WaitForEndOfFrame
            }
            solverFinished = true; // Mark that the solver loop completed naturally

            elapsedTime = Time.realtimeSinceStartup - startTime;
            finalMoveCount = LevelSolver.Instance.SolverMovesCount; // Get moves *after* solver finishes

            // --- Verify ---
            levelSolvedState = IsLevelSolved(); // Check the board state



            // --- Record Result ---
            LevelTestResult result = new LevelTestResult
            {
                LevelNumber = levelToTest.level,
                IsSpecialLevel = levelToTest.levelType == LevelType.SPECIAL_LEVEL,
                MoveCount = finalMoveCount,
                TimeToSolve = elapsedTime
            };

            if (levelSolvedState)
            {
                result.Status = LevelTestStatus.Success;
                result.ErrorMessage = "";
            }
            else
            {
                result.Status = LevelTestStatus.Failed;
                if (!solverStarted)
                {
                    result.ErrorMessage = "Solver failed to start.";
                }
                else if (!solverFinished && !isTestingInProgress) // Failed because cancelled during solve
                {
                    result.ErrorMessage = "Test cancelled during solve.";
                    // We should already have exited via yield break if cancelled, but double-check logic
                }
                else if (!solverFinished) // Should not happen if loop exited normally
                {
                    result.ErrorMessage = "Solver did not finish correctly (unexpected state).";
                }
                else // Solver finished, but board state is not solved
                {
                    result.ErrorMessage = "Solver finished, but level condition not met.";
                }
            }

            testResults.Add(result);

            string statusText = result.Status.ToString();
            Debug.Log($"Level {(result.IsSpecialLevel ? "Special" : "Normal")} {result.LevelNumber} test result: {statusText}. " +
                      $"Moves: {result.MoveCount}, Time: {result.TimeToSolve:F2}s. " +
                      (string.IsNullOrEmpty(result.ErrorMessage) ? "" : $"Msg: {result.ErrorMessage}"));


            // --- Cleanup ---
            if (LevelManager.Instance != null && isTestingInProgress) // Check if not cancelled before unload
            {
                LevelManager.Instance.UnLoadLevel();
                yield return WaitForUtils.EndOfFrame; // Short delay after unload
            }


            // --- Final Report (if single run) ---
            if (generateReport && isTestingInProgress) // Generate report only if this was a single run AND not cancelled
            {
                GenerateTestReport();
                isTestingInProgress = false; // Mark testing as done
            }
            // If generateReport is true but testing was cancelled, the cancel logic handles the state.
        }


        // --- Helper Methods ---

        private void AddFailedResult(LevelDataSO levelData, float time, string errorMessage, int moves)
        {
            LevelTestResult result = new LevelTestResult
            {
                LevelNumber = levelData.level,
                IsSpecialLevel = levelData.levelType == LevelType.SPECIAL_LEVEL,
                Status = LevelTestStatus.Failed,
                ErrorMessage = errorMessage,
                MoveCount = moves,
                TimeToSolve = time
            };
            testResults.Add(result);
            Debug.LogError($"Level {(result.IsSpecialLevel ? "Special" : "Normal")} {result.LevelNumber} FAILED: {errorMessage}");
        }


        private bool IsLevelSolved()
        {
            if (ScrewManager.Instance == null || ScrewManager.Instance.Screws == null)
            {
                Debug.LogWarning("IsLevelSolved check failed: LevelManager or LevelScrews not available.");
                return false;
            }
            if (ScrewManager.Instance.Screws.Count == 0)
            {
                Debug.LogWarning("IsLevelSolved check: No screws found in the level.");
                return false;
            }


            foreach (BaseScrew screw in ScrewManager.Instance.Screws)
            {
                if (screw == null)
                {
                    Debug.LogWarning("IsLevelSolved check: Found a null screw reference in LevelScrews list.");
                    continue; 
                }

                if (screw.CurrentNutCount > 0)
                {
                    if (screw.CurrentNutCount != screw.CurrentCapacity || !screw.IsSorted())
                    {
                        return false; 
                    }
                }
            }
            return true;
        }


        private void GenerateTestReport()
        {
            if (testResults == null || testResults.Count == 0)
            {
                Debug.Log("No test results to generate a report.");
                return;
            }

            int totalTests = testResults.Count;
            int successCount = 0;
            int failedCount = 0;
            float totalMovesSuccess = 0;
            float totalTimeSuccess = 0;

            List<LevelTestResult> failedResults = new List<LevelTestResult>();

            foreach (LevelTestResult result in testResults)
            {
                switch (result.Status)
                {
                    case LevelTestStatus.Success:
                        successCount++;
                        totalMovesSuccess += result.MoveCount;
                        totalTimeSuccess += result.TimeToSolve;
                        break;
                    case LevelTestStatus.Failed:
                    case LevelTestStatus.Timeout: // Treat Timeout as Failed for reporting
                        failedCount++;
                        failedResults.Add(result);
                        break;
                }
            }

            float successRate = totalTests > 0 ? (float)successCount / totalTests * 100f : 0f;
            float averageMoves = successCount > 0 ? totalMovesSuccess / successCount : 0;
            float averageTime = successCount > 0 ? totalTimeSuccess / successCount : 0;

            StringBuilder report = new StringBuilder();
            report.AppendLine("\n=== LEVEL SOLVER TEST REPORT ==="); // Added newline for console clarity
            report.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Test Variant: {(CurrentVariant != null ? CurrentVariant.name : "Unknown")} (AB Type: {currentABType})");
            report.AppendLine($"Total Levels Tested: {totalTests}");
            report.AppendLine($"Success Rate: {successRate:F1}% ({successCount}/{totalTests})");
            report.AppendLine($"Failed: {failedCount}");
            report.AppendLine($"Average Moves (Successful Only): {averageMoves:F1}");
            report.AppendLine($"Average Time (Successful Only): {averageTime:F2}s");
            report.AppendLine();

            if (failedResults.Count > 0)
            {
                report.AppendLine("--- FAILED/TIMEOUT LEVELS ---");
                // Sort failed results for easier reading
                failedResults.Sort((a, b) => a.LevelNumber.CompareTo(b.LevelNumber));
                foreach (var failedResult in failedResults)
                {
                    report.AppendLine($"- {(failedResult.IsSpecialLevel ? "Special" : "Normal")} Level {failedResult.LevelNumber}: " +
                                      $"{failedResult.Status} (Moves: {failedResult.MoveCount}, Time: {failedResult.TimeToSolve:F2}s) - {failedResult.ErrorMessage}");
                }
            }
            else if (totalTests > 0)
            {
                report.AppendLine("--- All Tested Levels Passed ---");
            }
            else
            {
                report.AppendLine("--- No levels were tested in this run ---");
            }
            report.AppendLine("==================================\n");


            Debug.Log(report.ToString());

            if (exportReportToCSV && totalTests > 0) // Only export if requested AND results exist
            {
                SaveReportToCSV(csvExportPath);
            }
        }

        private void SaveReportToCSV(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("CSV export failed: File path is empty.");
                return;
            }
            if (testResults == null || testResults.Count == 0)
            {
                Debug.LogWarning("No test results to save to CSV.");
                return;
            }

            try
            {
                StringBuilder csv = new StringBuilder();

                // Header
                csv.AppendLine("Variant,Level Type,Level Number,Status,Move Count,Time (seconds),Error Message");

                // Data - Sort results by level number for consistent CSV output
                List<LevelTestResult> sortedResults = testResults.OrderBy(r => r.IsSpecialLevel).ThenBy(r => r.LevelNumber).ToList();

                foreach (LevelTestResult result in sortedResults)
                {
                    string variantName = CurrentVariant != null ? CurrentVariant.name : "Unknown";
                    string levelType = result.IsSpecialLevel ? "Special" : "Normal";
                    // Escape potential commas or quotes in the error message
                    string safeErrorMessage = $"\"{result.ErrorMessage?.Replace("\"", "\"\"") ?? ""}\""; // Handle null message

                    csv.AppendLine($"{variantName},{levelType},{result.LevelNumber},{result.Status}," +
                                  $"{result.MoveCount},{result.TimeToSolve:F2},{safeErrorMessage}");
                }

                // Determine full path (use persistentDataPath for safety in builds, works in editor too)
                // If you *specifically* want it in the Assets folder structure relative to the project:
                // string fullPath = Path.Combine(Application.dataPath, "..", filePath); // Go up one level from Assets
                // Ensure the path is project-relative if needed, otherwise persistentDataPath is safer
                string fullPath;
                if (Path.IsPathRooted(filePath))
                {
                    fullPath = filePath; // Use absolute path if provided
                }
                else
                {
                    // Assume relative to project root (outside Assets)
                    fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", filePath));
                }


                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"Created directory for CSV export: {directory}");
                }

                File.WriteAllText(fullPath, csv.ToString());

                Debug.Log($"CSV report saved to: {fullPath}");
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"IO Error saving CSV report to {filePath}: {ioEx.Message}. Is the file open in another program?\nStack Trace: {ioEx.StackTrace}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save CSV report to {filePath}: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }

        // --- Enums and Structs ---

        [System.Serializable]
        public class LevelTestResult
        {
            public int LevelNumber;
            public bool IsSpecialLevel;
            public LevelTestStatus Status;
            public string ErrorMessage;
            public int MoveCount;
            public float TimeToSolve;
        }

        // Timeout status remains, but won't be set by the timeout logic anymore.
        // It might be set if the solver itself reports a timeout internally, or for other reasons.
        public enum LevelTestStatus
        {
            Success,
            Failed,
            Timeout // Kept for potential future use or if solver reports it
        }
        #endregion
    }
}
#endif