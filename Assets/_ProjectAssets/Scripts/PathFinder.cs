using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using System;

public class PathFinder
{
    private VisualElement[,] _board;
    private int[,] _intBoard;
    int rows , cols;

    List<Vector2> path;

    private Vector2[] directions = {
            new Vector2(0, 1),  // Right
            new Vector2(0, -1), // Left
            new Vector2(1, 0),  // Down
            new Vector2(-1, 0)  // Up
        };


    public PathFinder(VisualElement[,] board, int[,] intBoard, int finalNumber)
    {
        _board = board;
    }


    public List<Vector2> FindPath(int[,] intBoard)
    {
        rows = intBoard.GetLength(0);
        cols = intBoard.GetLength(1);
        path = new List<Vector2>();
        _intBoard = intBoard;
        int maxNumber = FindMaxNumber(intBoard);   
                

        HashSet<Vector2> visited = new HashSet<Vector2>();
        if (Backtrack(StartPosition(intBoard), 1, visited, maxNumber))
        {
            DebugPrintIntBoardAndPath(intBoard, path);
            return path; // Return the path if found
        }
                
        Debug.Log("No path found.");
        return null; // No path found
    }

    // Backtracking function to find the path
        bool Backtrack(Vector2 currentPos, int currentNumber, HashSet<Vector2> visited, int maxNumber)
        {
            if (currentNumber >= maxNumber)
            {
                return true; // Path is complete
            }

            // Check if the current position matches the current number
            if (_intBoard[(int)currentPos.x, (int)currentPos.y] != currentNumber)
            {
                return false;
            }

            // Mark the current position as visited
            visited.Add(currentPos);
            path.Add(currentPos);

            // Explore all possible moves (up, down, left, right)
            foreach (var direction in directions)
            {
                Vector2 nextPos = currentPos + direction;
                if (IsValidMove(nextPos, visited) && Backtrack(nextPos, currentNumber + 1, visited, maxNumber))
                {
                    return true;
                }
            }

            // Backtrack: undo the move
            visited.Remove(currentPos);
            path.RemoveAt(path.Count - 1);
            return false;
        }

    private int FindMaxNumber(int[,] intBoard)
    {
        int maxNumber = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (intBoard[i, j] > maxNumber)
                {
                    maxNumber = intBoard[i, j];
                }
            }
        }
        return maxNumber;
    }

    private Vector2 StartPosition(int[,] intBoard)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (intBoard[i, j] == 1)
                {
                    return new Vector2(i, j);
                }
            }
        }
        return new Vector2(-1, -1); // Return an invalid position if not found
    }

    bool IsValidMove(Vector2 pos, HashSet<Vector2> visited)
    {
            return pos.x >= 0 && pos.x < rows &&
                   pos.y >= 0 && pos.y < cols &&
                   !visited.Contains(pos);
    }
    

    private void DebugPrintIntBoardAndPath(int[,] intBoard, List<Vector2> path)
    {
        // Print the intBoard
        Debug.Log("intBoard:");
        for (int i = 0; i < intBoard.GetLength(0); i++)
        {
            string row = "";
            for (int j = 0; j < intBoard.GetLength(1); j++)
            {
                row += intBoard[i, j] + " ";
            }
            Debug.Log(row.Trim());
        }

        // Print the path
        if (path != null && path.Count > 0)
        {
            Debug.Log("Path:");
            foreach (var position in path)
            {
                Debug.Log($"({position.x}, {position.y})");
            }
        }
        else
        {
            Debug.Log("Path: No path found.");
        }
    }

}