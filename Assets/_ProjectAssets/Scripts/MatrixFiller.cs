using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MatrixFiller
{
    // Solve the puzzle and print the solution
    public static int[,] SolvePuzzle(int[,] board)
    {
        var predefinedNumbers = FindAllPreDefinedNumbers(board);
        List<Stack<Vector2>> paths = new List<Stack<Vector2>>();

        int startNr = 0;
        while (predefinedNumbers[startNr] + 1 == predefinedNumbers[startNr + 1])
        {
            startNr++;
        }
        
        FindPath(predefinedNumbers[startNr], predefinedNumbers[startNr + 1], board);
        return null;
    }

    private static List<Stack<Vector2>> FindPath(int start, int end, int[,] board)
    {
        int steps = end - start;
        Vector2 startPos = FindNumberPosition(board, start);
        Vector2 endPos = FindNumberPosition(board, end);
        Debug.Log( startPos +" to " + endPos);
        return GetPaths(startPos, endPos, steps, start, board, new Stack<Vector2>());
    }

    private static List<Stack<Vector2>> GetPaths(Vector2 start, Vector2 end, int steps, int currentNr, int[,] board, Stack<Vector2> stack)
    {
        List<Stack<Vector2>> successfulPaths = new List<Stack<Vector2>>(); // List to store successful paths
        stack.Push(start);

        // Base case: if no steps left, check if the start position is the same as the end position
        if (steps == 0)
        {
            if (start == end)
            {
                // Copy the stack to avoid reference issues
                successfulPaths.Add(new Stack<Vector2>(stack.Reverse()));
            }
            stack.Pop();
            return successfulPaths;
        }

        // Find available directions
        List<Vector2> directions = FindAvailableDirections(start, board, currentNr);

        // Explore each direction
        foreach (var direction in directions)
        {
            Vector2 nextPos = start + direction;
            if (!stack.Contains(nextPos))
            {
                // Recursively call GetPaths and collect all successful paths
                List<Stack<Vector2>> nextPaths = GetPaths(nextPos, end, steps - 1, currentNr + 1, board, new Stack<Vector2>(stack.Reverse()));
                successfulPaths.AddRange(nextPaths); // Add all successful paths from the recursive call
            }
        }

        stack.Pop();
        return successfulPaths;
    }

    private static List<Vector2> FindAvailableDirections(Vector2 pos, int[,] board, int currentValue)
    {
        List<Vector2> directions = new List<Vector2>();

        if (pos.x > 0) // Up
        {
            if (board[(int)pos.x - 1, (int)pos.y] == currentValue + 1)
            {
                return new List<Vector2> { new Vector2(-1, 0) };
            }
            if (board[(int)pos.x - 1, (int)pos.y] == 0)
            {
                directions.Add(new Vector2(-1, 0));
            }
        }

        if (pos.y > 0) // Left
        {
            if (board[(int)pos.x, (int)pos.y - 1] == currentValue + 1)
            {
                return new List<Vector2> { new Vector2(0, -1) };
            }
            if (board[(int)pos.x, (int)pos.y - 1] == 0)
            {
                directions.Add(new Vector2(0, -1));
            }
        }

        if (pos.x < board.GetLength(0) - 1) // Down
        {
            if (board[(int)pos.x + 1, (int)pos.y] == currentValue + 1)
            {
                return new List<Vector2> { new Vector2(1, 0) };
            }
            if (board[(int)pos.x + 1, (int)pos.y] == 0)
            {
                directions.Add(new Vector2(1, 0));
            }
        }

        if (pos.y < board.GetLength(1) - 1) // Right
        {
            if (board[(int)pos.x, (int)pos.y + 1] == currentValue + 1)
            {
                return new List<Vector2> { new Vector2(0, 1) };
            }
            if (board[(int)pos.x, (int)pos.y + 1] == 0)
            {
                directions.Add(new Vector2(0, 1));
            }
        }

        return directions;
    }

    private static List<int> FindAllPreDefinedNumbers(int[,] board)
    {
        List<int> predefined = new List<int>();
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] > 0)
                    predefined.Add(board[row, col]);
            }
        }
        predefined.Sort();
        return predefined;
    }

    private static Vector2 FindNumberPosition(int[,] board, int number)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == number)
                {
                    return new Vector2(row, col);
                }
            }
        }

        return new Vector2(-1, -1);
    }
}
