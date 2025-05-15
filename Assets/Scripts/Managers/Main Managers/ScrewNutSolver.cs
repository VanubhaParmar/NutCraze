using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class ScrewNutSolver
    {
        public class StateNode
        {
            public int HeuristicCost { get; }
            public int PathCost { get; }
            public int TotalCost => HeuristicCost + PathCost;
            public List<List<int>> State { get; }
            public StateNode Parent { get; }
            public (int, int)? Move { get; }
            public StateNode(int h, int p, List<List<int>> s, StateNode parent, (int, int)? m) { HeuristicCost = h; PathCost = p; State = s; Parent = parent; Move = m; }
        }

        public readonly struct Solution
        {
            public bool IsSolved { get; }
            public List<(int, int)> SolutionSteps { get; }
            public Solution(bool solved, List<(int, int)> steps) { IsSolved = solved; SolutionSteps = steps; }
        }

        private int[] screwCapacities;
        private int boosterScrewIndex = -1;
        private bool isBoosterCurrentlyUsableForSolve = false;

        private float heuristicWeight = 2.0f;
        private int maxVisitedStates = 50000;
        private bool useAggressiveEmptyPreference = false;
        private bool prioritizeSurpriseHandling = true;

        private HashSet<string> recentMoves = new HashSet<string>();
        private int maxRecentMovesToTrack = 10;
        private List<(int, int)> lastMoves = new List<(int, int)>();

        private const int surpriseNutId = 31;

        public void SetHeuristicWeight(float weight)
        {
            heuristicWeight = Mathf.Clamp(weight, 0.1f, 3.0f);
        }

        public void SetMaxVisitedStates(int maxStates)
        {
            maxVisitedStates = Mathf.Max(10000, maxStates);
        }

        public void SetAggressiveEmptyPreference(bool aggressive)
        {
            useAggressiveEmptyPreference = aggressive;
        }

        public void SetPrioritizeSurpriseHandling(bool prioritize)
        {
            prioritizeSurpriseHandling = prioritize;
        }

        public Solution SolveLevel(List<List<int>> startState, int numScrews, int[] capacities, int boosterIdx, bool isBoosterCurrentlyUsable)
        {
            this.isBoosterCurrentlyUsableForSolve = isBoosterCurrentlyUsable;
            this.screwCapacities = capacities;
            this.boosterScrewIndex = boosterIdx;

            var queue = new SimplePriorityQueue<StateNode, int>();
            var visited = new Dictionary<string, int>();

            int startHeuristic = CalculateHeuristic(startState);
            var startNode = new StateNode(startHeuristic, 0, startState, null, null);
            string startStateString = StateToString(startState);

            if (IsGoalState(startState, capacities))
            {
                Debug.Log("AI Solver: Start state is already goal state.");
                return new Solution(true, new List<(int, int)>());
            }

            queue.Enqueue(startNode, startNode.TotalCost);
            visited.Add(startStateString, 0);

            StateNode goalNode = null;
            int visitedCount = 0;
            int currentMaxVisitedStates = maxVisitedStates;

            int shortCircuitCount = Mathf.Min(currentMaxVisitedStates, 25000);
            int iterationsPerFrame = 1000;
            int iterationsSinceYield = 0;

            List<(int, int)> lastFewMoves = new List<(int, int)>();
            Dictionary<string, bool> recentMovePatterns = new Dictionary<string, bool>();
            bool isNearGoal = IsNearGoalState(startState, capacities);

            if (isNearGoal)
            {
                currentMaxVisitedStates = Mathf.Min(currentMaxVisitedStates * 2, 100000);
                shortCircuitCount = Mathf.Min(shortCircuitCount * 2, 50000);
                Debug.Log("AI Solver: Near goal state detected. Increasing search depth to find optimal solution.");
            }

            while (queue.Count > 0 && visitedCount < currentMaxVisitedStates)
            {
                if (iterationsSinceYield >= iterationsPerFrame)
                {
                    iterationsSinceYield = 0;

                    if (visitedCount > shortCircuitCount && goalNode == null)
                    {
                        Debug.Log($"AI Solver: Early termination after {visitedCount} states. No solution found yet.");
                        break;
                    }
                }

                StateNode currentNode = queue.Dequeue();
                visitedCount++;
                iterationsSinceYield++;

                if (IsGoalState(currentNode.State, capacities))
                {
                    goalNode = currentNode;
                    Debug.Log($"AI Solver: Goal state found after visiting {visitedCount} states. Path cost: {goalNode.PathCost}");
                    break;
                }

                GenerateNextStatesInChunksWithCyclePrevention(currentNode, queue, visited, 50, lastFewMoves, recentMovePatterns);
            }

            if ((visitedCount >= currentMaxVisitedStates || visitedCount >= shortCircuitCount) && goalNode == null)
            {
                Debug.Log($"AI Solver: Search limit ({visitedCount}/{currentMaxVisitedStates} visited states) reached. No solution found within limits.");
            }

            if (goalNode != null)
            {
                List<(int, int)> path = GeneratePath(goalNode);
                return new Solution(true, path);
            }
            else
            {
                return new Solution(false, null);
            }
        }

        private void GenerateNextStatesInChunksWithCyclePrevention(
            StateNode currentNode,
            SimplePriorityQueue<StateNode, int> queue,
            Dictionary<string, int> visited,
            int maxStatesPerNode,
            List<(int, int)> moveHistory,
            Dictionary<string, bool> recentMovePatterns)
        {
            List<List<int>> currentState = currentNode.State;
            int numScrews = currentState.Count;
            int currentPathCost = currentNode.PathCost;
            int statesGenerated = 0;

            (int, int)? lastMove = currentNode.Move;
            if (lastMove.HasValue)
            {
                moveHistory.Add(lastMove.Value);

                if (moveHistory.Count > 7)
                {
                    moveHistory.RemoveAt(0);
                }

                if (moveHistory.Count >= 2)
                {
                    var moveA = moveHistory[moveHistory.Count - 1];
                    var moveB = moveHistory[moveHistory.Count - 2];

                    if (moveA.Item1 == moveB.Item2 && moveA.Item2 == moveB.Item1)
                    {
                        string pattern = $"{moveB.Item2}-{moveB.Item1}-shuttle";
                        if (!recentMovePatterns.ContainsKey(pattern))
                        {
                            recentMovePatterns[pattern] = true;
                        }
                    }
                }

                if (moveHistory.Count >= 3)
                {
                    var move1 = moveHistory[moveHistory.Count - 1];
                    var move2 = moveHistory[moveHistory.Count - 2];
                    var move3 = moveHistory[moveHistory.Count - 3];

                    if (move1.Item1 == move3.Item2 && move1.Item2 != move3.Item1)
                    {
                        string pattern = $"{move3.Item1}-{move3.Item2}-{move2.Item2}-cycle";
                        if (!recentMovePatterns.ContainsKey(pattern))
                        {
                            recentMovePatterns[pattern] = true;
                        }
                    }
                }

                if (recentMovePatterns.Count > 15)
                {
                    recentMovePatterns.Clear();
                }
            }

            List<(int sourceIdx, int destIdx, int cycleRisk)> candidateMoves = new List<(int, int, int)>();

            for (int sourceIdx = 0; sourceIdx < numScrews; sourceIdx++)
            {
                for (int destIdx = 0; destIdx < numScrews; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(currentState, sourceIdx, destIdx))
                    {
                        int cycleRisk = 0;

                        if (lastMove.HasValue && lastMove.Value.Item1 == destIdx && lastMove.Value.Item2 == sourceIdx)
                        {
                            cycleRisk += 100;
                        }

                        string shuttlePattern = $"{sourceIdx}-{destIdx}-shuttle";
                        if (recentMovePatterns.ContainsKey(shuttlePattern))
                        {
                            cycleRisk += 50;
                        }

                        if (currentState[sourceIdx].Count > 0 && currentState[destIdx].Count > 0)
                        {
                            int sourceTop = currentState[sourceIdx][currentState[sourceIdx].Count - 1];
                            int destTop = currentState[destIdx][currentState[destIdx].Count - 1];

                            if (sourceTop == destTop && sourceTop != surpriseNutId)
                            {
                                bool sourceHomogeneous = IsScrewMostlyHomogeneous(currentState[sourceIdx], sourceTop);
                                bool destHomogeneous = IsScrewMostlyHomogeneous(currentState[destIdx], destTop);

                                if (sourceHomogeneous && destHomogeneous)
                                {
                                    cycleRisk += 30;
                                }
                            }
                        }

                        candidateMoves.Add((sourceIdx, destIdx, cycleRisk));
                    }
                }
            }

            candidateMoves.Sort((a, b) => a.cycleRisk.CompareTo(b.cycleRisk));

            foreach (var moveInfo in candidateMoves)
            {
                if (statesGenerated >= maxStatesPerNode) break;

                int sourceIdx = moveInfo.sourceIdx;
                int destIdx = moveInfo.destIdx;
                int cycleRisk = moveInfo.cycleRisk;

                List<List<int>> nextState = PerformMove(currentState, sourceIdx, destIdx);
                string nextStateString = StateToString(nextState);

                int nextPathCost = currentPathCost + 1;
                if (cycleRisk > 0)
                {
                    nextPathCost += Mathf.Min(3, cycleRisk / 20);
                }

                if (visited.TryGetValue(nextStateString, out int existingPathCost))
                {
                    if (existingPathCost <= nextPathCost) continue;
                    else visited[nextStateString] = nextPathCost;
                }
                else
                {
                    visited.Add(nextStateString, nextPathCost);
                }

                int nextHeuristic = CalculateHeuristic(nextState);

                var nextNode = new StateNode(nextHeuristic, nextPathCost, nextState, currentNode, (sourceIdx, destIdx));
                queue.Enqueue(nextNode, nextNode.TotalCost);
                statesGenerated++;
            }
        }

        public (int, int)? GetNextMove(List<List<int>> currentState, int[] capacities, int boosterIdx, bool isBoosterCurrentlyUsable)
        {
            int originalMaxVisitedStates = maxVisitedStates;
            float originalHeuristicWeight = heuristicWeight;

            try
            {
                maxVisitedStates = Mathf.Min(maxVisitedStates, 15000);
                heuristicWeight *= 1.1f;

                bool hasSurpriseNuts = false;
                bool surpriseStuck = false;
                bool emptyScrewAvailable = false;

                foreach (var screw in currentState)
                {
                    if (screw.Count == 0)
                    {
                        emptyScrewAvailable = true;
                        continue;
                    }

                    foreach (int nutType in screw)
                    {
                        if (nutType == surpriseNutId)
                        {
                            hasSurpriseNuts = true;
                            break;
                        }
                    }

                    if (screw.Count > 0 && screw[screw.Count - 1] == surpriseNutId && !emptyScrewAvailable)
                    {
                        surpriseStuck = true;
                    }
                }

                bool appearsToBeInCycle = false;
                if (lastMoves.Count >= 4)
                {
                    var move1 = lastMoves[lastMoves.Count - 1];
                    var move2 = lastMoves[lastMoves.Count - 2];
                    var move3 = lastMoves[lastMoves.Count - 3];
                    var move4 = lastMoves[lastMoves.Count - 4];

                    appearsToBeInCycle =
                        (move1.Item1 == move3.Item1 && move1.Item2 == move3.Item2 &&
                         move2.Item1 == move4.Item1 && move2.Item2 == move4.Item2) ||
                        (move1.Item1 == move3.Item2 && move3.Item1 == move2.Item2 && move2.Item1 == move1.Item2) ||
                        (move1.Item1 == move2.Item1 && move1.Item2 == move2.Item2);

                    if (appearsToBeInCycle)
                    {
                        Debug.Log("AI Solver: Detected cycle pattern in recent moves");
                    }
                }

                if (hasSurpriseNuts && surpriseStuck && isBoosterCurrentlyUsable)
                {
                    for (int sourceIdx = 0; sourceIdx < currentState.Count; sourceIdx++)
                    {
                        if (sourceIdx == boosterIdx || currentState[sourceIdx].Count == 0)
                            continue;

                        if (currentState[sourceIdx][currentState[sourceIdx].Count - 1] == surpriseNutId)
                        {
                            if (boosterIdx < capacities.Length &&
                                currentState[boosterIdx].Count < capacities[boosterIdx])
                            {
                                var boosterMove = (sourceIdx, boosterIdx);
                                TrackMove(boosterMove);
                                return boosterMove;
                            }
                        }
                    }
                }

                if (appearsToBeInCycle)
                {
                    var breakingMove = FindCycleBreakingMove(currentState, capacities);
                    if (breakingMove.HasValue)
                    {
                        TrackMove(breakingMove.Value);
                        return breakingMove;
                    }
                }

                if (hasSurpriseNuts && surpriseStuck)
                {
                    var surpriseMove = FindBestSurpriseMoveGreedy(currentState, capacities);
                    if (surpriseMove.HasValue)
                    {
                        TrackMove(surpriseMove.Value);
                        return surpriseMove;
                    }
                }

                int solverTimeout = hasSurpriseNuts ? 10000 : 8000;
                Solution solution = SolveLevel(currentState, currentState.Count, capacities, boosterIdx, isBoosterCurrentlyUsable);

                if (solution.IsSolved && solution.SolutionSteps != null && solution.SolutionSteps.Count > 0)
                {
                    var nextMove = solution.SolutionSteps[0];

                    if (appearsToBeInCycle || IsMoveLikelyToCauseCycle(currentState, nextMove.Item1, nextMove.Item2))
                    {
                        for (int i = 1; i < solution.SolutionSteps.Count && i < 3; i++)
                        {
                            var alternativeMove = solution.SolutionSteps[i];
                            if (!IsMoveLikelyToCauseCycle(currentState, alternativeMove.Item1, alternativeMove.Item2))
                            {
                                TrackMove(alternativeMove);
                                return alternativeMove;
                            }
                        }

                        var randomNonCyclicMove = FindNonCyclicMove(currentState, capacities);
                        if (randomNonCyclicMove.HasValue)
                        {
                            TrackMove(randomNonCyclicMove.Value);
                            return randomNonCyclicMove;
                        }
                    }

                    TrackMove(nextMove);
                    return nextMove;
                }
                else
                {
                    if (hasSurpriseNuts)
                    {
                        var surpriseMove = FindBestSurpriseMoveGreedy(currentState, capacities);
                        if (surpriseMove.HasValue)
                        {
                            TrackMove(surpriseMove.Value);
                            return surpriseMove;
                        }
                    }

                    var greedyMove = FindGreedyMove(currentState, capacities);
                    if (greedyMove.HasValue)
                    {
                        TrackMove(greedyMove.Value);
                        return greedyMove;
                    }

                    var randomNonRepeatingMove = FindRandomValidMove(currentState, capacities, lastMoves);
                    if (randomNonRepeatingMove.HasValue)
                    {
                        TrackMove(randomNonRepeatingMove.Value);
                        return randomNonRepeatingMove;
                    }

                    return null;
                }
            }
            finally
            {
                maxVisitedStates = originalMaxVisitedStates;
                heuristicWeight = originalHeuristicWeight;
            }
        }

        private void TrackMove((int, int) move)
        {
            lastMoves.Add(move);

            if (lastMoves.Count > maxRecentMovesToTrack)
            {
                lastMoves.RemoveAt(0);
            }

            string moveKey = $"{move.Item1}->{move.Item2}";
            recentMoves.Add(moveKey);

            if (recentMoves.Count > maxRecentMovesToTrack * 2)
            {
                recentMoves.Clear();
                foreach (var m in lastMoves)
                {
                    recentMoves.Add($"{m.Item1}->{m.Item2}");
                }
            }
        }

        private bool IsMoveLikelyToCauseCycle(List<List<int>> state, int sourceIndex, int destIndex)
        {
            string moveKey = $"{sourceIndex}->{destIndex}";
            string reverseKey = $"{destIndex}->{sourceIndex}";

            bool madeRecently = recentMoves.Contains(moveKey) || recentMoves.Contains(reverseKey);

            if (!madeRecently) return false;

            if (state[sourceIndex].Count > 0 && state[destIndex].Count > 0)
            {
                int sourceTop = state[sourceIndex][state[sourceIndex].Count - 1];
                int destTop = state[destIndex][state[destIndex].Count - 1];

                if (sourceTop == destTop && sourceTop != surpriseNutId && destTop != surpriseNutId)
                {
                    bool sourceHomogeneous = IsScrewMostlyHomogeneous(state[sourceIndex], sourceTop);
                    bool destHomogeneous = IsScrewMostlyHomogeneous(state[destIndex], destTop);

                    if (sourceHomogeneous && destHomogeneous)
                    {
                        Debug.Log($"AI Solver: Detected shuttle move between similar screws {sourceIndex}->{destIndex}");
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsScrewMostlyHomogeneous(List<int> screw, int topColor)
        {
            if (screw.Count <= 1) return true;

            int sameColorCount = 0;
            foreach (int nutType in screw)
            {
                if (nutType == topColor) sameColorCount++;
            }

            return (float)sameColorCount / screw.Count >= 0.8f;
        }

        public static bool IsGoalState(List<List<int>> state, int[] screwCapacities)
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
                if (!IsScrewSortedOrEmpty(state[i], i, screwCapacities))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsScrewSortedOrEmpty(List<int> screw, int screwIndex, int[] screwCapacities)
        {
            if (screw.Count == 0) return true;

            if (screwIndex < 0 || screwIndex >= screwCapacities.Length)
            {
                Debug.Log($"AI Solver Internal: Invalid screw index ({screwIndex}) passed to IsScrewSortedOrEmpty.");
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

        private bool IsScrewMonochromatic(List<int> screw)
        {
            if (screw.Count <= 1) return true;
            int firstNutType = screw[0];
            for (int i = 1; i < screw.Count; i++) { if (screw[i] != firstNutType) return false; }
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

            if (destIndex >= screwCapacities.Length)
            {
                Debug.Log($"AI Solver Internal: destIndex {destIndex} out of bounds for screwCapacities (size {screwCapacities.Length}).");
                return false;
            }
            if (destScrew.Count >= screwCapacities[destIndex]) return false;

            if (destIndex == boosterScrewIndex && !this.isBoosterCurrentlyUsableForSolve)
            {
                return false;
            }

            int nutToMove = sourceScrew[sourceScrew.Count - 1];

            if (destScrew.Count == 0)
            {
                return true;
            }

            int topNutOnDest = destScrew[destScrew.Count - 1];

            if (nutToMove == surpriseNutId)
                return false;

            if (topNutOnDest == surpriseNutId)
                return false;

            return nutToMove == topNutOnDest;
        }

        private List<List<int>> PerformMove(List<List<int>> currentState, int sourceIndex, int destIndex)
        {
            List<List<int>> newState = new List<List<int>>(currentState.Count);
            foreach (var screw in currentState) { newState.Add(new List<int>(screw)); }
            List<int> sourceScrew = newState[sourceIndex];
            List<int> destScrew = newState[destIndex];
            if (sourceScrew.Count > 0)
            {
                int nutToMove = sourceScrew[sourceScrew.Count - 1];
                sourceScrew.RemoveAt(sourceScrew.Count - 1);
                destScrew.Add(nutToMove);
            }
            return newState;
        }

        private int CalculateHeuristic(List<List<int>> state)
        {
            int heuristic = 0;

            float revealBonus = 60f * heuristicWeight;
            float surpriseExposureBonus = 100f * heuristicWeight;
            float emptySpaceBonus = 80f * heuristicWeight;
            float blockingPenalty = 20f * heuristicWeight;
            float regularSortingWeight = prioritizeSurpriseHandling ? 0.5f * heuristicWeight : 1.5f * heuristicWeight;

            float homogeneousStackPenalty = 40f * heuristicWeight;
            float completedStackBonus = 50f * heuristicWeight;
            float nearlyCompletedStackBonus = 30f * heuristicWeight;
            float dispersalPenalty = 25f * heuristicWeight;

            float moveMinimizationBonus = 35f * heuristicWeight;
            float directPathBonus = 45f * heuristicWeight;

            bool hasSurpriseNuts = false;
            foreach (var screw in state)
            {
                if (screw.Contains(surpriseNutId))
                {
                    hasSurpriseNuts = true;
                    break;
                }
            }

            int emptyScewCount = 0;
            foreach (var screw in state)
            {
                if (screw.Count == 0)
                {
                    emptyScewCount++;
                }
            }

            Dictionary<int, int> colorTotalCounts = new Dictionary<int, int>();
            Dictionary<int, List<int>> colorToScrews = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> colorToMainScrew = new Dictionary<int, List<int>>();
            Dictionary<int, bool> colorHasCompletedStack = new Dictionary<int, bool>();

            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];
                if (screw.Count == 0) continue;

                int screwCapacity = i < screwCapacities.Length ? screwCapacities[i] : 0;

                foreach (int color in screw)
                {
                    if (color == surpriseNutId) continue;

                    if (!colorTotalCounts.ContainsKey(color))
                    {
                        colorTotalCounts[color] = 0;
                        colorToScrews[color] = new List<int>();
                        colorHasCompletedStack[color] = false;
                    }
                    colorTotalCounts[color]++;

                    if (!colorToScrews[color].Contains(i))
                    {
                        colorToScrews[color].Add(i);
                    }
                }

                if (IsScrewMonochromatic(screw) && screw[0] != surpriseNutId)
                {
                    int color = screw[0];
                    if (!colorToMainScrew.ContainsKey(color))
                    {
                        colorToMainScrew[color] = new List<int>();
                    }
                    colorToMainScrew[color].Add(i);

                    if (screw.Count == screwCapacity)
                    {
                        colorHasCompletedStack[color] = true;
                        heuristic -= Mathf.RoundToInt(completedStackBonus * 1.5f);
                    }
                }
            }

            foreach (var entry in colorTotalCounts)
            {
                int color = entry.Key;
                int totalCount = entry.Value;
                List<int> screwsWithColor = colorToScrews[color];

                if (colorHasCompletedStack[color]) continue;

                bool canCompleteStack = false;
                int bestScrewIndex = -1;
                int bestScrewNutCount = 0;

                foreach (int screwIndex in screwsWithColor)
                {
                    if (screwIndex >= screwCapacities.Length) continue;
                    int capacity = screwCapacities[screwIndex];

                    int coloredNutsOnScrew = 0;
                    foreach (int nutColor in state[screwIndex])
                    {
                        if (nutColor == color) coloredNutsOnScrew++;
                    }

                    if (coloredNutsOnScrew > bestScrewNutCount)
                    {
                        bestScrewNutCount = coloredNutsOnScrew;
                        bestScrewIndex = screwIndex;
                    }

                    if (totalCount >= capacity)
                    {
                        canCompleteStack = true;
                    }
                }

                if (canCompleteStack && screwsWithColor.Count > 1)
                {
                    heuristic += Mathf.RoundToInt(dispersalPenalty * (screwsWithColor.Count - 1));
                }

                if (bestScrewIndex != -1 && bestScrewNutCount > (screwCapacities[bestScrewIndex] * 0.75f))
                {
                    heuristic -= Mathf.RoundToInt(nearlyCompletedStackBonus * (bestScrewNutCount / (float)screwCapacities[bestScrewIndex]));

                    if (bestScrewNutCount == totalCount && bestScrewNutCount < screwCapacities[bestScrewIndex])
                    {
                        heuristic -= Mathf.RoundToInt(moveMinimizationBonus);
                    }
                }
            }

            Dictionary<int, List<int>> topColorToScrews = new Dictionary<int, List<int>>();

            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];
                if (screw.Count > 0)
                {
                    int topColor = screw[screw.Count - 1];
                    if (topColor == surpriseNutId) continue;

                    if (!topColorToScrews.ContainsKey(topColor))
                    {
                        topColorToScrews[topColor] = new List<int>();
                    }
                    topColorToScrews[topColor].Add(i);
                }
            }

            foreach (var entry in topColorToScrews)
            {
                int color = entry.Key;
                List<int> screwIndices = entry.Value;

                if (screwIndices.Count > 1)
                {
                    int homogeneousScrews = 0;

                    foreach (int idx in screwIndices)
                    {
                        if (IsScrewMostlyHomogeneous(state[idx], color))
                        {
                            homogeneousScrews++;
                        }
                    }

                    if (homogeneousScrews > 1)
                    {
                        heuristic += Mathf.RoundToInt(homogeneousStackPenalty * (homogeneousScrews - 1) * 3);
                    }
                }
            }

            for (int i = 0; i < state.Count; i++)
            {
                List<int> screw = state[i];
                if (screw.Count == 0) continue;

                if (i >= screwCapacities.Length)
                {
                    Debug.Log($"AI Solver Heuristic: Index {i} out of bounds for screwCapacities (size {screwCapacities.Length}). Skipping capacity-based checks for this screw.");
                    continue;
                }
                int screwCapacity = screwCapacities[i];

                heuristic += Mathf.RoundToInt(screw.Count * regularSortingWeight);

                bool screwHasSurprise = false;
                int firstSurpriseIndex = -1;
                int surpriseCount = 0;

                for (int k = 0; k < screw.Count; k++)
                {
                    if (screw[k] == surpriseNutId)
                    {
                        surpriseCount++;
                        screwHasSurprise = true;
                        if (firstSurpriseIndex == -1)
                        {
                            firstSurpriseIndex = k;
                        }
                    }
                }

                if (screwHasSurprise && screw.Count > 0 && screw[screw.Count - 1] == surpriseNutId)
                {
                    heuristic -= Mathf.RoundToInt(surpriseExposureBonus * 1.5f);

                    if (emptyScewCount > 0)
                    {
                        float emptyBonus = useAggressiveEmptyPreference ? 250f * heuristicWeight : 200f * heuristicWeight;
                        heuristic -= Mathf.RoundToInt(emptyBonus);
                    }
                    else
                    {
                        heuristic += Mathf.RoundToInt(120f * heuristicWeight);
                    }
                }

                if (screwHasSurprise)
                {
                    int blockingNuts = 0;
                    for (int k = firstSurpriseIndex + 1; k < screw.Count; k++)
                    {
                        blockingNuts++;
                    }
                    heuristic += Mathf.RoundToInt(blockingNuts * blockingPenalty * 1.5f);

                    if (firstSurpriseIndex == screw.Count - 2)
                    {
                        heuristic -= Mathf.RoundToInt(revealBonus * 1.3f);
                    }
                    else if (firstSurpriseIndex == screw.Count - 3)
                    {
                        heuristic -= Mathf.RoundToInt(revealBonus * 0.7f);
                    }

                    if (emptyScewCount == 0)
                    {
                        heuristic += Mathf.RoundToInt(150f * heuristicWeight);
                    }
                    else
                    {
                        float emptyBonus = useAggressiveEmptyPreference ? emptySpaceBonus * 2f : emptySpaceBonus * 1.5f;
                        heuristic -= Mathf.RoundToInt(emptyScewCount * emptyBonus);
                    }
                }

                if (!hasSurpriseNuts || !prioritizeSurpriseHandling)
                {
                    if (screw.Count > 0)
                    {
                        int topColor = screw[screw.Count - 1];
                        if (topColor != surpriseNutId)
                        {
                            int correctSequenceCount = 0;
                            for (int j = screw.Count - 1; j >= 0; j--)
                            {
                                if (screw[j] == surpriseNutId) break;
                                if (screw[j] == topColor) correctSequenceCount++;
                                else break;
                            }
                            heuristic += Mathf.RoundToInt((screw.Count - correctSequenceCount) * regularSortingWeight * 1.2f);
                        }
                    }

                    if (IsScrewMonochromatic(screw))
                    {
                        if (screw.Count > 0 && screw[0] != surpriseNutId)
                        {
                            heuristic -= Mathf.RoundToInt(12f * regularSortingWeight);

                            float fillPercentage = screw.Count / (float)screwCapacity;
                            heuristic -= Mathf.RoundToInt(fillPercentage * 15f * regularSortingWeight);

                            if (screw.Count == screwCapacity)
                            {
                                heuristic -= Mathf.RoundToInt(completedStackBonus * 1.5f);
                            }
                            else if (screw.Count >= screwCapacity * 0.75f)
                            {
                                heuristic -= Mathf.RoundToInt(nearlyCompletedStackBonus * (screw.Count / (float)screwCapacity) * 1.2f);
                            }
                        }
                    }

                    if (screw.Count > 0 && screw.Count < screwCapacity)
                    {
                        int topColor = screw[screw.Count - 1];
                        if (topColor != surpriseNutId)
                        {
                            int sameColorCount = 0;
                            foreach (int color in screw)
                            {
                                if (color == topColor) sameColorCount++;
                            }

                            if (sameColorCount > (screwCapacity / 2) && sameColorCount < screwCapacity)
                            {
                                if (colorToScrews.ContainsKey(topColor) && colorToScrews[topColor].Count > 1)
                                {
                                    heuristic -= Mathf.RoundToInt(directPathBonus * (sameColorCount / (float)screwCapacity));
                                }
                            }
                        }
                    }
                }
            }

            return Mathf.Max(0, heuristic);
        }

        private List<(int, int)> GeneratePath(StateNode goalNode)
        {
            var path = new List<(int, int)>();
            StateNode current = goalNode;
            while (current != null && current.Move.HasValue) { path.Add(current.Move.Value); current = current.Parent; }
            path.Reverse();
            return path;
        }

        public static string StateToString(List<List<int>> state)
        {
            try
            {
                List<string> screwStrings = state?
                    .Select(screw => "[" + string.Join(",", screw ?? new List<int>()) + "]")
                    .OrderBy(s => s)
                    .ToList() ?? new List<string>();
                return string.Join(";", screwStrings);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error in StateToString: {ex.Message}");
                return "STATE_ERROR";
            }
        }

        private (int, int)? FindCycleBreakingMove(List<List<int>> state, int[] capacities)
        {
            Dictionary<int, int> screwUsageCounts = new Dictionary<int, int>();

            for (int i = 0; i < state.Count; i++)
            {
                screwUsageCounts[i] = 0;
            }

            foreach (var move in lastMoves)
            {
                if (!screwUsageCounts.ContainsKey(move.Item1))
                    screwUsageCounts[move.Item1] = 0;
                if (!screwUsageCounts.ContainsKey(move.Item2))
                    screwUsageCounts[move.Item2] = 0;

                screwUsageCounts[move.Item1]++;
                screwUsageCounts[move.Item2]++;
            }

            List<(int, int, int)> candidateMoves = new List<(int, int, int)>();

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        int usageScore = screwUsageCounts[sourceIdx] + screwUsageCounts[destIdx];

                        bool isRecentMove = false;
                        foreach (var move in lastMoves)
                        {
                            if (move.Item1 == sourceIdx && move.Item2 == destIdx)
                            {
                                isRecentMove = true;
                                break;
                            }
                        }

                        if (isRecentMove) continue;

                        candidateMoves.Add((sourceIdx, destIdx, usageScore));
                    }
                }
            }

            if (candidateMoves.Count > 0)
            {
                candidateMoves.Sort((a, b) => a.Item3.CompareTo(b.Item3));
                return (candidateMoves[0].Item1, candidateMoves[0].Item2);
            }

            return null;
        }

        private (int, int)? FindRandomValidMove(List<List<int>> state, int[] capacities, List<(int, int)> movesToAvoid = null)
        {
            List<(int, int)> validMoves = new List<(int, int)>();

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        bool shouldAvoid = false;

                        if (movesToAvoid != null)
                        {
                            foreach (var avoidMove in movesToAvoid)
                            {
                                if ((avoidMove.Item1 == sourceIdx && avoidMove.Item2 == destIdx) ||
                                    (avoidMove.Item1 == destIdx && avoidMove.Item2 == sourceIdx))
                                {
                                    shouldAvoid = true;
                                    break;
                                }
                            }
                        }

                        if (!shouldAvoid)
                        {
                            validMoves.Add((sourceIdx, destIdx));
                        }
                    }
                }
            }

            if (validMoves.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, validMoves.Count);
                return validMoves[randomIndex];
            }

            if (movesToAvoid != null && movesToAvoid.Count > 0)
            {
                return FindRandomValidMove(state, capacities);
            }

            return null;
        }

        private (int, int)? FindBestSurpriseMoveGreedy(List<List<int>> state, int[] capacities)
        {
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                if (sourceScrew[sourceScrew.Count - 1] == surpriseNutId)
                {
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (state[destIdx].Count == 0 && CanMoveNut(state, sourceIdx, destIdx))
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count <= 1) continue;

                bool hasBuriedSurprise = false;
                for (int i = 0; i < sourceScrew.Count - 1; i++)
                {
                    if (sourceScrew[i] == surpriseNutId)
                    {
                        hasBuriedSurprise = true;
                        break;
                    }
                }

                if (hasBuriedSurprise)
                {
                    int topNut = sourceScrew[sourceScrew.Count - 1];

                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (state[destIdx].Count == 0 && CanMoveNut(state, sourceIdx, destIdx))
                        {
                            return (sourceIdx, destIdx);
                        }
                    }

                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (destIdx == sourceIdx) continue;

                        var destScrew = state[destIdx];
                        if (destScrew.Count > 0 &&
                            destScrew[destScrew.Count - 1] == topNut &&
                            CanMoveNut(state, sourceIdx, destIdx))
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                int topColor = sourceScrew[sourceScrew.Count - 1];
                if (topColor == surpriseNutId) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (destIdx == sourceIdx) continue;

                    var destScrew = state[destIdx];
                    if (destScrew.Count > 0 &&
                        destScrew[destScrew.Count - 1] == topColor &&
                        CanMoveNut(state, sourceIdx, destIdx))
                    {
                        if (sourceScrew.Count == 1)
                        {
                            return (sourceIdx, destIdx);
                        }

                        if (destIdx < capacities.Length &&
                            destScrew.Count == capacities[destIdx] - 1)
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }

            return null;
        }

        private (int, int)? FindGreedyMove(List<List<int>> state, int[] capacities)
        {
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                int topColor = sourceScrew[sourceScrew.Count - 1];
                if (topColor == surpriseNutId) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (destIdx == sourceIdx) continue;

                    var destScrew = state[destIdx];
                    if (destScrew.Count > 0 &&
                        destScrew[destScrew.Count - 1] == topColor &&
                        CanMoveNut(state, sourceIdx, destIdx))
                    {
                        if (destIdx < capacities.Length &&
                            destScrew.Count == capacities[destIdx] - 1)
                        {
                            return (sourceIdx, destIdx);
                        }

                        if (sourceScrew.Count == 1)
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                int topColor = sourceScrew[sourceScrew.Count - 1];
                if (topColor == surpriseNutId) continue;

                int bestDestIdx = -1;
                int maxSameColorCount = 0;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (destIdx == sourceIdx) continue;

                    var destScrew = state[destIdx];
                    if (destScrew.Count > 0 &&
                        destScrew[destScrew.Count - 1] == topColor &&
                        CanMoveNut(state, sourceIdx, destIdx))
                    {
                        int sameColorCount = 0;
                        foreach (int nut in destScrew)
                        {
                            if (nut == topColor) sameColorCount++;
                        }

                        if (sameColorCount > maxSameColorCount)
                        {
                            maxSameColorCount = sameColorCount;
                            bestDestIdx = destIdx;
                        }
                    }
                }

                if (bestDestIdx != -1)
                {
                    return (sourceIdx, bestDestIdx);
                }
            }

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (state[destIdx].Count == 0 && CanMoveNut(state, sourceIdx, destIdx))
                    {
                        return (sourceIdx, destIdx);
                    }
                }
            }

            return null;
        }

        private (int, int)? FindNonCyclicMove(List<List<int>> state, int[] capacities)
        {
            List<(int, int, float)> moveScores = new List<(int, int, float)>();

            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;

                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        float score = 0;

                        foreach (var move in lastMoves)
                        {
                            if ((move.Item1 == sourceIdx && move.Item2 == destIdx) ||
                                (move.Item1 == destIdx && move.Item2 == sourceIdx))
                            {
                                score += 100;
                            }
                        }

                        if (state[sourceIdx].Count > 0 && state[destIdx].Count > 0)
                        {
                            int sourceTop = state[sourceIdx][state[sourceIdx].Count - 1];
                            int destTop = state[destIdx][state[destIdx].Count - 1];

                            if (sourceTop == destTop && sourceTop != surpriseNutId)
                            {
                                score += 50;
                            }
                        }

                        if (state[sourceIdx].Count == 1)
                        {
                            score -= 30;
                        }

                        if (destIdx < capacities.Length &&
                            state[destIdx].Count == capacities[destIdx] - 1)
                        {
                            score -= 40;
                        }

                        moveScores.Add((sourceIdx, destIdx, score));
                    }
                }
            }

            moveScores.Sort((a, b) => a.Item3.CompareTo(b.Item3));

            if (moveScores.Count > 0)
            {
                return (moveScores[0].Item1, moveScores[0].Item2);
            }

            return null;
        }

        private bool IsNearGoalState(List<List<int>> state, int[] capacities)
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

            if (hasSurpriseNuts) return false;

            int completedScrews = 0;
            int totalScrews = state.Count;
            float totalProgress = 0f;

            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];

                if (i >= capacities.Length) continue;

                if (IsScrewSortedOrEmpty(screw, i, capacities))
                {
                    completedScrews++;
                    totalProgress += 1.0f;
                }
                else if (screw.Count > 0)
                {
                    int capacity = capacities[i];
                    int dominantColor = FindDominantColor(screw);

                    if (dominantColor != -1)
                    {
                        int dominantColorCount = 0;
                        foreach (int color in screw)
                        {
                            if (color == dominantColor) dominantColorCount++;
                        }

                        float screwProgress = dominantColorCount / (float)capacity;
                        totalProgress += screwProgress;
                    }
                }
            }

            float averageProgress = totalProgress / totalScrews;

            return completedScrews > totalScrews * 0.7f || averageProgress > 0.85f;
        }

        private int FindDominantColor(List<int> screw)
        {
            if (screw.Count == 0) return -1;

            Dictionary<int, int> colorCounts = new Dictionary<int, int>();

            foreach (int color in screw)
            {
                if (color == surpriseNutId) continue;

                if (!colorCounts.ContainsKey(color))
                {
                    colorCounts[color] = 0;
                }
                colorCounts[color]++;
            }

            int dominantColor = -1;
            int maxCount = 0;

            foreach (var entry in colorCounts)
            {
                if (entry.Value > maxCount)
                {
                    maxCount = entry.Value;
                    dominantColor = entry.Key;
                }
            }

            return maxCount > screw.Count * 0.6f ? dominantColor : -1;
        }
    }

    public class SimplePriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<KeyValuePair<TElement, TPriority>> elements = new List<KeyValuePair<TElement, TPriority>>();
        public int Count => elements.Count;
        public void Enqueue(TElement element, TPriority priority) { elements.Add(new KeyValuePair<TElement, TPriority>(element, priority)); elements.Sort((x, y) => x.Value.CompareTo(y.Value)); }
        public TElement Dequeue() { if (Count == 0) throw new InvalidOperationException("Queue is empty"); TElement element = elements[0].Key; elements.RemoveAt(0); return element; }
        public bool TryDequeue(out TElement element, out TPriority priority) { if (Count == 0) { element = default; priority = default; return false; } element = elements[0].Key; priority = elements[0].Value; elements.RemoveAt(0); return true; }
        public TElement Peek() { if (Count == 0) throw new InvalidOperationException("Queue is empty"); return elements[0].Key; }
        public bool Contains(TElement element) { return elements.Any(kvp => EqualityComparer<TElement>.Default.Equals(kvp.Key, element)); }
    }
}