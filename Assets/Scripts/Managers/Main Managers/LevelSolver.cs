using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelSolver
    {
        private static LevelSolver instance;
        [ShowInInspector] List<List<int>> currentLevelState;
        public List<BaseScrew> allScrews => LevelManager.Instance.LevelScrews;
        public float actionDelay = 0.1f;

        // Add number of moves counter
        [ShowInInspector] private int solverMovesCount = 0;
        public int SolverMovesCount => solverMovesCount;

        private ScrewNutSolver solver;
        private Coroutine solvingCoroutine;
        private bool isMoveComplete = false;

        // Booster related variables
        private BoosterActivatedScrew boosterScrew = null;
        private int boosterScrewIndex = -1;
        private int baseBoosterCapacity = 0;
        private int extendedBoosterCapacity = 0;
        private ExtraScrewBooster extraScrewBoosterLogic = null;

        // Hardcoded Surprise Nut ID
        private const int surpriseNutId = 31;

        public static LevelSolver Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LevelSolver();
                }
                return instance;
            }
        }
        public LevelSolver()
        {
            solver = new ScrewNutSolver();
            // Set optimized solver configuration for minimalist solving
            solver.SetHeuristicWeight(1.2f); // Slightly increase heuristic influence for more decisive moves
            solver.SetPrioritizeSurpriseHandling(true); // Prioritize handling surprise nuts
            solver.SetAggressiveEmptyPreference(true); // Aggressively prefer empty screws for flexibility

            // Register event handler only once
            NutTransferHelper.Instance.RegisterOnNutTransferComplete(OnNutTransferComplete); // Added null check
            extraScrewBoosterLogic = BoosterManager.Instance.GetBooster(BoosterIdConstant.EXTRA_SCREW) as ExtraScrewBooster; // Added null check
        }

        ~LevelSolver()
        {
            // Unregister event handler
            if (NutTransferHelper.Instance != null)
            {
                NutTransferHelper.Instance.DeRegisterOnNutTransferComplete(OnNutTransferComplete);
            }
            StopAISolver(); // Ensure coroutine is stopped

        }

        private void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            if (solvingCoroutine != null)
            {
                isMoveComplete = true;
            }
        }

        [Button]
        public void StartAISolver()
        {
            Debug.Log("AI Solver: Start requested. Stopping any existing solver first.");
            StopAISolver();
            ResetSolverState();
            solvingCoroutine = CoroutineRunner.Instance.CoroutineStart(SolveAndPlay_StepByStep());
        }

        // Add a public method to check if the AI solver is currently running
        public bool IsAISolverRunning()
        {
            return solvingCoroutine != null;
        }

        private void ResetSolverState()
        {
            solver = new ScrewNutSolver();

            isMoveComplete = false;

            // Reset move counter
            solverMovesCount = 0;

            if (ScrewSelectionHelper.Instance != null)
            {
                ScrewSelectionHelper.Instance.ClearSelection();
            }

            boosterScrew = null;
            boosterScrewIndex = -1;
            baseBoosterCapacity = 0;
            extendedBoosterCapacity = 0;

            if (currentLevelState != null)
            {
                currentLevelState.Clear();
                currentLevelState = null;
            }

            System.GC.Collect();

            Debug.Log("AI Solver: Solver state fully reset");
        }


        [Button]
        public void StopAISolver()
        {
            if (solvingCoroutine != null)
            {
                CoroutineRunner.Instance.CoroutineStop(solvingCoroutine);
                Debug.Log("AI Solver: Coroutine stopped.");
            }
            solvingCoroutine = null;
            if (ScrewSelectionHelper.Instance != null)
            {
                ScrewSelectionHelper.Instance.ClearSelection();
            }
        }

        IEnumerator SolveAndPlay_StepByStep()
        {
            Debug.Log("AI Solver (Step-by-Step): Starting...");
            int moveCount = 0;
            int maxMoves = 200; // Safety break
            int failedMovesCount = 0;
            int maxFailedMoves = 3; // Safety threshold
            float timeoutTimer = 0f;
            float maxTimeout = 5f; // Maximum seconds to wait for a response
            bool isSolvingActive = true; // Flag to track if the solver is still actively solving

            // Tracking for stuck detection
            int noProgressCounter = 0;
            const int MAX_NO_PROGRESS = 6; // Reduced to detect stuck faster
            string previousStateHash = "";
            float stuckTimer = 0f;
            const float STUCK_TIMEOUT = 8f; // Reduced to detect stuck faster

            // Track previous states and moves to detect cycles
            List<string> recentStateHashes = new List<string>();
            const int MAX_RECENT_STATES = 6; // Track last 6 states
            List<(int, int)> recentMoves = new List<(int, int)>();
            const int MAX_RECENT_MOVES = 5; // Track last 5 moves
            int cyclicMoveDetectionCounter = 0;
            const int MAX_CYCLIC_MOVE_COUNTER = 2; // More sensitive detection

            // New variables to handle special cases
            bool nearCompletion = false;
            int consecutiveBoosterAttempts = 0;
            const int MAX_BOOSTER_ATTEMPTS = 2; // Limit consecutive booster attempts
            int lastMoveFailures = 0;
            const int MAX_LAST_MOVE_FAILURES = 3; // Track failures in the last move situation

            // Wait for a frame before starting - helps prevent initial freeze
            yield return null;

            // Performance optimization: Pre-allocate some frequently used collections to reduce GC
            Dictionary<string, bool> cyclicMovePatterns = new Dictionary<string, bool>();
            WaitForEndOfFrame endOfFrameYield = new WaitForEndOfFrame();
            WaitForSeconds shortWait = new WaitForSeconds(0.05f);
            WaitForSeconds mediumWait = new WaitForSeconds(0.1f);
            WaitForSeconds longWait = new WaitForSeconds(0.3f);

            // Main solving loop
            while (moveCount < maxMoves && failedMovesCount < maxFailedMoves && isSolvingActive)
            {
                // Wait for the end of frame (more efficient than null yield)
                yield return endOfFrameYield;

                // 1. Get Current State and Capacities (includes booster info)
                GetCurrentLevelStateAndBoosterInfo();

                if (currentLevelState == null)
                {
                    Debug.LogError("AI Solver: Failed to read level state mid-solve. Will try again.");
                    yield return endOfFrameYield;
                    continue; // Try again
                }

                // Compute hashes only once
                string currentStateHash = SafeComputeHash();
                string colorDistributionHash = "";

                // Only compute color distribution hash when needed for cycle detection
                if (cyclicMoveDetectionCounter > 0 || recentStateHashes.Contains(currentStateHash))
                {
                    colorDistributionHash = SafeComputeColorDistributionHash();
                    Debug.Log($"Current state hash: {currentStateHash}, Color hash: {colorDistributionHash}");
                }
                else
                {
                    Debug.Log($"Current state hash: {currentStateHash}, Previous hash: {previousStateHash}");
                }

                // Check for exact state repetition (optimized handling)
                bool stateChanged = currentStateHash != previousStateHash;

                if (!stateChanged)
                {
                    noProgressCounter++;
                    stuckTimer += Time.deltaTime;

                    // Only log every few frames to reduce overhead
                    if (noProgressCounter % 3 == 0)
                    {
                        Debug.Log($"AI Solver: No state change detected. Counter: {noProgressCounter}, Timer: {stuckTimer:F1}s");
                    }

                    // If we're stuck for too long, try a different approach
                    if (noProgressCounter >= MAX_NO_PROGRESS || stuckTimer >= STUCK_TIMEOUT)
                    {
                        Debug.LogWarning($"AI Solver: No progress detected for {noProgressCounter} moves or {stuckTimer:F1} seconds. Likely stuck.");

                        // Try booster activation as a last resort if we're stuck
                        if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                        {
                            Debug.LogWarning("AI Solver: Stuck state detected. Attempting to activate booster as a last resort...");
                            SafeActivateBooster();
                            consecutiveBoosterAttempts++;
                            yield return mediumWait;  // Slightly shorter wait
                            noProgressCounter = 0;
                            stuckTimer = 0f;
                            continue;
                        }

                        yield break; // Exit this solving attempt
                    }
                }
                else // State has changed
                {
                    // We'll handle hash containment check only when there are reasonable hashes to check
                    if (recentStateHashes.Count > 0)
                    {
                        // Check for cyclical patterns (ABAABA or similar)
                        if (recentStateHashes.Contains(currentStateHash))
                        {
                            cyclicMoveDetectionCounter++;
                            Debug.Log($"AI Solver: Potential cycle detected - state hash seen recently. Counter: {cyclicMoveDetectionCounter}");

                            if (cyclicMoveDetectionCounter >= MAX_CYCLIC_MOVE_COUNTER)
                            {
                                Debug.LogWarning($"AI Solver: Cycle detected after {cyclicMoveDetectionCounter} repetitions. Trying to break cycle.");

                                // Try to use booster if available to break cycles
                                if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                                {
                                    Debug.LogWarning("AI Solver: Attempting to activate booster to break cycle...");
                                    SafeActivateBooster();
                                    consecutiveBoosterAttempts++;
                                    yield return mediumWait;  // Slightly shorter wait
                                    cyclicMoveDetectionCounter = 0;
                                    continue;
                                }
                                else
                                {
                                    Debug.LogWarning("AI Solver: Couldn't use booster to break cycle. Exiting solve attempt.");
                                    yield break; // Exit this solving attempt
                                }
                            }
                        }
                        else
                        {
                            cyclicMoveDetectionCounter = 0;
                        }
                    }

                    // Add current state to recent states (optimized with limited size)
                    recentStateHashes.Add(currentStateHash);
                    if (recentStateHashes.Count > MAX_RECENT_STATES)
                    {
                        recentStateHashes.RemoveAt(0);
                    }

                    // Reset stuck detection when state changes
                    if (noProgressCounter > 0 || stuckTimer > 0f)
                    {
                        Debug.Log("AI Solver: State changed, resetting stuck detection");
                        noProgressCounter = 0;
                        stuckTimer = 0f;
                    }
                    previousStateHash = currentStateHash;
                    consecutiveBoosterAttempts = 0; // Reset consecutive booster counter
                }

                // Get capacities (only once)
                int[] currentCapacities = GetCurrentEffectiveCapacities();

                // Optimize goal state checking by caching results
                bool isGoalState = SafeCheckGoalState(currentLevelState, currentCapacities);
                bool hasSurpriseNuts = SafeCheckForSurpriseNuts();

                // Track if we're near completion (will help with last-move issue)
                // Only check this when we're making progress or close to the end
                nearCompletion = !hasSurpriseNuts && (stateChanged || moveCount % 5 == 0 || lastMoveFailures > 0) ?
                    IsNearCompletion(currentLevelState, currentCapacities) : nearCompletion;

                // Reduce logging - only log every few steps
                if (moveCount % 5 == 0)
                {
                    // Debug state - helps with troubleshooting (but only log occasionally)
                    Debug.Log($"AI Solver: Current state has {currentLevelState.Count} screws");
                    for (int i = 0; i < currentLevelState.Count && i < allScrews.Count; i++)
                    {
                        if (allScrews[i] != null)
                        {
                            Debug.Log($"Screw {i} ({allScrews[i].name}): {string.Join(",", currentLevelState[i])} (Cap: {currentCapacities[i]})");
                        }
                    }
                }

                // If we've reached the goal state, break out of the solving loop
                if (isGoalState)
                {
                    Debug.Log($"AI Solver: Goal state reached after {moveCount} moves.");
                    solverMovesCount = moveCount; // Store final move count
                    break;
                }

                if (hasSurpriseNuts && moveCount % 5 == 0)
                {
                    Debug.Log("AI Solver: Detected surprise nuts in the level. Using optimized strategy.");
                }

                // Only yield once before heavy calculations
                yield return endOfFrameYield;

                bool isBoosterCurrentlyUsable = (boosterScrew != null && boosterScrew.IsExtended);

                (int, int)? nextMove = null;
                bool moveCalculated = false;
                bool forceRandomMove = cyclicMoveDetectionCounter >= 2; // Force random move if we're seeing cycles

                if (forceRandomMove)
                {
                    Debug.Log("AI Solver: Detected cycle pattern, attempting to find alternative moves...");
                    nextMove = SafeFindAlternativeMove(currentLevelState, currentCapacities, recentMoves);
                    moveCalculated = true;
                }
                else
                {
                    CoroutineRunner.Instance.CoroutineStart(GetNextMoveAsync(
                        currentLevelState,
                        currentCapacities,
                        boosterScrewIndex,
                        isBoosterCurrentlyUsable,
                        result =>
                        {
                            nextMove = result;
                            moveCalculated = true;
                        }
                    ));

                    // Wait for calculation to complete with timeout
                    float calcTimeout = 0f;
                    float maxCalcTimeout = 2f; // Reduced from 3f for faster response

                    while (!moveCalculated && calcTimeout < maxCalcTimeout)
                    {
                        calcTimeout += Time.deltaTime;
                        yield return endOfFrameYield; // More efficient
                    }

                    if (calcTimeout >= maxCalcTimeout && !moveCalculated)
                    {
                        Debug.LogWarning("AI Solver: Move calculation taking too long. Trying alternative approach.");
                        nextMove = null;
                    }
                }

                // If no move is found and we have surprise nuts, force a valid move
                if (!nextMove.HasValue && hasSurpriseNuts)
                {
                    Debug.Log("AI Solver: No move found by solver. Forcing a simple move with surprise nuts...");
                    nextMove = FindForcedMoveWithSurpriseNuts(currentLevelState, currentCapacities);

                    if (!nextMove.HasValue)
                    {
                        // If even forced move fails, try completely different approach
                        nextMove = FindAnyValidMove(currentLevelState, currentCapacities);
                    }
                }

                // If we're near completion but couldn't find a move, try more aggressively
                if (!nextMove.HasValue && nearCompletion)
                {
                    Debug.Log("AI Solver: Near completion but no move found. Trying more aggressive approach...");
                    // Try to find ANY valid move
                    nextMove = FindAnyValidMove(currentLevelState, currentCapacities);

                    // Track failures in this scenario
                    if (!nextMove.HasValue)
                    {
                        lastMoveFailures++;
                        if (lastMoveFailures >= MAX_LAST_MOVE_FAILURES)
                        {
                            Debug.LogWarning("AI Solver: Failed to find last moves multiple times. Trying booster...");

                            // Try using booster in last-move situation
                            if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse())
                            {
                                SafeActivateBooster();
                                yield return shortWait;
                                lastMoveFailures = 0;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        lastMoveFailures = 0;
                    }
                }

                // Only yield once before proceeding to move execution
                yield return endOfFrameYield;

                // 4. Check if a Move Was Found / Try activating booster
                if (!nextMove.HasValue)
                {
                    bool triedBoosterActivation = false;
                    if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                    {
                        Debug.LogWarning("AI Solver: No move found. Attempting to activate booster...");

                        // Safe booster activation without try-catch
                        SafeActivateBooster();
                        consecutiveBoosterAttempts++;
                        yield return shortWait;  // Reduced wait time

                        GetCurrentLevelStateAndBoosterInfo(); // Re-read state
                        currentCapacities = GetCurrentEffectiveCapacities(); // Recalculate capacities
                        isBoosterCurrentlyUsable = (boosterScrew != null && boosterScrew.IsExtended); // Update usability flag

                        // Try to get move after booster - using direct method since we already waited
                        nextMove = solver.GetNextMove(currentLevelState, currentCapacities, boosterScrewIndex, isBoosterCurrentlyUsable);

                        // Even after booster, try forced move if surprise nuts present
                        if (!nextMove.HasValue && hasSurpriseNuts)
                        {
                            Debug.Log("AI Solver: No move found after booster. Forcing a move with surprise nuts...");
                            nextMove = FindForcedMoveWithSurpriseNuts(currentLevelState, currentCapacities);
                        }

                        triedBoosterActivation = true;
                    }

                    if (!nextMove.HasValue)
                    {
                        // Still no move? Try a different approach
                        Debug.LogWarning($"AI Solver: No valid next move found{(triedBoosterActivation ? " even after activating booster" : "")}. Solver stuck after {moveCount} moves.");

                        // Last-ditch effort: Just find any legal move at all
                        nextMove = FindAnyValidMove(currentLevelState, currentCapacities);
                        if (!nextMove.HasValue)
                        {
                            yield break; // Exit this solving attempt
                        }
                    }
                }

                // Move execution
                int sourceIndex = nextMove.Value.Item1;
                int destIndex = nextMove.Value.Item2;

                if (sourceIndex < 0 || sourceIndex >= allScrews.Count || destIndex < 0 || destIndex >= allScrews.Count || allScrews[sourceIndex] == null || allScrews[destIndex] == null)
                {
                    Debug.LogError($"AI Solver: Solver proposed invalid move indices or involves null screw: {sourceIndex} -> {destIndex}. Trying different approach.");
                    yield break; // Exit this solving attempt
                }

                BaseScrew sourceBolt = allScrews[sourceIndex];
                BaseScrew destBolt = allScrews[destIndex];

                // Only log when needed
                if (moveCount % 5 == 0 || hasSurpriseNuts || nearCompletion)
                {
                    string sourceBoltInfo = $"{sourceBolt.name} (Nuts: {sourceBolt.CurrentNutCount}/{sourceBolt.ScrewNutsCapacity})";
                    string destBoltInfo = $"{destBolt.name} (Nuts: {destBolt.CurrentNutCount}/{destBolt.ScrewNutsCapacity})";
                    Debug.Log($"AI Solver: Attempting move from {sourceBoltInfo} -> {destBoltInfo}");
                }

                // Sanity check using actual game rules - NO TRY-CATCH
                bool canTransfer = SafeCheckCanTransfer(sourceBolt, destBolt);

                if (!canTransfer)
                {
                    Debug.LogError($"AI Solver: Solver proposed move {sourceIndex}->{destIndex} ({sourceBolt.name} -> {destBolt.name}), but CanTransferNuts is false! State inconsistency?");
                    failedMovesCount++;

                    if (failedMovesCount >= maxFailedMoves)
                    {
                        yield break; // Exit this solving attempt
                    }

                    yield return shortWait; // Short wait before trying again
                    continue; // Try again with a different move instead of stopping
                }

                if (moveCount % 10 == 0 || hasSurpriseNuts || nearCompletion)
                {
                    Debug.Log($"AI Move {moveCount + 1}: {sourceBolt.gameObject.name} ({sourceIndex}) -> {destBolt.gameObject.name} ({destIndex})");
                }

                if (ScrewSelectionHelper.Instance != null) ScrewSelectionHelper.Instance.ClearSelection();

                // Very short wait after clearing selection
                yield return new WaitForSeconds(0.02f);

                isMoveComplete = false; // Reset flag before triggering move

                // Simulate Clicks via ScrewSelectionHelper
                if (ScrewSelectionHelper.Instance != null)
                {
                    // Execute move without try-catch
                    bool moveSuccess = true;

                    // First click source
                    moveSuccess = SafeClickScrew(sourceBolt);
                    if (!moveSuccess)
                    {
                        failedMovesCount++;
                        if (failedMovesCount >= maxFailedMoves)
                        {
                            yield break;
                        }
                        yield return shortWait;
                        continue;
                    }

                    // Wait between clicks - reduced for performance
                    yield return new WaitForSeconds(actionDelay);

                    // Then click destination
                    moveSuccess = SafeClickScrew(destBolt);
                    if (!moveSuccess)
                    {
                        failedMovesCount++;
                        if (failedMovesCount >= maxFailedMoves)
                        {
                            yield break;
                        }
                        yield return shortWait;
                        continue;
                    }

                    // Reset timeout timer
                    timeoutTimer = 0f;

                    // Wait for move completion with timeout - outside any try-catch
                    while (!isMoveComplete && timeoutTimer < maxTimeout)
                    {
                        timeoutTimer += Time.deltaTime;
                        yield return endOfFrameYield; // More efficient
                    }

                    if (timeoutTimer >= maxTimeout)
                    {
                        Debug.LogWarning("AI Solver: Move completion timed out. Proceeding to next move.");
                        // Force continue anyway
                        isMoveComplete = true;
                    }

                    // If move was successful, reset failed moves counter
                    failedMovesCount = 0;

                    // Increment the solver moves counter for each successful move
                    solverMovesCount++;

                    // Add move to recent moves list for cycle detection
                    if (nextMove.HasValue)
                    {
                        recentMoves.Add(nextMove.Value);
                        if (recentMoves.Count > MAX_RECENT_MOVES)
                        {
                            recentMoves.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    Debug.LogError("AI Solver: ScrewSelectionHelper instance is null. Cannot simulate clicks.");
                    break;
                }

                // Reduced wait time after move completion for better performance
                yield return new WaitForSeconds(actionDelay * 1.2f);

                // Check for goal state again after move to ensure we don't miss completion
                // Optimize by only checking this if state has changed or near completion
                if (stateChanged || nearCompletion)
                {
                    GetCurrentLevelStateAndBoosterInfo(); // Re-read state
                    if (SafeCheckGoalState(currentLevelState, GetCurrentEffectiveCapacities()))
                    {
                        Debug.Log($"AI Solver: Goal state reached after move {moveCount}!");
                        solverMovesCount = moveCount + 1; // Store final move count
                        break;
                    }
                }

                moveCount++;
            } // End of while loop

            // Final cleanup
            if (moveCount >= maxMoves)
            {
                Debug.LogWarning($"AI Solver: Reached max move limit ({maxMoves})");
            }
            else if (failedMovesCount >= maxFailedMoves)
            {
                Debug.LogWarning($"AI Solver: Reached failed moves limit ({maxFailedMoves})");
            }
            else
            {
                Debug.Log($"AI Solver (Step-by-Step): Finished. Total moves: {solverMovesCount}");
            }

            // Important: Explicitly stop the solving coroutine
            StopAISolver();
        }

        // Optimize hash computation for better performance
        string SafeComputeHash()
        {
            if (currentLevelState == null || currentLevelState.Count == 0)
                return "empty";

            // Use a StringBuilder for faster string concatenation
            System.Text.StringBuilder sb = new System.Text.StringBuilder(currentLevelState.Count * 20);

            for (int i = 0; i < currentLevelState.Count; i++)
            {
                var screw = currentLevelState[i];
                sb.Append(screw.Count).Append(':');

                // Skip StringBuilder operations for empty screws
                if (screw.Count > 0)
                {
                    foreach (int nutType in screw)
                    {
                        sb.Append(nutType).Append(',');
                    }
                }
                sb.Append(';');
            }

            return sb.ToString().GetHashCode().ToString();
        }

        // Optimize hash computation for better performance
        string SafeComputeColorDistributionHash()
        {
            if (currentLevelState == null || currentLevelState.Count == 0)
                return "empty";

            // Count total nuts of each color
            Dictionary<int, int> colorCounts = new Dictionary<int, int>(8); // Preallocate with reasonable size

            // Also track which colors are at the top of each screw
            Dictionary<int, List<int>> topColorScrews = new Dictionary<int, List<int>>(8);

            // First, analyze the state
            for (int i = 0; i < currentLevelState.Count; i++)
            {
                var screw = currentLevelState[i];
                if (screw.Count == 0) continue;

                // Get top color
                int topColor = screw[screw.Count - 1];

                // Track which screws have this color on top
                if (!topColorScrews.ContainsKey(topColor))
                {
                    topColorScrews[topColor] = new List<int>(4); // Most colors won't be on more than a few screws
                }
                topColorScrews[topColor].Add(i);

                // Count all colors on this screw more efficiently
                foreach (int color in screw)
                {
                    if (!colorCounts.ContainsKey(color))
                    {
                        colorCounts[color] = 0;
                    }
                    colorCounts[color]++;
                }
            }

            // Optimized string building - reduce string creation overhead
            System.Text.StringBuilder sb = new System.Text.StringBuilder(colorCounts.Count * 15 + topColorScrews.Count * 20);

            // Add color counts
            foreach (var entry in colorCounts)
            {
                sb.Append("C").Append(entry.Key).Append(':').Append(entry.Value).Append(';');
            }

            // Add top color patterns - crucial for detecting cycles
            sb.Append("T:");
            foreach (var entry in topColorScrews)
            {
                sb.Append(entry.Key).Append(":[");

                // Sort the indices for consistent hash generation
                List<int> sortedIndices = entry.Value;
                sortedIndices.Sort();

                for (int i = 0; i < sortedIndices.Count; i++)
                {
                    sb.Append(sortedIndices[i]);
                    if (i < sortedIndices.Count - 1)
                        sb.Append(',');
                }
                sb.Append("];");
            }

            return sb.ToString().GetHashCode().ToString();
        }

        bool SafeCheckGoalState(List<List<int>> state, int[] capacities)
        {
            bool hasSurpriseNuts = false;
            foreach (var screw in state)
            {
                if (screw.Contains(surpriseNutId))
                {
                    hasSurpriseNuts = true;
                    break;
                }
            }

            if (hasSurpriseNuts)
            {
                return false;
            }

            for (int i = 0; i < state.Count; i++)
            {
                if (!IsScrewSortedOrEmpty(state[i], i, capacities))
                {
                    return false;
                }
            }
            return true;
        }

        bool SafeCheckForSurpriseNuts()
        {
            foreach (var screwState in currentLevelState)
            {
                if (screwState.Contains(surpriseNutId))
                {
                    return true;
                }
            }
            return false;
        }

        void SafeActivateBooster()
        {
            if (extraScrewBoosterLogic != null)
                extraScrewBoosterLogic.Use();
        }

        bool SafeCheckCanTransfer(BaseScrew source, BaseScrew dest)
        {
            return NutTransferHelper.Instance != null && NutTransferHelper.Instance.CanTransferNuts(source, dest);
        }

        bool SafeClickScrew(BaseScrew screw)
        {
            if (ScrewSelectionHelper.Instance != null && screw != null)
            {
                ScrewSelectionHelper.Instance.OnScrewClicked(screw);
                return true;
            }
            return false;
        }

        IEnumerator GetNextMoveAsync(List<List<int>> state, int[] capacities, int boosterIdx, bool isBoosterUsable, System.Action<(int, int)?> onComplete)
        {
            yield return null;

            (int, int)? result = null;
            try
            {
                Debug.Log("AI Solver: Calculating next best move (async)...");
                result = solver.GetNextMove(state, capacities, boosterIdx, isBoosterUsable);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting next move async: {e.Message}");
                result = null;
            }

            yield return null;

            onComplete(result);
        }

        (int, int)? FindForcedMoveWithSurpriseNuts(List<List<int>> state, int[] capacities)
        {
            bool IsValidGameMove(int sourceIdx, int destIdx)
            {
                if (sourceIdx < 0 || sourceIdx >= allScrews.Count ||
                    destIdx < 0 || destIdx >= allScrews.Count ||
                    allScrews[sourceIdx] == null || allScrews[destIdx] == null)
                {
                    return false;
                }

                BaseScrew sourceScrew = allScrews[sourceIdx];
                BaseScrew destScrew = allScrews[destIdx];

                return NutTransferHelper.Instance.CanTransferNuts(sourceScrew, destScrew);
            }

            Debug.Log("AI Solver: Searching for forced moves in a level with surprise nuts...");

            // HIGHEST PRIORITY: Move exposed surprise nuts to empty screws
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                if (sourceScrew[sourceScrew.Count - 1] == surpriseNutId)
                {
                    Debug.Log($"AI Solver: Found surprise nut at top of screw {sourceIdx}. Looking for empty screws...");
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (destIdx == sourceIdx) continue;
                        if (state[destIdx].Count == 0)
                        {
                            if (IsValidGameMove(sourceIdx, destIdx))
                            {
                                Debug.Log($"AI Solver: Forced move - Moving surprise nut from screw {sourceIdx} to empty screw {destIdx}");
                                return (sourceIdx, destIdx);
                            }
                            else
                            {
                                Debug.LogWarning($"AI Solver: Found surprise nut and empty screw, but move {sourceIdx}->{destIdx} is invalid according to game rules.");
                            }
                        }
                    }

                    Debug.Log("AI Solver: Surprise nut at top but no empty screws. Will try to create space.");
                }
            }

            // MEDIUM PRIORITY: Expose buried surprise nuts
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count <= 1) continue;

                // Find the highest surprise nut (closest to top)
                int highestSurpriseIndex = -1;
                for (int i = sourceScrew.Count - 2; i >= 0; i--) // Start from second from top
                {
                    if (sourceScrew[i] == surpriseNutId)
                    {
                        highestSurpriseIndex = i;
                        break;
                    }
                }

                // If we found a buried surprise nut
                if (highestSurpriseIndex >= 0)
                {
                    Debug.Log($"AI Solver: Found buried surprise nut in screw {sourceIdx} at position {highestSurpriseIndex}. Trying to expose it...");

                    // Try to move the top nut to create empty screws or build stacks
                    int topNut = sourceScrew[sourceScrew.Count - 1];

                    // First try: Move to empty screws
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (destIdx == sourceIdx) continue;
                        if (state[destIdx].Count == 0 && IsValidGameMove(sourceIdx, destIdx))
                        {
                            Debug.Log($"AI Solver: Forced move - Moving nut from screw with buried surprise {sourceIdx} to empty screw {destIdx}");
                            return (sourceIdx, destIdx);
                        }
                    }

                    // Second try: Move to matching screws
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (destIdx == sourceIdx) continue;

                        var destScrew = state[destIdx];
                        if (destScrew.Count > 0 &&
                            destScrew[destScrew.Count - 1] == topNut &&
                            IsValidGameMove(sourceIdx, destIdx))
                        {
                            Debug.Log($"AI Solver: Forced move - Moving {topNut} from screw with buried surprise {sourceIdx} to matching screw {destIdx}");
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }

            // LOWER PRIORITY: Create an empty screw
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count != 1) continue; // Only screws with exactly 1 nut

                // Skip if this is a surprise nut (handled by higher priority)
                if (sourceScrew[0] == surpriseNutId) continue;

                int nutToMove = sourceScrew[0];

                // Try to find a matching screw
                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (destIdx == sourceIdx) continue;

                    var destScrew = state[destIdx];
                    if (destScrew.Count > 0 &&
                        destScrew[destScrew.Count - 1] == nutToMove &&
                        IsValidGameMove(sourceIdx, destIdx))
                    {
                        Debug.Log($"AI Solver: Forced move - Moving single nut from {sourceIdx} to create empty screw");
                        return (sourceIdx, destIdx);
                    }
                }
            }

            Debug.Log("AI Solver: No surprise-specific moves found. Looking for ANY valid move...");
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (IsValidGameMove(sourceIdx, destIdx))
                    {
                        Debug.Log($"AI Solver: Forced move - Found valid move from {sourceIdx} to {destIdx}");
                        return (sourceIdx, destIdx);
                    }
                }
            }

            Debug.LogWarning("AI Solver: Could not find ANY valid move according to game rules. Level might be unsolvable.");
            return null;
        }

        int[] GetCurrentEffectiveCapacities()
        {
            if (allScrews == null) return new int[0];

            int[] capacities = new int[allScrews.Count];
            for (int i = 0; i < allScrews.Count; i++)
            {
                BaseScrew screw = allScrews[i];
                if (screw == null)
                {
                    capacities[i] = 0;
                    continue;
                }

                bool boosterIsExtended = (boosterScrew != null && boosterScrew.IsExtended);

                if (i == boosterScrewIndex && boosterIsExtended)
                {
                    capacities[i] = extendedBoosterCapacity;
                }
                else
                {
                    capacities[i] = screw.ScrewNutsCapacity;
                }
            }
            return capacities;
        }

        void GetCurrentLevelStateAndBoosterInfo()
        {
            boosterScrew = null;
            boosterScrewIndex = -1;
            baseBoosterCapacity = 0;
            extendedBoosterCapacity = 0;

            List<BaseScrew> screws = LevelManager.Instance?.LevelScrews;
            if (screws == null)
            {
                Debug.LogError("AI Solver: EditorLevelManager.Instance.LevelScrews is null!");
                currentLevelState = null;
                return;
            }

            currentLevelState = new List<List<int>>(screws.Count);

            for (int i = 0; i < screws.Count; i++)
            {
                BaseScrew screw = screws[i];
                if (screw == null)
                {
                    Debug.LogWarning($"AI Solver: Screw at index {i} is null.");
                    currentLevelState.Add(new List<int>());
                    continue;
                }

                List<BaseNut> nutsOnScrew = screw.NutsHolderStack?.nutsHolder;
                List<int> screwState = new List<int>(nutsOnScrew?.Count ?? 0);

                if (nutsOnScrew != null)
                {
                    foreach (BaseNut nut in nutsOnScrew)
                    {
                        if (nut == null)
                        {
                            Debug.LogWarning($"AI Solver: Found a null nut on screw {screw.gameObject.name} (index {i}).");
                            continue;
                        }
                        screwState.Add(nut.GetNutColorType());
                    }
                }
                currentLevelState.Add(screwState);

                if (screw is BoosterActivatedScrew bScrew)
                {
                    boosterScrew = bScrew;
                    boosterScrewIndex = i;
                    baseBoosterCapacity = bScrew.ScrewNutsCapacity;
                    extendedBoosterCapacity = bScrew.CurrentScrewCapacity;
                }
            }
        }

        // Check if a screw is sorted or empty
        private static bool IsScrewSortedOrEmpty(List<int> screw, int screwIndex, int[] screwCapacities)
        {
            if (screw.Count == 0) return true;

            if (screwIndex < 0 || screwIndex >= screwCapacities.Length)
            {
                Debug.LogError($"AI Solver Internal: Invalid screw index ({screwIndex}) passed to IsScrewSortedOrEmpty.");
                return false;
            }
            int capacity = screwCapacities[screwIndex];

            // If not at capacity, it's not considered fully sorted
            if (screw.Count != capacity) return false;

            int firstNutType = screw[0];
            // Surprise nuts can never be considered sorted
            if (firstNutType == surpriseNutId) return false;

            // All nuts must be the same color to be sorted
            foreach (int nutType in screw)
            {
                if (nutType != firstNutType) return false;
            }
            return true;
        }

        // Check if a nut can be moved from one screw to another
        private bool CanMoveNut(List<List<int>> state, int sourceIndex, int destIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= state.Count || destIndex < 0 || destIndex >= state.Count)
            {
                return false;
            }

            List<int> sourceScrew = state[sourceIndex];
            List<int> destScrew = state[destIndex];

            if (sourceScrew.Count == 0) return false; // Cannot move from empty

            // Get capacities for this check
            int[] capacities = GetCurrentEffectiveCapacities();

            // Check destination capacity
            if (destIndex >= capacities.Length)
            {
                Debug.LogError($"AI Solver Internal: destIndex {destIndex} out of bounds for capacities (size {capacities.Length}).");
                return false;
            }
            if (destScrew.Count >= capacities[destIndex]) return false; // Destination full

            // Check if moving TO booster when it's not usable for this solve run
            if (destIndex == boosterScrewIndex && !(boosterScrew != null && boosterScrew.IsExtended))
            {
                return false; // Cannot move to inactive booster
            }

            // Color match check
            int nutToMove = sourceScrew[sourceScrew.Count - 1];

            // Empty destination check
            if (destScrew.Count == 0)
            {
                // Empty destination is always valid, but add extra validation for surprise nuts
                // Surprise nuts should only go to empty screws
                if (nutToMove == surpriseNutId)
                {
                    return true; // This is the perfect case for surprise nuts
                }
                return true; // Empty destination is always valid
            }

            // Color matching for regular nuts - surprise nuts always need an empty screw
            int topNutOnDest = destScrew[destScrew.Count - 1];

            // Surprise nut rules:
            // 1. Surprise nuts can NEVER stack on other nuts (surprise or regular)
            if (nutToMove == surpriseNutId)
                return false; // Surprise nuts can only go to empty screws

            // 2. Regular nuts can't stack on surprise nuts
            if (topNutOnDest == surpriseNutId)
                return false;

            // Regular color matching for normal nuts
            return nutToMove == topNutOnDest;
        }

        (int, int)? SafeFindAlternativeMove(List<List<int>> state, int[] capacities, List<(int, int)> recentMoves)
        {
            try
            {
                // Get all possible moves
                List<(int, int, int)> possibleMoves = new List<(int, int, int)>();

                // Find all valid moves with a score
                for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
                {
                    if (state[sourceIdx].Count == 0) continue;

                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (sourceIdx == destIdx) continue;

                        // Check if this move is valid according to game rules
                        if (CanMoveNut(state, sourceIdx, destIdx))
                        {
                            // Calculate a score for this move (lower is better)
                            int score = 0;

                            // Check if this move was made recently (higher penalty)
                            bool madeRecently = false;
                            foreach (var recentMove in recentMoves)
                            {
                                // Check for same move or reverse move (A→B, B→A)
                                if ((sourceIdx == recentMove.Item1 && destIdx == recentMove.Item2) ||
                                    (sourceIdx == recentMove.Item2 && destIdx == recentMove.Item1))
                                {
                                    score += 100; // High penalty
                                    madeRecently = true;
                                }
                            }

                            // If move wasn't made recently, it's a better candidate
                            if (!madeRecently)
                            {
                                score -= 20; // Bonus for new moves
                            }

                            // Bonus for moves that create empty screws
                            if (state[sourceIdx].Count == 1)
                            {
                                score -= 30; // Creating empty screws is good
                            }

                            // Bonus for moves that build homogeneous stacks
                            if (state[destIdx].Count > 0)
                            {
                                int topColor = state[destIdx][state[destIdx].Count - 1];
                                if (topColor != surpriseNutId)
                                {
                                    // Count same-colored nuts in destination
                                    int sameColorCount = 0;
                                    foreach (int nut in state[destIdx])
                                    {
                                        if (nut == topColor) sameColorCount++;
                                    }

                                    // If destination is mostly same color
                                    if (sameColorCount > state[destIdx].Count * 0.7f)
                                    {
                                        score -= 15; // Bonus for building homogeneous stacks
                                    }
                                }
                            }

                            // Special handling for surprise nuts - highest priority
                            int nutToMove = state[sourceIdx][state[sourceIdx].Count - 1];
                            if (nutToMove == surpriseNutId && state[destIdx].Count == 0)
                            {
                                score -= 200; // Extreme priority to move surprise nuts
                            }

                            possibleMoves.Add((sourceIdx, destIdx, score));
                        }
                    }
                }

                if (possibleMoves.Count == 0)
                {
                    Debug.Log("AI Solver: No alternative moves found.");
                    return null;
                }

                // Sort by score (lower is better)
                possibleMoves.Sort((a, b) => a.Item3.CompareTo(b.Item3));

                // Return the best scoring move
                var selectedMove = (possibleMoves[0].Item1, possibleMoves[0].Item2);
                Debug.Log($"AI Solver: Selected alternative move {selectedMove.Item1} → {selectedMove.Item2} with score {possibleMoves[0].Item3}");
                return selectedMove;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error finding alternative move: {e.Message}");
                return null;
            }
        }

        // New helper method to check if the level is near completion
        private bool IsNearCompletion(List<List<int>> state, int[] capacities)
        {
            // First check if there are any surprise nuts left
            if (SafeCheckForSurpriseNuts())
            {
                return false; // Not near completion if we still have surprise nuts
            }

            // Count completed screws
            int completedScrews = 0;
            int totalScrews = state.Count;

            for (int i = 0; i < state.Count; i++)
            {
                if (IsScrewSortedOrEmpty(state[i], i, capacities))
                {
                    completedScrews++;
                }
            }

            // If 75% or more screws are completed, consider it near completion
            return (float)completedScrews / totalScrews >= 0.75f;
        }

        // New helper method to find any valid move as a last resort
        private (int, int)? FindAnyValidMove(List<List<int>> state, int[] capacities)
        {
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        Debug.Log($"AI Solver: Last resort - Found valid move from {sourceIdx} to {destIdx}");
                        return (sourceIdx, destIdx);
                    }
                }
            }

            return null;
        }
    }
}
