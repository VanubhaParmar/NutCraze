using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class LevelTestManager : SerializedManager<LevelTestManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private ABTestType currentABType;
        [SerializeField] private LevelVariantSO currentVariant;
        [SerializeField] private LevelDataSO currentLevelDataSO;

        [ShowInInspector, ReadOnly] private bool isTestingInProgress = false;
        [ShowInInspector, ReadOnly] private int currentTestingLevelIndex = 0;
        [ShowInInspector, ReadOnly] private bool isTestingSpecialLevels = false;
        [ShowInInspector, ReadOnly] private int totalLevelsToTest = 0;
        [ShowInInspector, ReadOnly] private int currentTestingProgress = 0;

        [ShowInInspector, ReadOnly] private List<LevelTestResult> testResults = new List<LevelTestResult>();
        [ShowInInspector] private float testTimeoutSeconds = 30f;
        [ShowInInspector] private bool exportReportToCSV = true;
        [ShowInInspector] private string csvExportPath = "LevelSolverTest_Report.csv";

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
                if (currentVariant == null)
                    ResourceManager.Instance.TryGetLevelVariant(currentABType, out currentVariant);
                return currentVariant;
            }
        }

        public bool IsTestingInProgress => isTestingInProgress;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            GameplayManager.Instance.StartEditorGamePlay();
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region LEVEL SOLVER TESTING METHODS
        [Button("Start Testing All Levels")]
        public void StartTestingAllLevels()
        {
            if (isTestingInProgress)
                return;

            testResults.Clear();
            testingCoroutine = StartCoroutine(TestAllLevelsCoroutine());
        }

        [Button("Start Testing Special Levels Only")]
        public void StartTestingSpecialLevelsOnly()
        {
            if (isTestingInProgress)
                return;
            testResults.Clear();
            testingCoroutine = StartCoroutine(TestSpecialLevelsCoroutine());
        }

        [Button("Start Testing Normal Levels Only")]
        public void StartTestingNormalLevelsOnly()
        {
            if (isTestingInProgress)
                return;

            testResults.Clear();
            testingCoroutine = StartCoroutine(TestNormalLevelsCoroutine());
        }

        [Button("Test Current Level")]
        public void TestCurrentLevel()
        {
            if (isTestingInProgress)
                return;

            if (CurrentLevelDataSO == null)
                return;

            testResults.Clear();
            testingCoroutine = StartCoroutine(TestSingleLevelCoroutine(CurrentLevelDataSO));
        }

        [Button("Test Specific Level")]
        public void TestSpecificLevel(int levelNumber, bool isSpecialLevel = false)
        {
            if (isTestingInProgress)
                return;

            LevelDataSO levelToTest = null;
            if (isSpecialLevel)
                levelToTest = CurrentVariant.GetSpecialLevel(levelNumber);
            else
                levelToTest = CurrentVariant.GetNormalLevel(levelNumber);

            if (levelToTest == null)
                return;

            testResults.Clear();
            testingCoroutine = StartCoroutine(TestSingleLevelCoroutine(levelToTest));
        }

        [Button("Cancel Testing")]
        public void CancelTesting()
        {
            if (!isTestingInProgress || testingCoroutine == null)
                return;

            StopCoroutine(testingCoroutine);
            testingCoroutine = null;
            isTestingInProgress = false;

            if (LevelSolver.Instance.IsAISolverRunning())
                LevelSolver.Instance.StopAISolver();

            LevelManager.Instance.UnLoadLevel();
        }

        [Button("Export Results to CSV")]
        public void ExportResultsToCSV()
        {
            if (testResults == null || testResults.Count == 0)
                return;
            SaveReportToCSV(csvExportPath);
        }

        private IEnumerator TestAllLevelsCoroutine()
        {
            isTestingInProgress = true;
            currentTestingProgress = 0;

            Debug.Log("Starting to test all levels (normal and special)...");

            yield return StartCoroutine(TestNormalLevelsCoroutine(false));

            yield return StartCoroutine(TestSpecialLevelsCoroutine(false));

            GenerateTestReport();

            isTestingInProgress = false;
            Debug.Log("Completed testing of all levels.");
        }

        private IEnumerator TestNormalLevelsCoroutine(bool generateReport = true)
        {
            isTestingInProgress = true;
            isTestingSpecialLevels = false;
            currentTestingProgress = 0;

            List<LevelDataSO> normalLevels = CurrentVariant.GetAllNormalLevels();
            totalLevelsToTest = normalLevels.Count;

            Debug.Log($"Starting to test {totalLevelsToTest} normal levels...");

            for (int i = 0; i < normalLevels.Count; i++)
            {
                currentTestingLevelIndex = i + 1;
                currentTestingProgress = (i * 100) / totalLevelsToTest;

                LevelDataSO levelToTest = normalLevels[i];
                Debug.Log($"Testing Normal Level {levelToTest.level} ({i + 1}/{totalLevelsToTest})");

                yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false));
            }

            if (generateReport)
            {
                GenerateTestReport();
            }

            if (generateReport)
            {
                isTestingInProgress = false;
            }

            Debug.Log($"Completed testing of {totalLevelsToTest} normal levels.");
        }

        private IEnumerator TestSpecialLevelsCoroutine(bool generateReport = true)
        {
            isTestingInProgress = true;
            isTestingSpecialLevels = true;
            currentTestingProgress = 0;

            List<LevelDataSO> specialLevels = CurrentVariant.GetAllSpecialLevels();
            totalLevelsToTest = specialLevels.Count;

            Debug.Log($"Starting to test {totalLevelsToTest} special levels...");

            for (int i = 0; i < specialLevels.Count; i++)
            {
                currentTestingLevelIndex = i + 1;
                currentTestingProgress = (i * 100) / totalLevelsToTest;

                LevelDataSO levelToTest = specialLevels[i];
                Debug.Log($"Testing Special Level {levelToTest.level} ({i + 1}/{totalLevelsToTest})");

                yield return StartCoroutine(TestSingleLevelCoroutine(levelToTest, false));
            }

            if (generateReport)
            {
                GenerateTestReport();
            }

            if (generateReport)
            {
                isTestingInProgress = false;
            }

            Debug.Log($"Completed testing of {totalLevelsToTest} special levels.");
        }

        private IEnumerator TestSingleLevelCoroutine(LevelDataSO levelToTest, bool generateReport = true)
        {
            if (levelToTest == null)
            {
                Debug.LogError("Cannot test a null level!");
                yield break;
            }

            GameplayManager.Instance.LoadLevel(levelToTest);

            yield return new WaitForSeconds(0.5f);

            if (LevelManager.Instance.LevelScrews == null || LevelManager.Instance.LevelScrews.Count == 0)
            {
                LevelTestResult failResult = new LevelTestResult
                {
                    LevelNumber = levelToTest.level,
                    IsSpecialLevel = levelToTest.levelType == LevelType.SPECIAL_LEVEL,
                    Status = LevelTestStatus.Failed,
                    ErrorMessage = "Failed to load level correctly (no screws found)",
                    MoveCount = 0,
                    TimeToSolve = 0
                };

                testResults.Add(failResult);
                Debug.LogError($"Failed to load level {levelToTest.level} correctly!");

                yield return new WaitForSeconds(0.5f);
                LevelManager.Instance.UnLoadLevel();
                yield break;
            }

            float startTime = Time.realtimeSinceStartup;
            bool wasRunning = false;

            if (LevelSolver.Instance != null)
            {
                LevelSolver.Instance.StartAISolver();
                wasRunning = true;
            }
            else
            {
                Debug.LogError("LevelSolver instance not found! Cannot test level.");

                LevelTestResult failResult = new LevelTestResult
                {
                    LevelNumber = levelToTest.level,
                    IsSpecialLevel = levelToTest.levelType == LevelType.SPECIAL_LEVEL,
                    Status = LevelTestStatus.Failed,
                    ErrorMessage = "LevelSolver instance not found",
                    MoveCount = 0,
                    TimeToSolve = 0
                };

                testResults.Add(failResult);

                yield return new WaitForSeconds(0.5f);
                LevelManager.Instance.UnLoadLevel();
                yield break;
            }

            float elapsedTime = 0f;
            bool solverCompleted = false;

            while (elapsedTime < testTimeoutSeconds)
            {
                elapsedTime = Time.realtimeSinceStartup - startTime;

                if (!LevelSolver.Instance.IsAISolverRunning() && wasRunning)
                {
                    solverCompleted = true;
                    break;
                }

                yield return new WaitForSeconds(0.5f);
            }

            if (LevelSolver.Instance.IsAISolverRunning())
            {
                LevelSolver.Instance.StopAISolver();
            }

            bool isLevelSolved = IsLevelSolved();

            LevelTestResult result = new LevelTestResult
            {
                LevelNumber = levelToTest.level,
                IsSpecialLevel = levelToTest.levelType == LevelType.SPECIAL_LEVEL,
                MoveCount = LevelSolver.Instance.SolverMovesCount,
                TimeToSolve = elapsedTime
            };

            if (!solverCompleted)
            {
                result.Status = LevelTestStatus.Timeout;
                result.ErrorMessage = "Solver exceeded maximum time allowed";
            }
            else if (!isLevelSolved)
            {
                result.Status = LevelTestStatus.Failed;
                result.ErrorMessage = "Solver stopped but level not completed";
            }
            else
            {
                result.Status = LevelTestStatus.Success;
                result.ErrorMessage = "";
            }

            testResults.Add(result);

            string statusText = result.Status == LevelTestStatus.Success ? "SUCCESS" :
                               (result.Status == LevelTestStatus.Timeout ? "TIMEOUT" : "FAILED");

            Debug.Log($"Level {(result.IsSpecialLevel ? "Special" : "Normal")} {result.LevelNumber} test result: {statusText}. " +
                      $"Moves: {result.MoveCount}, Time: {result.TimeToSolve:F2}s");

            yield return new WaitForSeconds(0.5f);
            LevelManager.Instance.UnLoadLevel();

            if (generateReport)
            {
                GenerateTestReport();
                isTestingInProgress = false;
            }

            yield return new WaitForSeconds(0.5f);
        }

        private bool IsLevelSolved()
        {
            if (LevelManager.Instance.LevelScrews == null || LevelManager.Instance.LevelScrews.Count == 0)
                return false;

            foreach (BaseScrew screw in LevelManager.Instance.LevelScrews)
            {
                if (screw == null) continue;

                if (screw.CurrentNutCount == 0)
                    continue;
                if (!screw.IsSorted())
                    return false;
                if (screw.CurrentNutCount > 0 && screw.CurrentNutCount != screw.ScrewNutsCapacity)
                    return false;
            }

            return true;
        }
      

        private void GenerateTestReport()
        {
            if (testResults == null || testResults.Count == 0)
                return;

            int totalTests = testResults.Count;
            int successCount = 0;
            int failedCount = 0;
            int timeoutCount = 0;
            float totalMoves = 0;
            float totalTime = 0;

            foreach (LevelTestResult result in testResults)
            {
                switch (result.Status)
                {
                    case LevelTestStatus.Success:
                        successCount++;
                        totalMoves += result.MoveCount;
                        totalTime += result.TimeToSolve;
                        break;
                    case LevelTestStatus.Failed:
                        failedCount++;
                        break;
                    case LevelTestStatus.Timeout:
                        timeoutCount++;
                        break;
                }
            }

            float successRate = (float)successCount / totalTests * 100f;
            float averageMoves = successCount > 0 ? totalMoves / successCount : 0;
            float averageTime = successCount > 0 ? totalTime / successCount : 0;

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== LEVEL SOLVER TEST REPORT ===");
            report.AppendLine($"Total Levels Tested: {totalTests}");
            report.AppendLine($"Success Rate: {successRate:F1}% ({successCount}/{totalTests})");
            report.AppendLine($"Failed: {failedCount}, Timeouts: {timeoutCount}");
            report.AppendLine($"Average Moves (for successful solutions): {averageMoves:F1}");
            report.AppendLine($"Average Time (for successful solutions): {averageTime:F2}s");
            report.AppendLine();

            report.AppendLine("=== FAILED/TIMEOUT LEVELS ===");
            for (int i = 0; i < testResults.Count; i++)
            {
                LevelTestResult result = testResults[i];
                if (result.Status != LevelTestStatus.Success)
                {
                    report.AppendLine($"{(result.IsSpecialLevel ? "Special" : "Normal")} Level {result.LevelNumber}: " +
                                    $"{result.Status} - {result.ErrorMessage}");
                }
            }

            Debug.Log(report.ToString());

            if (exportReportToCSV)
            {
                SaveReportToCSV(csvExportPath);
            }
        }

        private void SaveReportToCSV(string filePath)
        {
            try
            {
                StringBuilder csv = new StringBuilder();

                csv.AppendLine("Level Type,Level Number,Status,Move Count,Time (seconds),Error Message");

                foreach (LevelTestResult result in testResults)
                {
                    string levelType = result.IsSpecialLevel ? "Special" : "Normal";

                    csv.AppendLine($"{levelType},{result.LevelNumber},{result.Status}," +
                                  $"{result.MoveCount},{result.TimeToSolve:F2},\"{result.ErrorMessage}\"");
                }

                string fullPath = Path.Combine(Application.persistentDataPath, filePath);
                File.WriteAllText(fullPath, csv.ToString());

                Debug.Log($"CSV report saved to: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save CSV report: {e.Message}");
            }
        }

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

        public enum LevelTestStatus
        {
            Success,
            Failed,
            Timeout
        }
        #endregion
    }
}
