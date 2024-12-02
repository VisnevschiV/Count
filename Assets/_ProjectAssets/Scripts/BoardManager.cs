using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    // Reference to the UIDocument (which contains the UI elements)
    public UIDocument uiDocument;

    public int boardSize=4;

    private VisualElement rootVisualElement;
    private Label lvlLabel;
    private int lvl = 1;
    private VisualElement _winPopUp;
    private int numberToBePlaced = 1;
    private int finalNr = 1000;
    private VisualElement playArea;
    private VisualElement[,] board;
    private Vector2 _lastClicked = new Vector2(-1, -1);

     private void OnEnable() {
        
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
        _winPopUp.Q<Button>().clicked +=StartNewGame;
        rootVisualElement.Query<Button>("restart").First().clicked += StartNewGame;
        playArea = rootVisualElement.Q<VisualElement>("playArea");

        StartNewGame();
    }

    // Method to handle button click events
    private void OnButtonClick(VisualElement button)
    {
        Vector2 pressed = GetIndexOf(board, button);

        Label txt = button.Q<Label>();
        string labelText = txt.text;

        // Check if the text is a valid integer
        bool placeHasANumber = int.TryParse(labelText, out int placeNumber);
        bool isInitialHit = _lastClicked == new Vector2(-1, -1);
        bool isAdjacent = Mathf.Abs(_lastClicked.x - pressed.x) == 1 && _lastClicked.y == pressed.y || 
                            _lastClicked.x == pressed.x && Mathf.Abs(_lastClicked.y - pressed.y) == 1;


        // Proceed with logic if the text is a valid integer or if the text is empty
        if (isInitialHit 
            ||
            (placeHasANumber && placeNumber == numberToBePlaced && isAdjacent)
            || 
            (!placeHasANumber && isAdjacent)
            )
        {
            _lastClicked = pressed;

            // Only set text if it's a valid move (or initially empty)
            txt.text = numberToBePlaced.ToString();
            numberToBePlaced++;
            txt.AddToClassList("confirmedNr");
            if (numberToBePlaced == finalNr + 1)
            {
                Win();
            
            }
            return;
        }

        if(_lastClicked == pressed
        || (isAdjacent && placeNumber == numberToBePlaced-1)
        ){
            if(!txt.ClassListContains("required")){
                txt.text = "";
            }
            numberToBePlaced--;
            txt.RemoveFromClassList("confirmedNr");
            _lastClicked = pressed;
            if(numberToBePlaced == 1){
                _lastClicked = new Vector2(-1,-1);
            }
        }

    }

    [ContextMenu("win")]
    private void Win(){
        _winPopUp.style.display = DisplayStyle.Flex;
        lvl++;
        lvlLabel.text = "Lvl"+lvl;
    }

    private void StartNewGame(){
        _winPopUp.style.display = DisplayStyle.None;
        numberToBePlaced = 1;
        _lastClicked = new Vector2(-1, -1);
        if(lvl<5){
            boardSize=3;
        }else if (lvl<10){
            boardSize = 4;
        }else {
            boardSize = 5;
        }
        CreateBoard(boardSize);
        FillBoardWithNumbers();
    }

    private void ClearBoard(){
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
                    label.text = ""; // Clear the label's text
                }
            }
        }
    }

    private void CreateBoard(int size){

        playArea.Clear();
        for(int i = 0; i < size*size; i++) {
            VisualElement btn = new VisualElement();
            btn.AddToClassList("button");
            btn.style.width = (1000/size) -5;
            btn.style.height = (1000/size) -5;
            Label label = new Label();
            label.AddToClassList("nrLabel");
            btn.Add(label);
            playArea.Add(btn);
            btn.RegisterCallback<ClickEvent>(e => OnButtonClick(btn));
        }
        ReadBoard();
    }

    private void ReadBoard(){
        var buttons = playArea.Query<VisualElement>().Class("button").ToList();
        board = ConvertListToMatrix(buttons);
    }

    private void FillBoardWithNumbers()
    {
        ClearBoard();

        int rows = board.GetLength(0); // Get number of rows
        int cols = board.GetLength(1); // Get number of columns
        // Create a list of all possible directions to move (up, down, left, right)
        Vector2[] directions = new Vector2[]
        {
        new Vector2(1, 0), // Down
        new Vector2(-1, 0), // Up
        new Vector2(0, 1), // Right
        new Vector2(0, -1) // Left
        };

        // Randomly select a starting position on the board
        int startRow = Random.Range(0, rows);
        int startCol = Random.Range(0, cols);
        Vector2 currentPos = new Vector2(startRow, startCol);

        // Initialize a counter for the numbers
        int currentNumber = 1;
        int maxNumbers = boardSize*boardSize;

        // List to track visited positions
        HashSet<Vector2> visited = new HashSet<Vector2>();
        visited.Add(currentPos);

        // Loop to place numbers
        while (currentNumber <= maxNumbers)
        {
            // Set the label text for the current position
            int row = (int)currentPos.x;
            int col = (int)currentPos.y;
            Label label = board[row, col].Q<Label>(); // Get the Label component
            if (label != null)
            {
                label.text = currentNumber.ToString(); // Set the text to the current number
                label.AddToClassList("required");
            }

            // Find a valid next position (adjacent and unvisited)
            List<Vector2> validMoves = new List<Vector2>();

            // Check all 4 directions
            foreach (var direction in directions)
            {
                Vector2 nextPos = currentPos + direction;

                // Ensure the position is within bounds and hasn't been visited yet
                if (nextPos.x >= 0 && nextPos.x < rows && nextPos.y >= 0 && nextPos.y < cols && !visited.Contains(nextPos))
                {
                    validMoves.Add(nextPos);
                }
            }

            // If we have valid moves, pick one randomly
            if (validMoves.Count > 0)
            {
                currentPos = validMoves[Random.Range(0, validMoves.Count)];
                visited.Add(currentPos);
            }
            else
            {
                // No valid moves, stop the loop (this shouldn't happen if there's enough space)
                break;
            }

            // Increment the number
            currentNumber++;
        }

        if(currentNumber > boardSize){
            ClearSomeSpaces(currentNumber - 1);
        }else{
            FillBoardWithNumbers();
        }
    }

    private void ClearSomeSpaces(int maxNr)
    {
        finalNr = maxNr;
        int lvl = Random.Range(6, maxNr);
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
        int size = (int)Mathf.Sqrt(count);

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
            int row = i / size;  // Integer division for rows
            int col = i % size;  // Modulo operation for columns
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
}
