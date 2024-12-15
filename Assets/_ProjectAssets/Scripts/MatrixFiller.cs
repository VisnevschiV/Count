using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MatrixFiller
{
    // Direction vectors for adjacency (up, down, left, right)
    private static readonly int[] RowOffsets = { -1, 1, 0, 0 };
    private static readonly int[] ColOffsets = { 0, 0, -1, 1 };

    // Solve the puzzle and print the solution
    public static int[,] SolvePuzzle(int[,] board)
    {
        int targetNumber = FindHighestNumber(board); // Determine the highest pre-defined number

        // Find the starting position of '1' or the first empty cell
        (int startRow, int startCol) = FindNumberPosition(board, 1);
        if (startRow == -1) // If '1' is not found, look for the first empty cell
        {
            (startRow, startCol) = FindFirstEmptyCell(board);
        }

        if (startRow == -1)
        {
            Debug.Log("No valid starting position found.");
            return null;
        }

        if (SolveBoard(board, startRow, startCol, 1, targetNumber))
        {
            Debug.Log($"Solved Board up to {targetNumber}:");
            PrintBoard(board); // Print the board for debugging
            return board;
        }
        else
        {
            Debug.Log($"No solution found up to {targetNumber}.");
        }

        return null;
    }

    // Find the highest pre-defined number on the board
    private static int FindHighestNumber(int[,] board)
    {
        int highestNumber = 0;

        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] > highestNumber)
                    highestNumber = board[row, col];
            }
        }

        return highestNumber;
    }

    // Find the position of a specific number on the board
    private static (int, int) FindNumberPosition(int[,] board, int number)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == number)
                    return (row, col);
            }
        }

        // Default to (-1, -1) if the number is not found
        return (-1, -1);
    }

    // Find the first empty cell in the board
    private static (int, int) FindFirstEmptyCell(int[,] board)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == 0)
                    return (row, col);
            }
        }

        // Default to (-1, -1) if no empty cell is found
        return (-1, -1);
    }

    // Recursive function to solve the board using DFS
    private static bool SolveBoard(int[,] board, int row, int col, int currentNumber, int targetNumber)
    {
        // Base case: Stop if the target number is reached
        if (currentNumber > targetNumber)
            return true;

        // If the current cell is already filled with the current number, move on to the next number
        if (board[row, col] == currentNumber)
            return SolveNextNumber(board, row, col, currentNumber + 1, targetNumber);

        // Otherwise, check if the cell is valid for placing the current number
        if (IsValidPlacement(board, row, col, currentNumber))
        {
            // Place the current number
            board[row, col] = currentNumber;

            // Try to solve the next number by recursively calling SolveBoard
            if (SolveNextNumber(board, row, col, currentNumber + 1, targetNumber))
                return true;

            // Backtrack: Remove the current number
            board[row, col] = 0;
        }

        return false;
    }

    // Explore all possible moves to place the next number in DFS
    private static bool SolveNextNumber(int[,] board, int row, int col, int currentNumber, int targetNumber)
    {
        // Explore all possible moves in the direction vectors
        foreach (var (newRow, newCol) in GetAdjacentCells(row, col))
        {
            // Make sure the new position is within bounds
            if (newRow >= 0 && newRow < board.GetLength(0) && newCol >= 0 && newCol < board.GetLength(1))
            {
                if (SolveBoard(board, newRow, newCol, currentNumber, targetNumber))
                    return true;
            }
        }

        return false;
    }

    // Get adjacent cells based on the direction vectors (up, down, left, right)
    private static IEnumerable<(int, int)> GetAdjacentCells(int row, int col)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            int newRow = row + RowOffsets[direction];
            int newCol = col + ColOffsets[direction];
            yield return (newRow, newCol);
        }
    }

    // Validate the placement of the current number in the given cell
    private static bool IsValidPlacement(int[,] board, int row, int col, int number)
    {
        // Check if the cell is within bounds
        if (row < 0 || row >= board.GetLength(0) || col < 0 || col >= board.GetLength(1))
            return false;

        // Check if the cell is empty or already matches the number being placed
        if (board[row, col] != 0 && board[row, col] != number)
            return false;

        // Ensure that the current number is adjacent to the previous number (if applicable)
        if (number > 1)
        {
            (int prevRow, int prevCol) = FindNumberPosition(board, number - 1);
            bool isAdjacent = (math.abs(row - prevRow) <= 1 && math.abs(col - prevCol) <= 1) && !(math.abs(row - prevRow) == 1 && math.abs(col - prevCol) == 1);
            if (!isAdjacent)
                return false;
        }

        return true;
    }

    // Print the board for debugging
    private static void PrintBoard(int[,] board)
    {
        string boardString = "";
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                boardString += board[row, col].ToString().PadLeft(3) + " ";
            }
            boardString += "\n";
        }
        Debug.Log(boardString);
    }
}
