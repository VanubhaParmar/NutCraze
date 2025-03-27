using System.Collections.Generic;
using System.Linq;

namespace tag.editor
{
    public static class LevelDataValidator
    {
        // Method to check if the puzzle is solvable
        // Maximum recursion depth to prevent stack overflow
        private const int MAX_DEPTH = 1000;

        // Method to check if the puzzle is solvable

        public static void ValidateLevelData(List<List<string>> tubes)
        {
            if (IsSolvable(tubes))
            {
                UnityEngine.Debug.Log("<color=green>The level is solvable.</color>");
            }
            else
            {
                UnityEngine.Debug.Log("<color=red>The level is not solvable.</color>");
            }
        }


        private static bool IsSolvable(List<List<string>> tubes)
        {
            // Create a deep copy of the tubes to avoid modifying the original
            var workingTubes = tubes.Select(tube => new List<string>(tube)).ToList();

            // Use an iterative approach with a depth limit
            return BacktrackIterative(workingTubes);
        }

        // Iterative backtracking method to solve the puzzle
        private static bool BacktrackIterative(List<List<string>> tubes)
        {
            // Stack to simulate recursion
            var stack = new Stack<(List<List<string>> State, int Depth)>();
            stack.Push((tubes, 0));

            // Set to track visited states to prevent cycling
            var visitedStates = new HashSet<string>();

            while (stack.Count > 0)
            {
                var (currentTubes, depth) = stack.Pop();

                // Check depth limit to prevent stack overflow
                if (depth > MAX_DEPTH)
                    continue;

                // Convert current state to a unique string representation
                string stateKey = SerializeState(currentTubes);

                // Skip if this state has been visited
                if (visitedStates.Contains(stateKey))
                    continue;

                visitedStates.Add(stateKey);

                // Check if all tubes are sorted
                if (IsSolved(currentTubes))
                    return true;

                // Try moving nuts between tubes
                for (int i = 0; i < currentTubes.Count; i++)
                {
                    // Skip empty tubes
                    if (currentTubes[i].Count == 0)
                        continue;

                    string nutColor = currentTubes[i][currentTubes[i].Count - 1];

                    for (int j = 0; j < currentTubes.Count; j++)
                    {
                        // Skip moving to the same tube
                        if (i == j)
                            continue;

                        // Check if we can move the nut
                        if (CanMove(nutColor, currentTubes[j]))
                        {
                            // Create a copy of the current state
                            var newTubes = currentTubes.Select(tube => new List<string>(tube)).ToList();

                            // Move the nut
                            newTubes[j].Add(nutColor);
                            newTubes[i].RemoveAt(newTubes[i].Count - 1);

                            // Push the new state to the stack
                            stack.Push((newTubes, depth + 1));
                        }
                    }
                }
            }

            return false;
        }

        // Check if all tubes are solved (empty or single color)
        private static bool IsSolved(List<List<string>> tubes)
        {
            return tubes.All(tube => tube.Count == 0 || tube.Distinct().Count() == 1);
        }

        // Check if a nut can be moved to a tube
        private static bool CanMove(string nutColor, List<string> tube)
        {
            // Can move if the tube is empty or the top color matches
            return tube.Count == 0 || tube[tube.Count - 1] == nutColor;
        }

        // Create a unique string representation of the tube state
        private static string SerializeState(List<List<string>> tubes)
        {
            return string.Join("|", tubes.Select(tube => string.Join(",", tube)));
        }

        public static void TestPuzzleSolver()
        {
        }
    }
}
