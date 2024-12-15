using System.Collections.Generic;
using UnityEngine;

public class MatrixFiller
{
    // Direction vectors for adjacency (up, down, left, right)
    private static readonly int[] RowOffsets = { -1, 1, 0, 0 };
    private static readonly int[] ColOffsets = { 0, 0, -1, 1 };

    // Solve the puzzle and print the solution
    public static void SolvePuzzle(int[,] board)
    {
        int maxNumber = board.GetLength(0) * board.GetLength(1); // Maximum number to place

        if (SolveBoard(board, 1, maxNumber))
        {
            Debug.Log("Solved Board:");
            PrintBoard(board);
        }
        else
        {
            Debug.Log("No solution found.");
        }
    }

    // Recursive function to solve the board
    private static bool SolveBoard(int[,] board, int currentNumber, int maxNumber)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                // Find the starting position or continue placing numbers
                if (board[row, col] == currentNumber - 1 || (currentNumber == 1 && board[row, col] == 0))
                {
                    if (TryPlaceNumber(board, row, col, currentNumber, maxNumber))
                        return true;
                }
            }
        }

        return false; // No solution found
    }

    private static bool TryPlaceNumber(int[,] board, int row, int col, int currentNumber, int maxNumber)
    {
        // Base case: All numbers placed
        if (currentNumber > maxNumber)
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

            if (TryPlaceNumber(board, newRow, newCol, currentNumber + 1, maxNumber))
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
