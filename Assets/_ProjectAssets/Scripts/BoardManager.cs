using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private List<Vector2> _hintPositions = new List<Vector2>();
    private PathFinder _pathFinder;


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
        Vector2 buttonIndex = GetIndexOf(_board, button);
        int numberToBePlaced = _placedNumbersPositions.Count + 1;
        Label label = button.Q<Label>();

        // Check if the text is a valid integer
        bool placeHasANumber = int.TryParse(label.text, out int placeNumber);
        bool isInitialHit = _placedNumbersPositions.Count == 0;
        Vector2 lastClicked = isInitialHit ? new Vector2(-1, -1) : _placedNumbersPositions.Peek();

        // Calculate adjacency based on the last number's position
        bool isAdjacent = Mathf.Abs(lastClicked.x - buttonIndex.x) == 1 && lastClicked.y == buttonIndex.y ||
                          lastClicked.x == buttonIndex.x && Mathf.Abs(lastClicked.y - buttonIndex.y) == 1;


        if (isInitialHit || // First number
            (isAdjacent && placeHasANumber && placeNumber == numberToBePlaced) || // Place a required number
            (!placeHasANumber && isAdjacent)) // Place a number
        {
            audioManager.PlayClick();
            _placedNumbersPositions.Push(buttonIndex);
            label.text = numberToBePlaced.ToString();
            label.AddToClassList("confirmedNr");

            if (numberToBePlaced == _finalNr && NoDoubleNumbers())
            {
                gameManager.Win();
            }

            if (gameManager.tutorialActive)
            {
                TutorialStep();
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

    private void InitializePathFinder()
    {
        _pathFinder = new PathFinder(_board, _int_board, _finalNr);
    }

    
    public void Help()
    {
        // Check if there are any placed numbers
        if (_placedNumbersPositions.Count == 0)
        {
            Debug.Log("No numbers placed yet. Cannot provide a hint.");
            return;
        }

        // Get the player's current position and the next number to place
        Vector2 currentPos = _placedNumbersPositions.Peek();
        int nextNumber = _placedNumbersPositions.Count + 1;

        Debug.Log($"Current position: {currentPos}, Next number: {nextNumber}");

        // Use the PathFinder to check for a valid path
        _pathFinder.FindPath(_int_board);
        
    }

    public void TutorialStep()
    {
        if (_hintPositions.Count > 0)
        {
            int nextNumber = _placedNumbersPositions.Count + 1;

            foreach (Vector2 pos in _hintPositions)
            {
                Label label = _board[(int)pos.x, (int)pos.y].Q<Label>();

                if (string.IsNullOrEmpty(label.text) && _int_board[(int)pos.x, (int)pos.y] == nextNumber)
                {
                    TutorialAnimationButton(label.parent);
                    _hintPositions.Remove(pos);
                    return;
                }
            }
        }
    }
    
    private async void TutorialAnimationButton(VisualElement button)
    {
        button.style.backgroundColor = new StyleColor(Color.yellow);
        await Task.Delay(1000);
        button.style.backgroundColor = StyleKeyword.Null;
        await Task.Delay(1000);
        if (button.Q<Label>().text == "")
        {
            TutorialAnimationButton(button);
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
        _hintPositions.Clear();
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
        InitializePathFinder();
    }


    private void ReadBoard(int rows, int cols)
    {
        var buttons = _playArea.Query<VisualElement>().Class("button").ToList();
        _board = ConvertListToMatrix(buttons, rows, cols);
    }


    public void FillBoardWithNumbers()
    {
        ClearBoard();

        // Step 1: Initialize board data
        InitializeBoardData(out int rows, out int cols, out int minSteps, out int maxNumbers, out Vector2[] directions);

        // Step 2: Generate start and end positions
        Vector2 start, end;
        GenerateStartAndEndPositions(rows, cols, out start, out end);

        // Step 3: Generate a valid path
        if (!GeneratePath(start, end, rows, cols, directions, minSteps, maxNumbers))
        {
            FillBoardWithNumbers(); // Retry generation if path is invalid
            return;
        }

        // Step 4: Place required numbers
        PlaceRequiredNumbers();
    }

    private void InitializeBoardData(out int rows, out int cols, out int minSteps, out int maxNumbers, out Vector2[] directions)
    {
        rows = _board.GetLength(0);
        cols = _board.GetLength(1);
        _int_board = new int[rows, cols];
        _hintPositions.Clear();

        minSteps = rows * cols - Random.Range(1, 4);
        maxNumbers = _boardSize * _boardSize;

        directions = new Vector2[]
        {
            new Vector2(1, 0), // Down
            new Vector2(-1, 0), // Up
            new Vector2(0, 1), // Right
            new Vector2(0, -1) // Left
        };
    }

    private void GenerateStartAndEndPositions(int rows, int cols, out Vector2 start, out Vector2 end)
    {
        do
        {
            start = GetRandomPosition(rows, cols);
            end = GetRandomPosition(rows, cols);
        } while (start == end);
    }

    private bool GeneratePath(Vector2 start, Vector2 end, int rows, int cols, Vector2[] directions, int minSteps, int maxNumbers)
    {
        HashSet<Vector2> visited = new HashSet<Vector2> { start };
        Vector2 currentPos = start;
        int currentNumber = 1, stepCount = 0;

        while ((currentPos != end || stepCount < minSteps) && currentNumber <= maxNumbers)
        {
            _int_board[(int)currentPos.x, (int)currentPos.y] = currentNumber++;
            _hintPositions.Add(currentPos);

            List<Vector2> validMoves = GetValidMoves(currentPos, directions, rows, cols, visited);
            if (validMoves.Count == 0) return false;

            currentPos = validMoves.OrderBy(move =>
                Random.Range(0, 2) == 0 ? Vector2.Distance(move, end) : Random.Range(0, 100)).First();

            visited.Add(currentPos);
            stepCount++;
        }

        return ValidatePath(currentPos, end, stepCount, minSteps);
    }

    private bool ValidatePath(Vector2 currentPos, Vector2 end, int stepCount, int minSteps)
    {
        return currentPos == end && stepCount >= minSteps;
    }

    private void PlaceRequiredNumbers()
    {
        if (_hintPositions.Count > 1)
        {
            RequireNumber(_hintPositions.Last());
            _finalNr = _int_board[(int)_hintPositions.Last().x, (int)_hintPositions.Last().y];
            _hintPositions.RemoveAt(_hintPositions.Count - 1);
        }

        for (int i = 0; i < Random.Range(1, 3) && _hintPositions.Count > 0; i++)
        {
            int index = Random.Range(0, _hintPositions.Count);
            RequireNumber(_hintPositions[index]);
            _hintPositions.RemoveAt(index);
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