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
        public float actionDelay = 0.15f;

        [ShowInInspector] private int solverMovesCount = 0;
        public int SolverMovesCount => solverMovesCount;

        private ScrewNutSolver solver;
        private Coroutine solvingCoroutine;

        private BoosterActivatedScrew boosterScrew = null;
        private int boosterScrewIndex = -1;
        private int extendedBoosterCapacity = 0;
        private ExtraScrewBooster extraScrewBoosterLogic = null;

        private const int surpriseNutId = 31;

        const int MAX_RECENT_STATES = 6;
        const int MAX_NO_PROGRESS = 6;
        const int MAX_RECENT_MOVES = 5;
        const int MAX_CYCLIC_MOVE_COUNTER = 2;
        const int MAX_BOOSTER_ATTEMPTS = 2;
        const int MAX_LAST_MOVE_FAILURES = 3;
        const float STUCK_TIMEOUT = 8f;

        WaitForEndOfFrame endOfFrameYield = new WaitForEndOfFrame();
        WaitForSeconds shortWait = new WaitForSeconds(0.05f);
        WaitForSeconds mediumWait = new WaitForSeconds(0.1f);
        WaitForSeconds longWait = new WaitForSeconds(0.3f);


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
            solver.SetHeuristicWeight(1.2f);
            solver.SetPrioritizeSurpriseHandling(true);
            solver.SetAggressiveEmptyPreference(true);
            extraScrewBoosterLogic = BoosterManager.Instance.GetBooster(BoosterIdConstant.EXTRA_SCREW) as ExtraScrewBooster;
        }

        ~LevelSolver()
        {
            StopAISolver();
        }

        [Button]
        public void StartAISolver()
        {
            Debug.Log("AI Solver: Start requested. Stopping any existing solver first.");
            StopAISolver();
            ResetSolverState();
            solvingCoroutine = CoroutineRunner.Instance.CoroutineStart(SolveAndPlay_StepByStep());
        }

        public bool IsAISolverRunning()
        {
            return solvingCoroutine != null;
        }

        private void ResetSolverState()
        {
            solver = new ScrewNutSolver();

            solverMovesCount = 0;

            ScrewSelectionHelper.Instance.ClearSelection();

            boosterScrew = null;
            boosterScrewIndex = -1;
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
            ScrewSelectionHelper.Instance.ClearSelection();
        }

        IEnumerator SolveAndPlay_StepByStep()
        {
            Debug.Log("AI Solver (Step-by-Step): Starting...");
            int moveCount = 0;
            int maxMoves = 200;

            int noProgressCounter = 0;
            string previousStateHash = "";
            float stuckTimer = 0f;

            List<string> recentStateHashes = new List<string>();
            List<(int, int)> recentMoves = new List<(int, int)>();
            int cyclicMoveDetectionCounter = 0;

            int consecutiveBoosterAttempts = 0;
            int lastMoveFailures = 0;

            yield return null;

            while (moveCount < maxMoves)
            {
                yield return endOfFrameYield;

                GetCurrentLevelStateAndBoosterInfo();

                if (currentLevelState == null)
                {
                    Debug.LogError("AI Solver: Failed to read level state mid-solve. Will try again.");
                    yield return endOfFrameYield;
                    continue;
                }

                string currentStateHash = SafeComputeHash();
                string colorDistributionHash = "";

                if (cyclicMoveDetectionCounter > 0 || recentStateHashes.Contains(currentStateHash))
                {
                    colorDistributionHash = SafeComputeColorDistributionHash();
                    Debug.Log($"Current state hash: {currentStateHash}, Color hash: {colorDistributionHash}");
                }
                else
                {
                    Debug.Log($"Current state hash: {currentStateHash}, Previous hash: {previousStateHash}");
                }

                bool stateChanged = currentStateHash != previousStateHash;

                if (!stateChanged)
                {
                    noProgressCounter++;
                    stuckTimer += Time.deltaTime;

                    if (noProgressCounter % 3 == 0)
                    {
                        Debug.Log($"AI Solver: No state change detected. Counter: {noProgressCounter}, Timer: {stuckTimer:F1}s");
                    }

                    if (noProgressCounter >= MAX_NO_PROGRESS || stuckTimer >= STUCK_TIMEOUT)
                    {
                        Debug.LogWarning($"AI Solver: No progress detected for {noProgressCounter} moves or {stuckTimer:F1} seconds. Likely stuck.");

                        if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                        {
                            Debug.LogWarning("AI Solver: Stuck state detected. Attempting to activate booster as a last resort...");
                            SafeActivateBooster();
                            consecutiveBoosterAttempts++;
                            yield return mediumWait;
                            noProgressCounter = 0;
                            stuckTimer = 0f;
                            continue;
                        }

                        yield break;
                    }
                }
                else
                {
                    if (recentStateHashes.Count > 0)
                    {
                        if (recentStateHashes.Contains(currentStateHash))
                        {
                            cyclicMoveDetectionCounter++;
                            Debug.Log($"AI Solver: Potential cycle detected - state hash seen recently. Counter: {cyclicMoveDetectionCounter}");

                            if (cyclicMoveDetectionCounter >= MAX_CYCLIC_MOVE_COUNTER)
                            {
                                Debug.LogWarning($"AI Solver: Cycle detected after {cyclicMoveDetectionCounter} repetitions. Trying to break cycle.");

                                if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                                {
                                    Debug.LogWarning("AI Solver: Attempting to activate booster to break cycle...");
                                    SafeActivateBooster();
                                    consecutiveBoosterAttempts++;
                                    yield return mediumWait;
                                    cyclicMoveDetectionCounter = 0;
                                    continue;
                                }
                                else
                                {
                                    Debug.LogWarning("AI Solver: Couldn't use booster to break cycle. Exiting solve attempt.");
                                    yield break;
                                }
                            }
                        }
                        else
                        {
                            cyclicMoveDetectionCounter = 0;
                        }
                    }

                    recentStateHashes.Add(currentStateHash);
                    if (recentStateHashes.Count > MAX_RECENT_STATES)
                    {
                        recentStateHashes.RemoveAt(0);
                    }

                    if (noProgressCounter > 0 || stuckTimer > 0f)
                    {
                        Debug.Log("AI Solver: State changed, resetting stuck detection");
                        noProgressCounter = 0;
                        stuckTimer = 0f;
                    }
                    previousStateHash = currentStateHash;
                    consecutiveBoosterAttempts = 0;
                }

                int[] currentCapacities = GetCurrentEffectiveCapacities();

                bool isGoalState = SafeCheckGoalState(currentLevelState, currentCapacities);
                bool hasSurpriseNuts = SafeCheckForSurpriseNuts();

                if (isGoalState)
                {
                    Debug.Log($"AI Solver: Goal state reached after {moveCount} moves.");
                    solverMovesCount = moveCount;
                    break;
                }

                if (hasSurpriseNuts && moveCount % 5 == 0)
                {
                    Debug.Log("AI Solver: Detected surprise nuts in the level. Using optimized strategy.");
                }

                yield return endOfFrameYield;

                bool isBoosterCurrentlyUsable = (boosterScrew != null && boosterScrew.IsExtended);

                (int, int)? nextMove = null;
                bool moveCalculated = false;
                bool forceRandomMove = cyclicMoveDetectionCounter >= 2;

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

                    float calcTimeout = 0f;
                    float maxCalcTimeout = 2f;

                    while (!moveCalculated && calcTimeout < maxCalcTimeout)
                    {
                        calcTimeout += Time.deltaTime;
                        yield return endOfFrameYield;
                    }

                    if (calcTimeout >= maxCalcTimeout && !moveCalculated)
                    {
                        Debug.LogWarning("AI Solver: Move calculation taking too long. Trying alternative approach.");
                        nextMove = null;
                    }
                }

                if (!nextMove.HasValue && hasSurpriseNuts)
                {
                    Debug.Log("AI Solver: No move found by solver. Forcing a simple move with surprise nuts...");
                    nextMove = FindForcedMoveWithSurpriseNuts(currentLevelState, currentCapacities);

                    if (!nextMove.HasValue)
                    {
                        nextMove = FindAnyValidMove(currentLevelState, currentCapacities);
                    }
                }

                if (!nextMove.HasValue)
                {
                    Debug.Log("AI Solver: Near completion but no move found. Trying more aggressive approach...");
                    nextMove = FindAnyValidMove(currentLevelState, currentCapacities);

                    if (!nextMove.HasValue)
                    {
                        lastMoveFailures++;
                        if (lastMoveFailures >= MAX_LAST_MOVE_FAILURES)
                        {
                            Debug.LogWarning("AI Solver: Failed to find last moves multiple times. Trying booster...");

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

                yield return endOfFrameYield;

                if (!nextMove.HasValue)
                {
                    bool triedBoosterActivation = false;
                    if (boosterScrew != null && extraScrewBoosterLogic != null && !boosterScrew.IsExtended && extraScrewBoosterLogic.CanUse() && consecutiveBoosterAttempts < MAX_BOOSTER_ATTEMPTS)
                    {
                        Debug.LogWarning("AI Solver: No move found. Attempting to activate booster...");

                        SafeActivateBooster();
                        consecutiveBoosterAttempts++;
                        yield return shortWait;

                        GetCurrentLevelStateAndBoosterInfo();
                        currentCapacities = GetCurrentEffectiveCapacities();
                        isBoosterCurrentlyUsable = (boosterScrew != null && boosterScrew.IsExtended);

                        nextMove = solver.GetNextMove(currentLevelState, currentCapacities, boosterScrewIndex, isBoosterCurrentlyUsable);

                        if (!nextMove.HasValue && hasSurpriseNuts)
                        {
                            Debug.Log("AI Solver: No move found after booster. Forcing a move with surprise nuts...");
                            nextMove = FindForcedMoveWithSurpriseNuts(currentLevelState, currentCapacities);
                        }

                        triedBoosterActivation = true;
                    }

                    if (!nextMove.HasValue)
                    {
                        Debug.LogWarning($"AI Solver: No valid next move found{(triedBoosterActivation ? " even after activating booster" : "")}. Solver stuck after {moveCount} moves.");

                        nextMove = FindAnyValidMove(currentLevelState, currentCapacities);
                        if (!nextMove.HasValue)
                        {
                            yield break;
                        }
                    }
                }

                int sourceIndex = nextMove.Value.Item1;
                int destIndex = nextMove.Value.Item2;

                if (sourceIndex < 0 || sourceIndex >= allScrews.Count || destIndex < 0 || destIndex >= allScrews.Count || allScrews[sourceIndex] == null || allScrews[destIndex] == null)
                {
                    Debug.LogError($"AI Solver: Solver proposed invalid move indices or involves null screw: {sourceIndex} -> {destIndex}. Trying different approach.");
                    yield break;
                }

                BaseScrew sourceBolt = allScrews[sourceIndex];
                BaseScrew destBolt = allScrews[destIndex];

                bool canTransfer = NutTransferHelper.Instance.CanTransferNuts(sourceBolt, destBolt);

                if (!canTransfer)
                {
                    Debug.LogError($"AI Solver: Solver proposed move {sourceIndex}->{destIndex} ({sourceBolt.name} -> {destBolt.name}), but CanTransferNuts is false! State inconsistency?");
                    yield return shortWait;
                    continue;
                }

                ScrewSelectionHelper.Instance.ClearSelection();

                yield return new WaitForSeconds(0.02f);

                ScrewSelectionHelper.Instance.OnScrewClicked(sourceBolt);
                yield return new WaitForSeconds(actionDelay);
                ScrewSelectionHelper.Instance.OnScrewClicked(destBolt);

                solverMovesCount++;

                if (nextMove.HasValue)
                {
                    recentMoves.Add(nextMove.Value);
                    if (recentMoves.Count > MAX_RECENT_MOVES)
                    {
                        recentMoves.RemoveAt(0);
                    }
                }
                yield return new WaitForSeconds(actionDelay * 1.2f);

                if (stateChanged)
                {
                    GetCurrentLevelStateAndBoosterInfo();
                    if (SafeCheckGoalState(currentLevelState, GetCurrentEffectiveCapacities()))
                    {
                        Debug.Log($"AI Solver: Goal state reached after move {moveCount}!");
                        solverMovesCount = moveCount + 1;
                        break;
                    }
                }

                moveCount++;
            }

            if (moveCount >= maxMoves)
            {
                Debug.LogWarning($"AI Solver: Reached max move limit ({maxMoves})");
            }
            else
            {
                Debug.Log($"AI Solver (Step-by-Step): Finished. Total moves: {solverMovesCount}");
            }

            StopAISolver();
        }

        string SafeComputeHash()
        {
            if (currentLevelState == null || currentLevelState.Count == 0)
                return "empty";

            System.Text.StringBuilder sb = new System.Text.StringBuilder(currentLevelState.Count * 20);

            for (int i = 0; i < currentLevelState.Count; i++)
            {
                var screw = currentLevelState[i];
                sb.Append(screw.Count).Append(':');

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

        string SafeComputeColorDistributionHash()
        {
            if (currentLevelState == null || currentLevelState.Count == 0)
                return "empty";

            Dictionary<int, int> colorCounts = new Dictionary<int, int>(8);
            Dictionary<int, List<int>> topColorScrews = new Dictionary<int, List<int>>(8);

            for (int i = 0; i < currentLevelState.Count; i++)
            {
                var screw = currentLevelState[i];
                if (screw.Count == 0) continue;

                int topColor = screw[screw.Count - 1];

                if (!topColorScrews.ContainsKey(topColor))
                {
                    topColorScrews[topColor] = new List<int>(4);
                }
                topColorScrews[topColor].Add(i);

                foreach (int color in screw)
                {
                    if (!colorCounts.ContainsKey(color))
                    {
                        colorCounts[color] = 0;
                    }
                    colorCounts[color]++;
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(colorCounts.Count * 15 + topColorScrews.Count * 20);

            foreach (var entry in colorCounts)
            {
                sb.Append("C").Append(entry.Key).Append(':').Append(entry.Value).Append(';');
            }

            sb.Append("T:");
            foreach (var entry in topColorScrews)
            {
                sb.Append(entry.Key).Append(":[");

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
                return false;

            for (int i = 0; i < state.Count; i++)
            {
                if (!IsScrewSortedOrEmpty(state[i], i, capacities))
                    return false;
            }
            return true;
        }

        bool SafeCheckForSurpriseNuts()
        {
            foreach (var screwState in currentLevelState)
            {
                if (screwState.Contains(surpriseNutId))
                    return true;
            }
            return false;
        }

        void SafeActivateBooster()
        {
            if (extraScrewBoosterLogic != null)
                extraScrewBoosterLogic.Use();
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

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count <= 1) continue;

                int highestSurpriseIndex = -1;
                for (int i = sourceScrew.Count - 2; i >= 0; i--)
                {
                    if (sourceScrew[i] == surpriseNutId)
                    {
                        highestSurpriseIndex = i;
                        break;
                    }
                }

                if (highestSurpriseIndex >= 0)
                {
                    Debug.Log($"AI Solver: Found buried surprise nut in screw {sourceIdx} at position {highestSurpriseIndex}. Trying to expose it...");

                    int topNut = sourceScrew[sourceScrew.Count - 1];

                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (destIdx == sourceIdx) continue;
                        if (state[destIdx].Count == 0 && IsValidGameMove(sourceIdx, destIdx))
                        {
                            Debug.Log($"AI Solver: Forced move - Moving nut from screw with buried surprise {sourceIdx} to empty screw {destIdx}");
                            return (sourceIdx, destIdx);
                        }
                    }

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

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count != 1) continue;

                if (sourceScrew[0] == surpriseNutId) continue;

                int nutToMove = sourceScrew[0];

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
                        screwState.Add(nut.GetRealNutColorType());
                    }
                }
                currentLevelState.Add(screwState);

                if (screw is BoosterActivatedScrew bScrew)
                {
                    boosterScrew = bScrew;
                    boosterScrewIndex = i;
                    extendedBoosterCapacity = bScrew.CurrentScrewCapacity;
                }
            }
        }

        private static bool IsScrewSortedOrEmpty(List<int> screw, int screwIndex, int[] screwCapacities)
        {
            if (screw.Count == 0) return true;

            if (screwIndex < 0 || screwIndex >= screwCapacities.Length)
            {
                Debug.LogError($"AI Solver Internal: Invalid screw index ({screwIndex}) passed to IsScrewSortedOrEmpty.");
                return false;
            }
            int capacity = screwCapacities[screwIndex];

            if (screw.Count != capacity) return false;

            int firstNutType = screw[0];
            if (firstNutType == surpriseNutId) return false;

            foreach (int nutType in screw)
            {
                if (nutType != firstNutType) return false;
            }
            return true;
        }

        private bool CanMoveNut(List<List<int>> state, int sourceIndex, int destIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= state.Count || destIndex < 0 || destIndex >= state.Count)
            {
                return false;
            }

            List<int> sourceScrew = state[sourceIndex];
            List<int> destScrew = state[destIndex];

            if (sourceScrew.Count == 0) return false;

            int[] capacities = GetCurrentEffectiveCapacities();

            if (destIndex >= capacities.Length)
            {
                Debug.LogError($"AI Solver Internal: destIndex {destIndex} out of bounds for capacities (size {capacities.Length}).");
                return false;
            }
            if (destScrew.Count >= capacities[destIndex]) return false;

            if (destIndex == boosterScrewIndex && !(boosterScrew != null && boosterScrew.IsExtended))
            {
                return false;
            }

            int nutToMove = sourceScrew[sourceScrew.Count - 1];

            if (destScrew.Count == 0)
            {
                if (nutToMove == surpriseNutId)
                {
                    return true;
                }
                return true;
            }

            int topNutOnDest = destScrew[destScrew.Count - 1];

            if (nutToMove == surpriseNutId)
                return false;

            if (topNutOnDest == surpriseNutId)
                return false;

            return nutToMove == topNutOnDest;
        }

        (int, int)? SafeFindAlternativeMove(List<List<int>> state, int[] capacities, List<(int, int)> recentMoves)
        {
            try
            {
                List<(int, int, int)> possibleMoves = new List<(int, int, int)>();

                for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
                {
                    if (state[sourceIdx].Count == 0) continue;

                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (sourceIdx == destIdx) continue;

                        if (CanMoveNut(state, sourceIdx, destIdx))
                        {
                            int score = 0;
                            bool madeRecently = false;
                            foreach (var recentMove in recentMoves)
                            {
                                if ((sourceIdx == recentMove.Item1 && destIdx == recentMove.Item2) ||
                                    (sourceIdx == recentMove.Item2 && destIdx == recentMove.Item1))
                                {
                                    score += 100;
                                    madeRecently = true;
                                }
                            }

                            if (!madeRecently)
                            {
                                score -= 20;
                            }

                            if (state[sourceIdx].Count == 1)
                            {
                                score -= 30;
                            }

                            if (state[destIdx].Count > 0)
                            {
                                int topColor = state[destIdx][state[destIdx].Count - 1];
                                if (topColor != surpriseNutId)
                                {
                                    int sameColorCount = 0;
                                    foreach (int nut in state[destIdx])
                                    {
                                        if (nut == topColor) sameColorCount++;
                                    }

                                    if (sameColorCount > state[destIdx].Count * 0.7f)
                                    {
                                        score -= 15;
                                    }
                                }
                            }

                            int nutToMove = state[sourceIdx][state[sourceIdx].Count - 1];
                            if (nutToMove == surpriseNutId && state[destIdx].Count == 0)
                            {
                                score -= 200;
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

                possibleMoves.Sort((a, b) => a.Item3.CompareTo(b.Item3));

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