using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    // Reference to the UIDocument (which contains the UI elements)
    public UIDocument uiDocument;
    public AudioSource click;
    public AudioSource win;
    public AudioSource music;
    private int boardSize = 3;

    private VisualElement rootVisualElement;
    private Label lvlLabel;
    private int lvl = 1;
    private VisualElement _winPopUp;
    private int finalNr = 1000;
    private VisualElement playArea;
    private VisualElement[,] board;
    private Vector2 _lastClicked = new Vector2(-1, -1);
    private Stack<Vector2> _numberPositions = new Stack<Vector2>();


    private void OnEnable()
    {
        // Ensure the UIDocument is assigned
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned!");
            return;
        }

        // Get the root VisualElement of the UI
        rootVisualElement = uiDocument.rootVisualElement;


        lvlLabel = rootVisualElement.Q<Label>("lvl");
        _winPopUp = rootVisualElement.Q<VisualElement>("popUp");
        _winPopUp.Q<Button>().clicked += StartNewGame;
        rootVisualElement.Query<Button>("restart").First().clicked += StartNewGame;
        rootVisualElement.Query<Button>("clear").First().clicked += Clear;
        rootVisualElement.Query<Button>("help").First().clicked += Help;
        rootVisualElement.Query<Button>("music").First().clicked += TogleMusic;
        rootVisualElement.Query<Button>("sound").First().clicked += TogleAudio;

        playArea = rootVisualElement.Q<VisualElement>("playArea");

        StartNewGame();
    }

    // Method to handle button click events
    private void OnButtonClick(VisualElement button)
    {
        PlayClick();
        Vector2 pressed = GetIndexOf(board, button);
        int numberToBePlaced = _numberPositions.Count + 1;
        Label txt = button.Q<Label>();
        string labelText = txt.text;

        // Check if the text is a valid integer
        bool placeHasANumber = int.TryParse(labelText, out int placeNumber);
        bool isInitialHit = _numberPositions.Count == 0;
        Vector2 lastClicked = isInitialHit ? new Vector2(-1, -1) : _numberPositions.Peek();

        // Calculate adjacency based on the last number's position
        bool isAdjacent = Mathf.Abs(lastClicked.x - pressed.x) == 1 && lastClicked.y == pressed.y ||
                          lastClicked.x == pressed.x && Mathf.Abs(lastClicked.y - pressed.y) == 1;


        // Valid move: Initial hit or placing the next number in sequence at an adjacent position
        if (isInitialHit ||
            (placeHasANumber && placeNumber == numberToBePlaced && isAdjacent) ||
            (!placeHasANumber && isAdjacent))
        {
            if (placeHasANumber && placeNumber != numberToBePlaced)
            {
                return;
            }

            _numberPositions.Push(pressed); // Update stack with the current position
            txt.text = numberToBePlaced.ToString();
            txt.AddToClassList("confirmedNr");

            if (numberToBePlaced == finalNr)
            {
                Win();
            }

            return;
        }

        // Undo move: Clicking the last placed number or an adjacent number with the previous value
        if (_numberPositions.Count > 0 &&
            pressed == _numberPositions.Peek())
        {
            _numberPositions.Pop(); // Remove the last position from the stack
            if (!txt.ClassListContains("required"))
            {
                txt.text = "";
            }

            txt.RemoveFromClassList("confirmedNr");
        }
    }


    [ContextMenu("win")]
    private void Win()
    {
        PlayWin();
        _winPopUp.style.display = DisplayStyle.Flex;
        lvl++;
        lvlLabel.text = "Lvl" + lvl;
    }

    private void Clear()
    {
        PlayClick();
        while (_numberPositions.Count > 0)
        {
            Vector2 pos = _numberPositions.Pop();
            Label label = board[(int) pos.x, (int) pos.y].Q<Label>();
            if (!label.ClassListContains("required"))
            {
                label.text = "";
            }

            label.RemoveFromClassList("confirmedNr");
        }
    }
    

    private void Help()
    {
       
        MatrixFiller.SolvePuzzle(GetMatrixAsInt(board));
    }


    private void PlayAds()
    {
        //Aici Bogatu
    }

    private void StartNewGame()
    {
        _numberPositions.Clear();
        _winPopUp.style.display = DisplayStyle.None;
        _lastClicked = new Vector2(-1, -1);
        if (lvl < 5)
        {
            boardSize = 3;
        }
        else if (lvl < 10)
        {
            boardSize = 4;
        }
        else
        {
            boardSize = 5;
        }

        CreateBoard(boardSize);
        FillBoardWithNumbers();
    }

    private void ClearBoard()
    {
        int rows = board.GetLength(0); // Get number of rows
        int cols = board.GetLength(1); // Get number of columns

        // Clear all labels by setting them to an empty string
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Label label = board[i, j].Q<Label>(); // Get the Label component
                if (label != null)
                {
                    label.RemoveFromClassList("confirmedNr");
                    label.RemoveFromClassList("required");
                    label.text = ""; 
                }
            }
        }
    }

    private Vector2 FindStartPosition()
    {
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i,j].Q<Label>().text =="1")
                {
                    return new Vector2(i, j);
                }
            }
        }

        return new Vector2(-1, -1);
    }

    private int FindLastNumber()
    {
        return 1;
    }
    
    [ContextMenu("complete")]
    private void CompletePathAfterClear()
    {
        
    }

    private void CreateBoard(int size)
    {
        playArea.Clear();
        for (int i = 0; i < size * size; i++)
        {
            VisualElement btn = new VisualElement();
            btn.AddToClassList("button");
            btn.style.width = (1000 / size) - 5;
            btn.style.height = (1000 / size) - 5;
            Label label = new Label();
            label.AddToClassList("nrLabel");
            btn.Add(label);
            playArea.Add(btn);
            btn.RegisterCallback<ClickEvent>(e => OnButtonClick(btn));
        }

        ReadBoard();
    }

    private void ReadBoard()
    {
        var buttons = playArea.Query<VisualElement>().Class("button").ToList();
        board = ConvertListToMatrix(buttons);
    }

    private void FillBoardWithNumbers()
    {
        ClearBoard();

        int rows = board.GetLength(0); // Get number of rows
        int cols = board.GetLength(1); // Get number of columns

        // Minimum number of steps required for the path
        int minSteps = rows * cols - Random.Range(1, 4);

        // Create a list of all possible directions to move (up, down, left, right)
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0), // Down
            new Vector2(-1, 0), // Up
            new Vector2(0, 1), // Right
            new Vector2(0, -1) // Left
        };

        // Generate random start and end positions that are not the same
        Vector2 start;
        Vector2 end;
        do
        {
            start = new Vector2(Random.Range(0, rows), Random.Range(0, cols));
            end = new Vector2(Random.Range(0, rows), Random.Range(0, cols));
        } while (start == end);

        Vector2 currentPos = start;
        int currentNumber = 1;
        int maxNumbers = boardSize * boardSize;

        // List to track visited positions
        HashSet<Vector2> visited = new HashSet<Vector2>();
        visited.Add(currentPos);

        // Keep track of the number of steps taken
        int stepCount = 0;

        // Loop to create a path from start to end
        while ((currentPos != end || stepCount < minSteps) && currentNumber <= maxNumbers)
        {
            // Set the label text for the current position
            int row = (int) currentPos.x;
            int col = (int) currentPos.y;
            Label label = board[row, col].Q<Label>(); // Get the Label component
            if (label != null)
            {
                label.text = currentNumber.ToString(); // Set the text to the current number
                label.AddToClassList("required");
            }

            // Find possible moves
            List<Vector2> validMoves = new List<Vector2>();

            foreach (var direction in directions)
            {
                Vector2 nextPos = currentPos + direction;

                // Ensure the position is within bounds and hasn't been visited yet
                if (nextPos.x >= 0 && nextPos.x < rows && nextPos.y >= 0 && nextPos.y < cols &&
                    !visited.Contains(nextPos))
                {
                    validMoves.Add(nextPos);
                }
            }

            // If valid moves exist, choose one
            if (validMoves.Count > 0)
            {
                // Bias the direction slightly towards the endpoint to avoid the shortest path
                Vector2 bestMove = validMoves.OrderBy(move =>
                    Random.Range(0, 2) == 0 ? Vector2.Distance(move, end) : Random.Range(0, 100)).First();
                currentPos = bestMove;
                visited.Add(currentPos);
                stepCount++;
            }
            else
            {
                // No valid moves, end path generation
                break;
            }

            currentNumber++;
        }

        // Check if the path satisfies the minimum steps and end-point criteria
        if ((currentPos != end || stepCount < minSteps) && currentNumber <= maxNumbers)
        {
            FillBoardWithNumbers(); // Retry generation
        }
        else
        {
            ClearSomeSpaces(currentNumber - 1);
        }
    }


    private void ClearSomeSpaces(int maxNr)
    {
        finalNr = maxNr;
        int lvl = maxNr - Random.Range(3, 4);
        int deleted = 0;

        // Assuming board is a 2D array of VisualElement objects.
        while (deleted < lvl)
        {
            int x = Random.Range(0, board.GetLength(0));
            int y = Random.Range(0, board.GetLength(1));
            VisualElement place = board[x, y];

            // Get the Label component within the VisualElement
            Label label = place.Q<Label>();

            // Ensure the label is not empty and parse the number if possible
            if (label != null && label.text != "" && int.TryParse(label.text, out int parsedValue))
            {
                // Only delete if the parsed value is not equal to maxNr
                if (parsedValue != maxNr)
                {
                    label.RemoveFromClassList("required");
                    label.text = ""; // Clear the label's text
                    deleted++; // Increment the counter for deleted spaces
                }
            }
        }
    }

    public VisualElement[,] ConvertListToMatrix(List<VisualElement> inputList)
    {
        // Check if the list has a perfect square number of elements
        int count = inputList.Count;
        int size = (int) Mathf.Sqrt(count);

        if (size * size != count)
        {
            Debug.LogError($"List must contain a perfect square number of elements to form a {size}x{size} matrix.");
            return null;
        }

        // Create a square matrix with dimensions size x size
        VisualElement[,] matrix = new VisualElement[size, size];

        // Fill the matrix with the elements of the list
        for (int i = 0; i < count; i++)
        {
            int row = i / size; // Integer division for rows
            int col = i % size; // Modulo operation for columns
            matrix[row, col] = inputList[i];
        }

        return matrix;
    }


    private Vector2 GetIndexOf(VisualElement[,] matrix, VisualElement target)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j] == target)
                {
                    return new Vector2(i, j);
                }
            }
        }

        return new Vector2(-1, -1);
    }

    private int[,] GetMatrixAsInt(VisualElement[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        int[,] matrix = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                VisualElement element = board[i, j];
                string text = element?.Q<Label>()?.text; // Assuming the number is in a Label within the VisualElement

                if (int.TryParse(text, out int number))
                {
                    matrix[i, j] = number; // Assign the parsed number
                }
                else
                {
                    matrix[i, j] = 0; // Default to 0 if parsing fails or text is empty
                }
            }
        }

        return matrix;
    }

    #region audio

    private void PlayClick()
    {
        click.Play();
    }

    private void PlayWin()
    {
        win.Play();
    }

    private void TogleAudio()
    {
        if (win.volume == 0)
        {
            win.volume = 1;
            click.volume = 1;
        }
        else
        {
            win.volume = 0;
            click.volume = 0;
        }
    }

    private void TogleMusic()
    {
        if (music.volume == 0)
        {
            music.volume = 1;
        }
        else
        {
            music.volume = 0;
        }
    }

    #endregion
}