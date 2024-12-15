using System.Collections.Generic;
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

    // Recursive function to solve the board
    private static bool SolveBoard(int[,] board, int row, int col, int currentNumber, int targetNumber)
    {
        // Base case: Stop if the target number is reached
        if (currentNumber > targetNumber)
            return true;

        // Check if the current cell is valid for placement
        if (!IsValidPlacement(board, row, col, currentNumber))
            return false;

        // Place the current number
        board[row, col] = currentNumber;

        // Explore all possible moves
        for (int direction = 0; direction < 4; direction++)
        {
            int newRow = row + RowOffsets[direction];
            int newCol = col + ColOffsets[direction];

            if (SolveBoard(board, newRow, newCol, currentNumber + 1, targetNumber))
                return true;
        }

        // Backtrack: Remove the current number
        board[row, col] = 0;
        return false;
    }

    private static bool IsValidPlacement(int[,] board, int row, int col, int number)
    {
        // Check if the cell is within bounds
        if (row < 0 || row >= board.GetLength(0) || col < 0 || col >= board.GetLength(1))
            return false;

        // Check if the cell is empty or already matches the number being placed
        return board[row, col] == 0 || board[row, col] == number;
    }

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
