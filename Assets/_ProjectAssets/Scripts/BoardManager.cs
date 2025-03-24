using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    // Reference to the UIDocument (which contains the UI elements)
    public UIDocument uiDocument;
    public AudioManager audioManager;
    public GameManager gameManager;

    private int _boardSize = 3;

    private VisualElement _rootVisualElement;


    private int _finalNr = 1000;
    private VisualElement _playArea;
    private VisualElement[,] _board;
    private int[,] _int_board;
    private Vector2 _lastClicked = new Vector2(-1, -1);
    private Stack<Vector2> _placedNumbersPositions = new Stack<Vector2>();
    private List<Vector2> _visited = new List<Vector2>();


    private void OnEnable()
    {
        // Ensure the UIDocument is assigned
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned!");
            return;
        }

        // Get the root VisualElement of the UI
        _rootVisualElement = uiDocument.rootVisualElement;

        _playArea = _rootVisualElement.Q<VisualElement>("playArea");
        gameManager.OnEnableLate();
    }


    // Method to handle button click events
    private void OnButtonClick(VisualElement button)
    {
        audioManager.PlayClick();
        Vector2 buttonIndex = GetIndexOf(_board, button);
        int numberToBePlaced = _placedNumbersPositions.Count + 1;
        Label txt = button.Q<Label>();


        // Undo move
        if (_placedNumbersPositions.Count > 0 && buttonIndex == _placedNumbersPositions.Peek() && gameManager.level != 0)
        {
            RemoveNumber(txt);
            return;
        }


        // Check if the text is a valid integer
        bool placeHasANumber = int.TryParse(txt.text, out int placeNumber);
        bool isInitialHit = _placedNumbersPositions.Count == 0;
        Vector2 lastClicked = isInitialHit ? new Vector2(-1, -1) : _placedNumbersPositions.Peek();

        // Calculate adjacency based on the last number's position
        bool isAdjacent = Mathf.Abs(lastClicked.x - buttonIndex.x) == 1 && lastClicked.y == buttonIndex.y ||
                          lastClicked.x == buttonIndex.x && Mathf.Abs(lastClicked.y - buttonIndex.y) == 1;


        if (isInitialHit || //first nr
            (placeHasANumber && placeNumber == numberToBePlaced && isAdjacent) || //place a required nr
            (!placeHasANumber && isAdjacent)) //place a nr
        {
            _placedNumbersPositions.Push(buttonIndex);
            txt.text = numberToBePlaced.ToString();
            txt.AddToClassList("confirmedNr");

            if (numberToBePlaced == _finalNr && NoDoubleNumbers())
            {
                gameManager.Win();
            }
        }
    }


    private void RemoveNumber(Label txt)
    {
        _placedNumbersPositions.Pop(); // Remove the last position from the stack
        if (!txt.ClassListContains("required"))
        {
            txt.text = "";
        }

        txt.RemoveFromClassList("confirmedNr");
    }


    private bool NoDoubleNumbers()
    {
        HashSet<string> seenNumbers = new HashSet<string>();
        int rows = _board.GetLength(0);
        int cols = _board.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Label label = _board[i, j].Q<Label>();
                if (label != null && !string.IsNullOrEmpty(label.text))
                {
                    if (!seenNumbers.Add(label.text))
                    {
                        // Duplicate found
                        return false;
                    }
                }
            }
        }

        // No duplicates found
        return true;
    }


    public void ClearPlacedNumbers()
    {
        audioManager.PlayClick();
        while (_placedNumbersPositions.Count > 0)
        {
            Vector2 pos = _placedNumbersPositions.Pop();
            Label label = _board[(int) pos.x, (int) pos.y].Q<Label>();
            if (!label.ClassListContains("required"))
            {
                label.text = "";
            }

            label.RemoveFromClassList("confirmedNr");
        }
    }


    public void Help()
    {

        if (_visited.Count > 0)
        {
            List<Vector2> local = new List<Vector2>(_visited); // Copy _visited to avoid modifying while iterating

            while (local.Count > 0)
            {
                int index = Random.Range(0, local.Count); // Pick a random index
                Vector2 pos = local[index];

                Label label = _board[(int) pos.x, (int) pos.y].Q<Label>();

                // Check if the cell is empty before placing a number
                if (string.IsNullOrEmpty(label.text))
                {
                    label.text = _int_board[(int) pos.x, (int) pos.y].ToString();
                    label.AddToClassList("required"); // Mark it as required
                    _visited.Remove(pos); // Remove from visited since it's now placed
                    return;
                }

                local.RemoveAt(index); // Remove the checked position
            }
        }
    }


    private void ClearBoard()
    {
        int rows = _board.GetLength(0); // Get number of rows
        int cols = _board.GetLength(1); // Get number of columns

        // Clear all labels by setting them to an empty string
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Label label = _board[i, j].Q<Label>(); // Get the Label component
                if (label != null)
                {
                    label.RemoveFromClassList("confirmedNr");
                    label.RemoveFromClassList("required");
                    label.text = "";
                }
            }
        }

        _placedNumbersPositions.Clear();
        _lastClicked = new Vector2(-1, -1);
        _visited.Clear();
    }

    public void CreateBoard(int rows, int cols)
    {
        _boardSize = Mathf.Max(rows, cols); // Store the largest dimension for scaling
        _playArea.Clear();

        float containerWidth = _playArea.resolvedStyle.width;
        float containerHeight = _playArea.resolvedStyle.height;

        float spacing = 5f; // Adjust the spacing between elements

        // Calculate the max square size while maintaining spacing
        float totalSpacingX = (cols - 1) * spacing;
        float totalSpacingY = (rows - 1) * spacing;
        float squareSizeX = (containerWidth - totalSpacingX) / cols;
        float squareSizeY = (containerHeight - totalSpacingY) / rows;
        float squareSize = Mathf.Min(squareSizeX, squareSizeY); // Ensure squares

        for (int i = 0; i < rows * cols; i++)
        {
            VisualElement btn = new VisualElement();
            btn.AddToClassList("button");
        
            btn.style.width = squareSize;
            btn.style.height = squareSize;
        
            btn.style.marginRight = (i % cols == cols - 1) ? 0 : spacing;
            btn.style.marginBottom = (i / cols == rows - 1) ? 0 : spacing;

            Label label = new Label();
            label.AddToClassList("nrLabel");
            btn.Add(label);
        
            _playArea.Add(btn);
            btn.RegisterCallback<ClickEvent>(e => OnButtonClick(btn));
        }

        ReadBoard(rows, cols);
    }


    private void ReadBoard(int rows, int cols)
    {
        var buttons = _playArea.Query<VisualElement>().Class("button").ToList();
        _board = ConvertListToMatrix(buttons, rows, cols);
    }


    public void FillBoardWithNumbers()
    {
        ClearBoard();

        int rows = _board.GetLength(0);
        int cols = _board.GetLength(1);
        _int_board = new int[rows, cols];
        _visited.Clear(); // Ensure it's fresh

        int minSteps = rows * cols - Random.Range(1, 4);
        int maxNumbers = _boardSize * _boardSize;

        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0), // Down
            new Vector2(-1, 0), // Up
            new Vector2(0, 1), // Right
            new Vector2(0, -1) // Left
        };

        Vector2 start, end;
        do
        {
            start = GetRandomPosition(rows, cols);
            end = GetRandomPosition(rows, cols);
        } while (start == end);

        HashSet<Vector2> visited = new HashSet<Vector2> {start};
        Vector2 currentPos = start;
        int currentNumber = 1, stepCount = 0;

        while ((currentPos != end || stepCount < minSteps) && currentNumber <= maxNumbers)
        {
            _int_board[(int) currentPos.x, (int) currentPos.y] = currentNumber++;
            _visited.Add(currentPos); // ✅ Ensure positions are stored

            List<Vector2> validMoves = GetValidMoves(currentPos, directions, rows, cols, visited);
            if (validMoves.Count == 0) break;

            currentPos = validMoves.OrderBy(move =>
                Random.Range(0, 2) == 0 ? Vector2.Distance(move, end) : Random.Range(0, 100)).First();

            visited.Add(currentPos);
            stepCount++;
        }

        if ((currentPos != end || stepCount < minSteps) && currentNumber <= maxNumbers)
        {
            FillBoardWithNumbers(); // Retry generation
            return;
        }

        // Ensure the last required number is placed
        if (_visited.Count > 1)
        {
            RequireNumber(_visited.Last());
            _finalNr = _int_board[(int) _visited.Last().x, (int) _visited.Last().y];
            _visited.RemoveAt(_visited.Count - 1);
        }

        for (int i = 0; i < Random.Range(1, 3) && _visited.Count > 0; i++)
        {
            int index = Random.Range(0, _visited.Count);
            RequireNumber(_visited[index]);
            _visited.RemoveAt(index);
        }
    }


    private Vector2 GetRandomPosition(int rows, int cols)
    {
        return new Vector2(Random.Range(0, rows), Random.Range(0, cols));
    }

    private List<Vector2> GetValidMoves(Vector2 currentPos, Vector2[] directions, int rows, int cols,
        HashSet<Vector2> visited)
    {
        List<Vector2> validMoves = new List<Vector2>();

        foreach (var direction in directions)
        {
            Vector2 nextPos = currentPos + direction;
            if (nextPos.x >= 0 && nextPos.x < rows && nextPos.y >= 0 && nextPos.y < cols && !visited.Contains(nextPos))
            {
                validMoves.Add(nextPos);
            }
        }

        return validMoves;
    }


    private void RequireNumber(Vector2 pos)
    {
        _board[(int) pos.x, (int) pos.y].Q<Label>().text = _int_board[(int) pos.x, (int) pos.y].ToString();
        _board[(int) pos.x, (int) pos.y].Q<Label>().AddToClassList("required");
    }

    public VisualElement[,] ConvertListToMatrix(List<VisualElement> inputList, int rows, int cols)
    {
        if (inputList.Count != rows * cols)
        {
            Debug.LogError($"The list must contain exactly {rows * cols} elements to form a {rows}x{cols} matrix.");
            return null;
        }

        VisualElement[,] matrix = new VisualElement[rows, cols];

        for (int i = 0; i < inputList.Count; i++)
        {
            int row = i / cols; // Integer division for rows
            int col = i % cols; // Modulo operation for columns
            matrix[row, col] = inputList[i];
        }

        return matrix;
    }


    private Vector2 GetIndexOf<T>(T[,] matrix, T target)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (EqualityComparer<T>.Default.Equals(matrix[i, j], target))
                {
                    return new Vector2(i, j);
                }
            }
        }

        return new Vector2(-1, -1);
    }
}