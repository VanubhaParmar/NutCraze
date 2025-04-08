using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class ScrewNutSolver
    {
        // StateNode struct
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

        // Solution struct
        public readonly struct Solution
        {
            public bool IsSolved { get; }
            public List<(int, int)> SolutionSteps { get; }
            public Solution(bool solved, List<(int, int)> steps) { IsSolved = solved; SolutionSteps = steps; }
        }

        // Member variables
        private int[] screwCapacities;
        private int boosterScrewIndex = -1;
        private bool isBoosterCurrentlyUsableForSolve = false;
        
        // Configuration parameters
        private float heuristicWeight = 1.0f;
        private int maxVisitedStates = 50000;
        private bool useAggressiveEmptyPreference = false;
        private bool prioritizeSurpriseHandling = true;
        
        // Anti-cycling mechanism
        private HashSet<string> recentMoves = new HashSet<string>();
        private int maxRecentMovesToTrack = 10;
        private List<(int, int)> lastMoves = new List<(int, int)>();

        // Constants
        private const int surpriseNutId = 31; // Hardcoded ID
        
        // Configuration methods
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

        // SolveLevel method
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

            // Basic check: Is the start state already the goal state?
            if (IsGoalState(startState, capacities))
            {
                Debug.Log("AI Solver: Start state is already goal state.");
                return new Solution(true, new List<(int, int)>()); // Return success with empty path
            }

            queue.Enqueue(startNode, startNode.TotalCost);
            visited.Add(startStateString, 0);

            StateNode goalNode = null;
            int visitedCount = 0;
            int currentMaxVisitedStates = maxVisitedStates; // Use the configured value
            
            // Add short-circuit values to limit search even more
            int shortCircuitCount = Mathf.Min(currentMaxVisitedStates, 25000);
            int iterationsPerFrame = 1000; // Process this many nodes before yielding back to Unity
            int iterationsSinceYield = 0;
            
            // Track the last few nodes processed to detect and avoid cycles
            List<(int, int)> lastFewMoves = new List<(int, int)>();
            Dictionary<string, bool> recentMovePatterns = new Dictionary<string, bool>();
            // Check if we're near the goal state - use less strict heuristic if so
            bool isNearGoal = IsNearGoalState(startState, capacities);
            
            // If near goal, increase search depth to find optimal solutions
            if (isNearGoal)
            {
                currentMaxVisitedStates = Mathf.Min(currentMaxVisitedStates * 2, 100000);
                shortCircuitCount = Mathf.Min(shortCircuitCount * 2, 50000);
                Debug.Log("AI Solver: Near goal state detected. Increasing search depth to find optimal solution.");
            }

            while (queue.Count > 0 && visitedCount < currentMaxVisitedStates)
            {
                // Process nodes in smaller batches for better responsiveness
                if (iterationsSinceYield >= iterationsPerFrame)
                {
                    // Reset counter and break out early if we've processed enough for now
                    iterationsSinceYield = 0;
                    
                    // Optional: Check if we've done a significant amount of work already
                    if (visitedCount > shortCircuitCount && goalNode == null)
                    {
                        Debug.LogWarning($"AI Solver: Early termination after {visitedCount} states. No solution found yet.");
                        break;
                    }
                }
                
                StateNode currentNode = queue.Dequeue();
                visitedCount++;
                iterationsSinceYield++;

                // Check if the *current node's state* is the goal AFTER dequeuing
                if (IsGoalState(currentNode.State, capacities))
                {
                    goalNode = currentNode;
                    Debug.Log($"AI Solver: Goal state found after visiting {visitedCount} states. Path cost: {goalNode.PathCost}");
                    break;
                }

                // Generate next states and add to queue with anti-cycling logic
                GenerateNextStatesInChunksWithCyclePrevention(currentNode, queue, visited, 50, lastFewMoves, recentMovePatterns);
            }

            if ((visitedCount >= currentMaxVisitedStates || visitedCount >= shortCircuitCount) && goalNode == null)
            {
                Debug.LogWarning($"AI Solver: Search limit ({visitedCount}/{currentMaxVisitedStates} visited states) reached. No solution found within limits.");
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
        
        // Generate next states in chunks with cycle prevention
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
            
            // Track if the current node is part of a move from the parent
            (int, int)? lastMove = currentNode.Move;
            if (lastMove.HasValue)
            {
                // Add to move history for cycle detection
                moveHistory.Add(lastMove.Value);
                
                // Keep the list at max size
                if (moveHistory.Count > 7)
                {
                    moveHistory.RemoveAt(0);
                }
                
                // Check for back-and-forth moves between two specific screws
                // (A→B, B→A pattern)
                if (moveHistory.Count >= 2)
                {
                    var moveA = moveHistory[moveHistory.Count - 1];
                    var moveB = moveHistory[moveHistory.Count - 2];
                    
                    // Check if we're seeing A→B followed by B→A
                    if (moveA.Item1 == moveB.Item2 && moveA.Item2 == moveB.Item1)
                    {
                        string pattern = $"{moveB.Item2}-{moveB.Item1}-shuttle";
                        if (!recentMovePatterns.ContainsKey(pattern))
                        {
                            recentMovePatterns[pattern] = true;
                        }
                    }
                }
                
                // Detect longer cycles like A→B, B→C, C→A
                if (moveHistory.Count >= 3)
                {
                    var move1 = moveHistory[moveHistory.Count - 1];
                    var move2 = moveHistory[moveHistory.Count - 2];
                    var move3 = moveHistory[moveHistory.Count - 3];
                    
                    // A→B→C→A cycle
                    if (move1.Item1 == move3.Item2 && move1.Item2 != move3.Item1)
                    {
                        string pattern = $"{move3.Item1}-{move3.Item2}-{move2.Item2}-cycle";
                        if (!recentMovePatterns.ContainsKey(pattern))
                        {
                            recentMovePatterns[pattern] = true;
                        }
                    }
                }
                
                // Manage pattern dictionary size
                if (recentMovePatterns.Count > 15)
                {
                    recentMovePatterns.Clear();
                }
            }

            // Create a list of candidate moves with their scores
            List<(int sourceIdx, int destIdx, int cycleRisk)> candidateMoves = new List<(int, int, int)>();

            // First, generate all possible candidate moves with cycle risk scores
            for (int sourceIdx = 0; sourceIdx < numScrews; sourceIdx++)
            {
                for (int destIdx = 0; destIdx < numScrews; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;

                    if (CanMoveNut(currentState, sourceIdx, destIdx))
                    {
                        // Calculate a "cycle risk" score
                        int cycleRisk = 0;
                        
                        // Check for immediate back-and-forth pattern
                        if (lastMove.HasValue && lastMove.Value.Item1 == destIdx && lastMove.Value.Item2 == sourceIdx)
                        {
                            cycleRisk += 100; // Heavy penalty for immediate reversal
                        }
                        
                        // Check for known cycle patterns
                        string shuttlePattern = $"{sourceIdx}-{destIdx}-shuttle";
                        if (recentMovePatterns.ContainsKey(shuttlePattern))
                        {
                            cycleRisk += 50;
                        }
                        
                        // Check for same-color shuttling
                        if (currentState[sourceIdx].Count > 0 && currentState[destIdx].Count > 0)
                        {
                            int sourceTop = currentState[sourceIdx][currentState[sourceIdx].Count - 1];
                            int destTop = currentState[destIdx][currentState[destIdx].Count - 1];
                            
                            if (sourceTop == destTop && sourceTop != surpriseNutId)
                            {
                                // Check if both screws are mostly homogeneous with the same color
                                bool sourceHomogeneous = IsScrewMostlyHomogeneous(currentState[sourceIdx], sourceTop);
                                bool destHomogeneous = IsScrewMostlyHomogeneous(currentState[destIdx], destTop);
                                
                                if (sourceHomogeneous && destHomogeneous)
                                {
                                    cycleRisk += 30; // Penalty for moving between similar stacks
                                }
                            }
                        }
                        
                        // Add this move to candidates
                        candidateMoves.Add((sourceIdx, destIdx, cycleRisk));
                    }
                }
            }

            // Sort by cycle risk (lower is better)
            candidateMoves.Sort((a, b) => a.cycleRisk.CompareTo(b.cycleRisk));
            
            // Process moves with preference for those less likely to cause cycles
            foreach (var moveInfo in candidateMoves)
            {
                if (statesGenerated >= maxStatesPerNode) break;
                
                int sourceIdx = moveInfo.sourceIdx;
                int destIdx = moveInfo.destIdx;
                int cycleRisk = moveInfo.cycleRisk;
                
                // Perform the move
                List<List<int>> nextState = PerformMove(currentState, sourceIdx, destIdx);
                string nextStateString = StateToString(nextState);
                
                // Calculate path cost with penalty for risky moves
                int nextPathCost = currentPathCost + 1;
                if (cycleRisk > 0)
                {
                    // Add a small path cost penalty for moves with high cycle risk
                    // but not so much that it dominates good moves
                    nextPathCost += Mathf.Min(3, cycleRisk / 20);
                }

                // Check if we've seen this state and if our new path is better
                if (visited.TryGetValue(nextStateString, out int existingPathCost))
                {
                    if (existingPathCost <= nextPathCost) continue;
                    else visited[nextStateString] = nextPathCost;
                }
                else
                {
                    visited.Add(nextStateString, nextPathCost);
                }

                // Calculate heuristic for this new state
                int nextHeuristic = CalculateHeuristic(nextState);
                
                // Create new node and add to queue
                var nextNode = new StateNode(nextHeuristic, nextPathCost, nextState, currentNode, (sourceIdx, destIdx));
                queue.Enqueue(nextNode, nextNode.TotalCost);
                statesGenerated++;
            }
        }

        // Get next move
        public (int, int)? GetNextMove(List<List<int>> currentState, int[] capacities, int boosterIdx, bool isBoosterCurrentlyUsable)
        {
            // Throttle down the search parameters for faster response
            int originalMaxVisitedStates = maxVisitedStates;
            float originalHeuristicWeight = heuristicWeight;
            
            try
            {
                // Performance optimization: For Unity builds, use smaller search space
                maxVisitedStates = Mathf.Min(maxVisitedStates, 15000); // Reduced for faster response in builds
                heuristicWeight *= 1.1f; // Slightly increase heuristic influence

                // Check for surprise nuts and if any are stuck (needs empty screws)
                bool hasSurpriseNuts = false;
                bool surpriseStuck = false;
                bool emptyScrewAvailable = false;
                
                // Single-pass scan of the board state to gather key information
                // This is more efficient than multiple separate scans
                foreach (var screw in currentState)
                {
                    // Track if any screw is empty
                    if (screw.Count == 0)
                    {
                        emptyScrewAvailable = true;
                        continue;
                    }
                    
                    // Check for surprise nuts
                    foreach (int nutType in screw)
                    {
                        if (nutType == surpriseNutId)
                        {
                            hasSurpriseNuts = true;
                            break;
                        }
                    }
                    
                    // Check for surprise nut at top
                    if (screw.Count > 0 && screw[screw.Count - 1] == surpriseNutId && !emptyScrewAvailable)
                    {
                        surpriseStuck = true;
                    }
                }
                
                // Cycle detection optimized with less dictionary operations
                bool appearsToBeInCycle = false;
                if (lastMoves.Count >= 4)
                {
                    var move1 = lastMoves[lastMoves.Count - 1];
                    var move2 = lastMoves[lastMoves.Count - 2];
                    var move3 = lastMoves[lastMoves.Count - 3];
                    var move4 = lastMoves[lastMoves.Count - 4];
                    
                    // Check direct A→B→A→B pattern efficiently
                    appearsToBeInCycle = 
                        (move1.Item1 == move3.Item1 && move1.Item2 == move3.Item2 && 
                         move2.Item1 == move4.Item1 && move2.Item2 == move4.Item2) ||
                        // Check triangular A→B→C→A pattern
                        (move1.Item1 == move3.Item2 && move3.Item1 == move2.Item2 && move2.Item1 == move1.Item2) ||
                        // Check repeating move
                        (move1.Item1 == move2.Item1 && move1.Item2 == move2.Item2);
                        
                    if (appearsToBeInCycle)
                    {
                        Debug.Log("AI Solver: Detected cycle pattern in recent moves");
                    }
                }
                
                // Fast path for surprise nuts at top needing empty screws
                if (hasSurpriseNuts && surpriseStuck && isBoosterCurrentlyUsable)
                {
                    // Find the best move using the booster as destination
                    for (int sourceIdx = 0; sourceIdx < currentState.Count; sourceIdx++)
                    {
                        if (sourceIdx == boosterIdx || currentState[sourceIdx].Count == 0) 
                            continue;
                        
                        // Source screw has a surprise nut at top
                        if (currentState[sourceIdx][currentState[sourceIdx].Count - 1] == surpriseNutId)
                        {
                            // If booster has space
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

                // Handle cycles using more focused, faster approach
                if (appearsToBeInCycle)
                {
                    // If we're in a cycle, try to break it with an efficient special move
                    var breakingMove = FindCycleBreakingMove(currentState, capacities);
                    if (breakingMove.HasValue)
                    {
                        TrackMove(breakingMove.Value);
                        return breakingMove;
                    }
                }
                
                // For urgent surprise nut cases, use the dedicated greedy approach
                if (hasSurpriseNuts && surpriseStuck)
                {
                    var surpriseMove = FindBestSurpriseMoveGreedy(currentState, capacities);
                    if (surpriseMove.HasValue)
                    {
                        TrackMove(surpriseMove.Value);
                        return surpriseMove;
                    }
                }
                
                // Run the A* solver with reduced search parameters for better performance
                // Use a shorter timeout and limit max steps to improve responsiveness
                int solverTimeout = hasSurpriseNuts ? 10000 : 8000; // Less time for surprise nut levels
                Solution solution = SolveLevel(currentState, currentState.Count, capacities, boosterIdx, isBoosterCurrentlyUsable);

                if (solution.IsSolved && solution.SolutionSteps != null && solution.SolutionSteps.Count > 0)
                {
                    // Get first move
                    var nextMove = solution.SolutionSteps[0];
                    
                    // Check if this move would create a cycle by moving between identical screws
                    if (appearsToBeInCycle || IsMoveLikelyToCauseCycle(currentState, nextMove.Item1, nextMove.Item2))
                    {
                        // Try to find an alternative move from the solution path
                        for (int i = 1; i < solution.SolutionSteps.Count && i < 3; i++) // Look at fewer alternatives for speed
                        {
                            var alternativeMove = solution.SolutionSteps[i];
                            if (!IsMoveLikelyToCauseCycle(currentState, alternativeMove.Item1, alternativeMove.Item2))
                            {
                                // Found a non-cycling move
                                TrackMove(alternativeMove);
                                return alternativeMove;
                            }
                        }
                        
                        // If we're here, all potential moves seem cyclic, try a completely different approach
                        var randomNonCyclicMove = FindNonCyclicMove(currentState, capacities);
                        if (randomNonCyclicMove.HasValue)
                        {
                            TrackMove(randomNonCyclicMove.Value);
                            return randomNonCyclicMove;
                        }
                    }
                    
                    // Use the move from the solution
                    TrackMove(nextMove);
                    return nextMove;
                }
                else
                {
                    // No solution found - try greedy approaches
                    // Look for a move that focuses on surprise nuts if present
                    if (hasSurpriseNuts)
                    {
                        var surpriseMove = FindBestSurpriseMoveGreedy(currentState, capacities);
                        if (surpriseMove.HasValue)
                        {
                            TrackMove(surpriseMove.Value);
                            return surpriseMove;
                        }
                    }
                    
                    // Try to find a move that leads toward completion
                    var greedyMove = FindGreedyMove(currentState, capacities);
                    if (greedyMove.HasValue)
                    {
                        TrackMove(greedyMove.Value);
                        return greedyMove;
                    }
                    
                    // Last resort: Any non-repeating valid move
                    var randomNonRepeatingMove = FindRandomValidMove(currentState, capacities, lastMoves);
                    if (randomNonRepeatingMove.HasValue)
                    {
                        TrackMove(randomNonRepeatingMove.Value);
                        return randomNonRepeatingMove;
                    }
                    
                    return null; // No solution found from this state
                }
            }
            finally
            {
                // Restore original values
                maxVisitedStates = originalMaxVisitedStates;
                heuristicWeight = originalHeuristicWeight;
            }
        }
        
        // Track recent moves to detect cycles
        private void TrackMove((int, int) move)
        {
            // Add to recent moves list
            lastMoves.Add(move);
            
            // Keep list at max size
            if (lastMoves.Count > maxRecentMovesToTrack)
            {
                lastMoves.RemoveAt(0);
            }
            
            // Add string representation to hash set
            string moveKey = $"{move.Item1}->{move.Item2}";
            recentMoves.Add(moveKey);
            
            // Prune hash set if it gets too large
            if (recentMoves.Count > maxRecentMovesToTrack * 2)
            {
                recentMoves.Clear();
                foreach (var m in lastMoves)
                {
                    recentMoves.Add($"{m.Item1}->{m.Item2}");
                }
            }
        }
        
        // Check if a move is likely to cause a cycle
        private bool IsMoveLikelyToCauseCycle(List<List<int>> state, int sourceIndex, int destIndex)
        {
            // Check if this exact move was made recently
            string moveKey = $"{sourceIndex}->{destIndex}";
            string reverseKey = $"{destIndex}->{sourceIndex}";
            
            // If we've made this move or its reverse recently, it might be part of a cycle
            bool madeRecently = recentMoves.Contains(moveKey) || recentMoves.Contains(reverseKey);
            
            // If the move hasn't been made recently, it's probably not a cycle
            if (!madeRecently) return false;
            
            // Additional check: are we moving between screws with the same color on top?
            if (state[sourceIndex].Count > 0 && state[destIndex].Count > 0)
            {
                int sourceTop = state[sourceIndex][state[sourceIndex].Count - 1];
                int destTop = state[destIndex][state[destIndex].Count - 1];
                
                // If both screws have the same color on top and neither is a surprise nut,
                // this could be a shuttle move that doesn't help us progress
                if (sourceTop == destTop && sourceTop != surpriseNutId && destTop != surpriseNutId)
                {
                    // Check if the two screws are mostly homogeneous with the same color
                    bool sourceHomogeneous = IsScrewMostlyHomogeneous(state[sourceIndex], sourceTop);
                    bool destHomogeneous = IsScrewMostlyHomogeneous(state[destIndex], destTop);
                    
                    // If both screws are homogeneous with the same color, this move is likely part of a cycle
                    if (sourceHomogeneous && destHomogeneous)
                    {
                        Debug.Log($"AI Solver: Detected shuttle move between similar screws {sourceIndex}->{destIndex}");
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        // Check if a screw is mostly the same color (>80% same)
        private bool IsScrewMostlyHomogeneous(List<int> screw, int topColor)
        {
            if (screw.Count <= 1) return true;
            
            int sameColorCount = 0;
            foreach (int nutType in screw)
            {
                if (nutType == topColor) sameColorCount++;
            }
            
            // If 80% or more of the nuts are the same color as the top, consider it homogeneous
            return (float)sameColorCount / screw.Count >= 0.8f;
        }

        // Check if the state is a goal state
        public static bool IsGoalState(List<List<int>> state, int[] screwCapacities)
        {
            // First check if there are still surprise nuts anywhere
            bool hasSurpriseNuts = false;
            foreach (var screw in state)
            {
                if (screw.Contains(surpriseNutId))
                {
                    hasSurpriseNuts = true;
                    break;
                }
            }
            
            // If we still have surprise nuts, level is not solved
            if (hasSurpriseNuts)
            {
                return false;
            }
            
            // Standard check for regular nuts
            for (int i = 0; i < state.Count; i++)
            {
                if (!IsScrewSortedOrEmpty(state[i], i, screwCapacities))
                {
                    return false;
                }
            }
            return true;
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
            if (screw.Count != capacity) return false;

            int firstNutType = screw[0];
            // Use hardcoded ID here
            if (firstNutType == surpriseNutId) return false; // Cannot be sorted if only surprise nuts

            foreach (int nutType in screw)
            {
                if (nutType != firstNutType) return false;
            }
            return true;
        }

        // Check if a screw has all the same colored nuts
        private bool IsScrewMonochromatic(List<int> screw)
        {
            if (screw.Count <= 1) return true;
            int firstNutType = screw[0];
            for (int i = 1; i < screw.Count; i++) { if (screw[i] != firstNutType) return false; }
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

            // Check destination capacity
            if (destIndex >= screwCapacities.Length)
            {
                Debug.LogError($"AI Solver Internal: destIndex {destIndex} out of bounds for screwCapacities (size {screwCapacities.Length}).");
                return false;
            }
            if (destScrew.Count >= screwCapacities[destIndex]) return false; // Destination full

            // Check if moving TO booster when it's not usable for this solve run
            if (destIndex == boosterScrewIndex && !this.isBoosterCurrentlyUsableForSolve)
            {
                return false; // Cannot move to inactive booster
            }

            // Color match check
            int nutToMove = sourceScrew[sourceScrew.Count - 1];
            
            // Empty destination check
            if (destScrew.Count == 0)
            {
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

        // Perform a move in the state
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

        // Calculate heuristic for a state
        private int CalculateHeuristic(List<List<int>> state)
        {
            int heuristic = 0;
            
            // Increased weights for more decisive moves
            float revealBonus = 60f * heuristicWeight;
            float surpriseExposureBonus = 100f * heuristicWeight;
            float emptySpaceBonus = 80f * heuristicWeight;
            float blockingPenalty = 20f * heuristicWeight;
            float regularSortingWeight = prioritizeSurpriseHandling ? 0.5f * heuristicWeight : 1.5f * heuristicWeight;
            
            // Higher penalties to avoid cycles and find the most direct path
            float homogeneousStackPenalty = 40f * heuristicWeight;
            float completedStackBonus = 50f * heuristicWeight;
            float nearlyCompletedStackBonus = 30f * heuristicWeight;
            float dispersalPenalty = 25f * heuristicWeight;
            
            // New weights for minimalist solving
            float moveMinimizationBonus = 35f * heuristicWeight;
            float directPathBonus = 45f * heuristicWeight;

            // Check if state has any surprise nuts at all
            bool hasSurpriseNuts = false;
            foreach (var screw in state)
            {
                if (screw.Contains(surpriseNutId))
                {
                    hasSurpriseNuts = true;
                    break;
                }
            }

            // Count empty screws - critical for surprise nuts
            int emptyScewCount = 0;
            foreach (var screw in state)
            {
                if (screw.Count == 0)
                {
                    emptyScewCount++;
                }
            }
            
            // Color distribution analysis
            Dictionary<int, int> colorTotalCounts = new Dictionary<int, int>();
            Dictionary<int, List<int>> colorToScrews = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> colorToMainScrew = new Dictionary<int, List<int>>();
            Dictionary<int, bool> colorHasCompletedStack = new Dictionary<int, bool>();
            
            // Analyze color distribution across all screws
            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];
                if (screw.Count == 0) continue;
                
                // Skip capacity index validation, will check later
                int screwCapacity = i < screwCapacities.Length ? screwCapacities[i] : 0;
                
                // Count total nuts by color and track which screws have each color
                foreach (int color in screw)
                {
                    // Skip surprise nuts for this analysis
                    if (color == surpriseNutId) continue;
                    
                    // Count total of each color
                    if (!colorTotalCounts.ContainsKey(color))
                    {
                        colorTotalCounts[color] = 0;
                        colorToScrews[color] = new List<int>();
                        colorHasCompletedStack[color] = false;
                    }
                    colorTotalCounts[color]++;
                    
                    // Track which screws have this color
                    if (!colorToScrews[color].Contains(i))
                    {
                        colorToScrews[color].Add(i);
                    }
                }
                
                // Check for monochromatic screws that could be main screws for a color
                if (IsScrewMonochromatic(screw) && screw[0] != surpriseNutId)
                {
                    int color = screw[0];
                    if (!colorToMainScrew.ContainsKey(color))
                    {
                        colorToMainScrew[color] = new List<int>();
                    }
                    colorToMainScrew[color].Add(i);
                    
                    // Check if this is a completed stack
                    if (screw.Count == screwCapacity)
                    {
                        colorHasCompletedStack[color] = true;
                        // Significant bonus for completed stacks - encourage completion
                        heuristic -= Mathf.RoundToInt(completedStackBonus * 1.5f);
                    }
                }
            }
            
            // Calculate penalties/bonuses based on color distribution analysis
            foreach (var entry in colorTotalCounts)
            {
                int color = entry.Key;
                int totalCount = entry.Value;
                List<int> screwsWithColor = colorToScrews[color];
                
                // Skip if this color already has a completed stack
                if (colorHasCompletedStack[color]) continue;
                
                // Determine if this color has enough nuts to make a complete stack
                bool canCompleteStack = false;
                int bestScrewIndex = -1;
                int bestScrewNutCount = 0;
                
                // Find the screw with the most nuts of this color
                foreach (int screwIndex in screwsWithColor)
                {
                    if (screwIndex >= screwCapacities.Length) continue;
                    int capacity = screwCapacities[screwIndex];
                    
                    // Count nuts of this color on this screw
                    int coloredNutsOnScrew = 0;
                    foreach (int nutColor in state[screwIndex])
                    {
                        if (nutColor == color) coloredNutsOnScrew++;
                    }
                    
                    // Check if this is the best screw for this color
                    if (coloredNutsOnScrew > bestScrewNutCount)
                    {
                        bestScrewNutCount = coloredNutsOnScrew;
                        bestScrewIndex = screwIndex;
                    }
                    
                    // If any screw could potentially be completed with this color
                    if (totalCount >= capacity)
                    {
                        canCompleteStack = true;
                    }
                }
                
                // If this color can complete a stack but is spread across multiple screws,
                // add a penalty proportional to the dispersal
                if (canCompleteStack && screwsWithColor.Count > 1)
                {
                    // More screws with this color = more dispersed = bigger penalty
                    heuristic += Mathf.RoundToInt(dispersalPenalty * (screwsWithColor.Count - 1));
                }
                
                // If there's a best screw for this color that's nearly full (>75% of capacity)
                if (bestScrewIndex != -1 && bestScrewNutCount > (screwCapacities[bestScrewIndex] * 0.75f))
                {
                    // Give bonus for nearly completed stacks to encourage finishing them
                    heuristic -= Mathf.RoundToInt(nearlyCompletedStackBonus * (bestScrewNutCount / (float)screwCapacities[bestScrewIndex]));
                    
                    // Additional bonus for having all of a color in one place - minimizes future moves
                    if (bestScrewNutCount == totalCount && bestScrewNutCount < screwCapacities[bestScrewIndex])
                    {
                        heuristic -= Mathf.RoundToInt(moveMinimizationBonus);
                    }
                }
            }
            
            // Check for similar stacks that could cause cycles - heavily penalize
            Dictionary<int, List<int>> topColorToScrews = new Dictionary<int, List<int>>();
            
            // First pass: group screws by their top nut color
            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];
                if (screw.Count > 0)
                {
                    int topColor = screw[screw.Count - 1];
                    // Skip surprise nuts for this grouping
                    if (topColor == surpriseNutId) continue;
                    
                    if (!topColorToScrews.ContainsKey(topColor))
                    {
                        topColorToScrews[topColor] = new List<int>();
                    }
                    topColorToScrews[topColor].Add(i);
                }
            }
            
            // Add penalty for situations that might lead to shuttling nuts back and forth
            foreach (var entry in topColorToScrews)
            {
                int color = entry.Key;
                List<int> screwIndices = entry.Value;
                
                // If multiple screws have the same color on top - strong cycle risk
                if (screwIndices.Count > 1)
                {
                    int homogeneousScrews = 0;
                    
                    // Count how many are mostly the same color
                    foreach (int idx in screwIndices)
                    {
                        if (IsScrewMostlyHomogeneous(state[idx], color))
                        {
                            homogeneousScrews++;
                        }
                    }
                    
                    // If multiple homogeneous screws with same color, penalize to discourage cycling
                    if (homogeneousScrews > 1)
                    {
                        // Triple penalty - this is a critical indicator of cycles
                        heuristic += Mathf.RoundToInt(homogeneousStackPenalty * (homogeneousScrews - 1) * 3);
                    }
                }
            }

            for (int i = 0; i < state.Count; i++)
            {
                List<int> screw = state[i];
                if (screw.Count == 0) continue;

                // Ensure capacity index is valid
                if (i >= screwCapacities.Length)
                {
                    Debug.LogWarning($"AI Solver Heuristic: Index {i} out of bounds for screwCapacities (size {screwCapacities.Length}). Skipping capacity-based checks for this screw.");
                    continue; // Skip screw if capacity info is missing
                }
                int screwCapacity = screwCapacities[i];

                // --- Standard Penalty ---
                heuristic += Mathf.RoundToInt(screw.Count * regularSortingWeight);

                // --- Surprise Nut Logic ---
                bool screwHasSurprise = false;
                int firstSurpriseIndex = -1; // Index of the highest surprise nut (closest to top)
                int surpriseCount = 0;

                // Count and locate surprise nuts
                for (int k = 0; k < screw.Count; k++)
                {
                    if (screw[k] == surpriseNutId)
                    {
                        surpriseCount++;
                        screwHasSurprise = true;
                        if (firstSurpriseIndex == -1) {
                            firstSurpriseIndex = k;
                        }
                    }
                }

                // ULTRA PRIORITY: Exposed surprise nut at top WITH empty screws available
                if (screwHasSurprise && screw.Count > 0 && screw[screw.Count - 1] == surpriseNutId)
                {
                    // EXTREME bonus for having surprise at top - this should dominate other considerations
                    heuristic -= Mathf.RoundToInt(surpriseExposureBonus * 1.5f); 
                    
                    // With surprise nuts at top, we MUST have empty screws
                    if (emptyScewCount > 0)
                    {
                        // Super high priority to move exposed surprise to empty screw
                        float emptyBonus = useAggressiveEmptyPreference ? 250f * heuristicWeight : 200f * heuristicWeight;
                        heuristic -= Mathf.RoundToInt(emptyBonus);
                    }
                    else
                    {
                        // If no empty screws, this is a terrible situation for surprise nuts
                        heuristic += Mathf.RoundToInt(120f * heuristicWeight);
                    }
                }

                if (screwHasSurprise)
                {
                    // Penalize based on how many nuts are ABOVE the first surprise nut
                    int blockingNuts = 0;
                    for (int k = firstSurpriseIndex + 1; k < screw.Count; k++)
                    {
                        blockingNuts++;
                    }
                    // Increased blocking penalty - we want to expose surprise nuts ASAP
                    heuristic += Mathf.RoundToInt(blockingNuts * blockingPenalty * 1.5f);

                    // Apply reveal bonus if any surprise nut is close to being exposed
                    if (firstSurpriseIndex == screw.Count - 2)
                    { // Highest surprise nut is second from top
                        heuristic -= Mathf.RoundToInt(revealBonus * 1.3f);
                    }
                    else if (firstSurpriseIndex == screw.Count - 3) 
                    { // Surprise nut is third from top
                        heuristic -= Mathf.RoundToInt(revealBonus * 0.7f); // More bonus if it's 2 away from top
                    }
                    
                    // CRITICALLY IMPORTANT: If we have surprise nuts, we need empty screws
                    if (emptyScewCount == 0) {
                        // Extremely high penalty for having no empty screws when surprise nuts exist
                        heuristic += Mathf.RoundToInt(150f * heuristicWeight);
                    }
                    else {
                        // Increased bonus for each empty screw (we need somewhere to put surprise nuts)
                        float emptyBonus = useAggressiveEmptyPreference ? emptySpaceBonus * 2f : emptySpaceBonus * 1.5f;
                        heuristic -= Mathf.RoundToInt(emptyScewCount * emptyBonus);
                    }
                }
                // --- End Surprise Nut Logic ---

                // In surprise nut mode, we de-prioritize regular sorting
                if (!hasSurpriseNuts || !prioritizeSurpriseHandling) {
                    // --- Sequence Penalty - only if no surprise nuts or not prioritizing surprise handling ---
                    if (screw.Count > 0)
                    {
                        int topColor = screw[screw.Count - 1];
                        // If top is not surprise
                        if (topColor != surpriseNutId)
                        { 
                            int correctSequenceCount = 0;
                            for (int j = screw.Count - 1; j >= 0; j--)
                            {
                                if (screw[j] == surpriseNutId) break; // Stop sequence at surprise
                                if (screw[j] == topColor) correctSequenceCount++;
                                else break;
                            }
                            // Penalty for mixed colors in a stack - encourages proper sorting
                            heuristic += Mathf.RoundToInt((screw.Count - correctSequenceCount) * regularSortingWeight * 1.2f);
                        }
                    }

                    // --- Monochromatic / Completion Bonus - only if no surprise nuts or not prioritizing surprise ---
                    if (IsScrewMonochromatic(screw))
                    {
                        // If not just surprise nuts
                        if (screw.Count > 0 && screw[0] != surpriseNutId)
                        { 
                            // Higher bonus for any monochromatic stack
                            heuristic -= Mathf.RoundToInt(12f * regularSortingWeight);
                            
                            // Calculate bonus based on how full the stack is - encourages continuing to stack
                            float fillPercentage = screw.Count / (float)screwCapacity;
                            heuristic -= Mathf.RoundToInt(fillPercentage * 15f * regularSortingWeight);
                            
                            // Extra bonus for completed stacks
                            if (screw.Count == screwCapacity)
                            {
                                // Significantly increased bonus for completed stacks
                                heuristic -= Mathf.RoundToInt(completedStackBonus * 1.5f);
                            }
                            // Bonus for almost-completed stacks
                            else if (screw.Count >= screwCapacity * 0.75f)
                            {
                                heuristic -= Mathf.RoundToInt(nearlyCompletedStackBonus * (screw.Count / (float)screwCapacity) * 1.2f);
                            }
                        }
                    }
                    
                    // Bonus for direct path to completion - encourages minimalist solving
                    if (screw.Count > 0 && screw.Count < screwCapacity)
                    {
                        int topColor = screw[screw.Count - 1];
                        if (topColor != surpriseNutId)
                        {
                            // Check if making this a full stack would solve part of the puzzle
                            int sameColorCount = 0;
                            foreach (int color in screw)
                            {
                                if (color == topColor) sameColorCount++;
                            }
                            
                            // If we have more than half of what we need for a full stack
                            if (sameColorCount > (screwCapacity / 2) && sameColorCount < screwCapacity)
                            {
                                // And there are other screws with this color
                                if (colorToScrews.ContainsKey(topColor) && colorToScrews[topColor].Count > 1)
                                {
                                    // Give a direct path bonus - encourages solving one stack at a time
                                    heuristic -= Mathf.RoundToInt(directPathBonus * (sameColorCount / (float)screwCapacity));
                                }
                            }
                        }
                    }
                }
            } // End loop through screws

            return Mathf.Max(0, heuristic); // Ensure non-negative
        }

        // Generate a path from a goal node back to the start
        private List<(int, int)> GeneratePath(StateNode goalNode)
        {
            var path = new List<(int, int)>();
            StateNode current = goalNode;
            while (current != null && current.Move.HasValue) { path.Add(current.Move.Value); current = current.Parent; }
            path.Reverse();
            return path;
        }

        // Convert a state to a string representation
        public static string StateToString(List<List<int>> state)
        {
            try {
                List<string> screwStrings = state?
                    .Select(screw => "[" + string.Join(",", screw ?? new List<int>()) + "]") // Handle null screw list
                    .OrderBy(s => s)
                    .ToList() ?? new List<string>(); // Handle null state
                return string.Join(";", screwStrings);
            } catch (Exception ex) {
                Debug.LogError($"Error in StateToString: {ex.Message}");
                return "STATE_ERROR";
            }
        }

        // Find a move that breaks out of cycles by prioritizing moves to/from less frequently used screws
        private (int, int)? FindCycleBreakingMove(List<List<int>> state, int[] capacities)
        {
            // Count how many times each screw has been involved in recent moves
            Dictionary<int, int> screwUsageCounts = new Dictionary<int, int>();
            
            // Initialize counters
            for (int i = 0; i < state.Count; i++)
            {
                screwUsageCounts[i] = 0;
            }
            
            // Count usage in recent moves
            foreach (var move in lastMoves)
            {
                if (!screwUsageCounts.ContainsKey(move.Item1))
                    screwUsageCounts[move.Item1] = 0;
                if (!screwUsageCounts.ContainsKey(move.Item2))
                    screwUsageCounts[move.Item2] = 0;
                
                screwUsageCounts[move.Item1]++;
                screwUsageCounts[move.Item2]++;
            }
            
            // Find screws with the lowest usage count that can be used for a move
            List<(int, int, int)> candidateMoves = new List<(int, int, int)>();
            
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;
                
                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;
                    
                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        // Calculate a priority score based on how rarely these screws have been used
                        // Lower is better (we want to use less-used screws)
                        int usageScore = screwUsageCounts[sourceIdx] + screwUsageCounts[destIdx];
                        
                        // Check if this move was made recently (exact match)
                        bool isRecentMove = false;
                        foreach (var move in lastMoves)
                        {
                            if (move.Item1 == sourceIdx && move.Item2 == destIdx)
                            {
                                isRecentMove = true;
                                break;
                            }
                        }
                        
                        // Skip moves that were made very recently
                        if (isRecentMove) continue;
                        
                        // Add to candidates
                        candidateMoves.Add((sourceIdx, destIdx, usageScore));
                    }
                }
            }
            
            // If we found candidate moves, return the one with lowest usage score
            if (candidateMoves.Count > 0)
            {
                // Sort by usage score (ascending)
                candidateMoves.Sort((a, b) => a.Item3.CompareTo(b.Item3));
                return (candidateMoves[0].Item1, candidateMoves[0].Item2);
            }
            
            return null;
        }
        
        // Find a random valid move, optionally avoiding recent moves
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
                        
                        // Check if this move should be avoided (it's in the avoid list)
                        if (movesToAvoid != null)
                        {
                            foreach (var avoidMove in movesToAvoid)
                            {
                                if ((avoidMove.Item1 == sourceIdx && avoidMove.Item2 == destIdx) ||
                                    (avoidMove.Item1 == destIdx && avoidMove.Item2 == sourceIdx)) // Also avoid the reverse
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
                // Choose a random move from valid options
                int randomIndex = UnityEngine.Random.Range(0, validMoves.Count);
                return validMoves[randomIndex];
            }
            
            // If no moves can avoid the avoid list, but there are valid moves in general,
            // and we were trying to avoid moves, then try again without avoiding
            if (movesToAvoid != null && movesToAvoid.Count > 0)
            {
                return FindRandomValidMove(state, capacities);
            }
            
            return null;
        }

        // New method to find the best move for handling surprise nuts using greedy approach
        private (int, int)? FindBestSurpriseMoveGreedy(List<List<int>> state, int[] capacities)
        {
            // First priority: Move exposed surprise nuts to empty screws
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;
                
                // Check if top nut is a surprise
                if (sourceScrew[sourceScrew.Count - 1] == surpriseNutId)
                {
                    // Find an empty screw
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (state[destIdx].Count == 0 && CanMoveNut(state, sourceIdx, destIdx))
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }
            
            // Second priority: Unblock screws with buried surprise nuts
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count <= 1) continue;
                
                // Check if screw has buried surprise nuts
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
                    
                    // First try to move to empty screws
                    for (int destIdx = 0; destIdx < state.Count; destIdx++)
                    {
                        if (state[destIdx].Count == 0 && CanMoveNut(state, sourceIdx, destIdx))
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                    
                    // Then try to move to screws with matching top color
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
            
            // Third priority: Create an empty screw by completing a monochromatic stack
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;
                
                int topColor = sourceScrew[sourceScrew.Count - 1];
                if (topColor == surpriseNutId) continue;
                
                // Find a screw with matching color
                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (destIdx == sourceIdx) continue;
                    
                    var destScrew = state[destIdx];
                    if (destScrew.Count > 0 && 
                        destScrew[destScrew.Count - 1] == topColor &&
                        CanMoveNut(state, sourceIdx, destIdx))
                    {
                        // Check if source screw would be empty after move
                        if (sourceScrew.Count == 1)
                        {
                            return (sourceIdx, destIdx);
                        }
                        
                        // Check if this move would complete a stack
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

        // New method to find the best moves based on a greedy approach
        private (int, int)? FindGreedyMove(List<List<int>> state, int[] capacities)
        {
            // First priority: Complete a stack if possible
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
                        // Check if this move completes a stack
                        if (destIdx < capacities.Length && 
                            destScrew.Count == capacities[destIdx] - 1)
                        {
                            return (sourceIdx, destIdx);
                        }
                        
                        // Check if this move makes source empty (good for future options)
                        if (sourceScrew.Count == 1)
                        {
                            return (sourceIdx, destIdx);
                        }
                    }
                }
            }
            
            // Second priority: Move to grow homogeneous stacks
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                var sourceScrew = state[sourceIdx];
                if (sourceScrew.Count == 0) continue;
                
                int topColor = sourceScrew[sourceScrew.Count - 1];
                if (topColor == surpriseNutId) continue;
                
                // Find the destination with the most same-colored nuts
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
                        // Count same-colored nuts
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
            
            // Third priority: Move to empty screw (creates options)
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

        // Find a move that is not part of recent cycles
        private (int, int)? FindNonCyclicMove(List<List<int>> state, int[] capacities)
        {
            List<(int, int, float)> moveScores = new List<(int, int, float)>();
            
            // Generate all valid moves with a score
            for (int sourceIdx = 0; sourceIdx < state.Count; sourceIdx++)
            {
                if (state[sourceIdx].Count == 0) continue;
                
                for (int destIdx = 0; destIdx < state.Count; destIdx++)
                {
                    if (sourceIdx == destIdx) continue;
                    
                    if (CanMoveNut(state, sourceIdx, destIdx))
                    {
                        // Calculate a score for this move (lower is better)
                        float score = 0;
                        
                        // Check if move or its inverse has been made recently
                        foreach (var move in lastMoves)
                        {
                            if ((move.Item1 == sourceIdx && move.Item2 == destIdx) ||
                                (move.Item1 == destIdx && move.Item2 == sourceIdx))
                            {
                                score += 100; // Strong penalty for recent moves
                            }
                        }
                        
                        // Check if we're moving between similar screws
                        if (state[sourceIdx].Count > 0 && state[destIdx].Count > 0)
                        {
                            int sourceTop = state[sourceIdx][state[sourceIdx].Count - 1];
                            int destTop = state[destIdx][state[destIdx].Count - 1];
                            
                            if (sourceTop == destTop && sourceTop != surpriseNutId)
                            {
                                score += 50; // Penalty for moving between same-top screws
                            }
                        }
                        
                        // Favor moves that create empty screws
                        if (state[sourceIdx].Count == 1)
                        {
                            score -= 30; // Bonus for creating empty screws
                        }
                        
                        // Bonus for completing stacks
                        if (destIdx < capacities.Length && 
                            state[destIdx].Count == capacities[destIdx] - 1)
                        {
                            score -= 40; // Strong bonus for completing a stack
                        }
                        
                        moveScores.Add((sourceIdx, destIdx, score));
                    }
                }
            }
            
            // Sort by score (lower is better)
            moveScores.Sort((a, b) => a.Item3.CompareTo(b.Item3));
            
            // Take best scoring move
            if (moveScores.Count > 0)
            {
                return (moveScores[0].Item1, moveScores[0].Item2);
            }
            
            return null;
        }

        // New method to check if we're near a goal state
        private bool IsNearGoalState(List<List<int>> state, int[] capacities)
        {
            // First check if there are still surprise nuts
            bool hasSurpriseNuts = false;
            foreach (var screw in state)
            {
                if (screw.Contains(surpriseNutId))
                {
                    hasSurpriseNuts = true;
                    break;
                }
            }
            
            // If we have surprise nuts, we're not near the goal
            if (hasSurpriseNuts) return false;
            
            // Count completed screws and progress toward completion
            int completedScrews = 0;
            int totalScrews = state.Count;
            float totalProgress = 0f;
            
            for (int i = 0; i < state.Count; i++)
            {
                var screw = state[i];
                
                // Skip if capacity info is missing
                if (i >= capacities.Length) continue;
                
                // Check if screw is completed
                if (IsScrewSortedOrEmpty(screw, i, capacities))
                {
                    completedScrews++;
                    totalProgress += 1.0f;
                }
                else if (screw.Count > 0)
                {
                    // Calculate partial progress
                    int capacity = capacities[i];
                    int dominantColor = FindDominantColor(screw);
                    
                    if (dominantColor != -1) // -1 means no dominant color found
                    {
                        // Count nuts of dominant color
                        int dominantColorCount = 0;
                        foreach (int color in screw)
                        {
                            if (color == dominantColor) dominantColorCount++;
                        }
                        
                        // Progress is ratio of dominant color nuts to capacity
                        float screwProgress = dominantColorCount / (float)capacity;
                        totalProgress += screwProgress;
                    }
                }
            }
            
            // Average progress across all screws
            float averageProgress = totalProgress / totalScrews;
            
            // Consider "near goal" if either:
            // 1. Over 70% of screws are completely solved, or
            // 2. Average progress across all screws is over 85%
            return completedScrews > totalScrews * 0.7f || averageProgress > 0.85f;
        }

        // Helper to find the dominant color in a screw
        private int FindDominantColor(List<int> screw)
        {
            if (screw.Count == 0) return -1;
            
            Dictionary<int, int> colorCounts = new Dictionary<int, int>();
            
            // Count each color
            foreach (int color in screw)
            {
                if (color == surpriseNutId) continue; // Skip surprise nuts
                
                if (!colorCounts.ContainsKey(color))
                {
                    colorCounts[color] = 0;
                }
                colorCounts[color]++;
            }
            
            // Find the most common color
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
            
            // Only return a dominant color if it represents more than 60% of the nuts
            return maxCount > screw.Count * 0.6f ? dominantColor : -1;
        }
    }

    // Simple priority queue implementation
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