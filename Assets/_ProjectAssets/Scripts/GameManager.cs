using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public AudioManager audioManager;
    public GameObject menu;
    
    
    [SerializeField]
    private BoardManager _boardManager;
    
    
    private VisualElement _rootVisualElement;
    private Label _lvlLabel;
    private VisualElement _winPopUp;
    private VisualElement _timeOverPopUp;
    
    private int _lvl = 1;
    
    private Timer _timer = new Timer();
    
    
    
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


        _lvlLabel = _rootVisualElement.Q<Label>("lvl");
        _winPopUp = _rootVisualElement.Q<VisualElement>("winPopUp");
        _timeOverPopUp = _rootVisualElement.Q<VisualElement>("timeOverPopUp");
        _winPopUp.Q<Button>().clicked += StartNewGame;
        _timeOverPopUp.Q<Button>().clicked += Home;
        _rootVisualElement.Query<Button>("restart").First().clicked += StartNewGame;
        _rootVisualElement.Query<Button>("clear").First().clicked += _boardManager.ClearPlacedNumbers;
        _rootVisualElement.Query<Button>("help").First().clicked += _boardManager.Help;
        _rootVisualElement.Query<Button>("music").First().clicked += audioManager.ToggleMusic;
        _rootVisualElement.Query<Button>("sound").First().clicked += audioManager.ToggleSound;
        
        StartNewGame();
        _timer.StartCountDown(_rootVisualElement.Q<Label>("timer"), 20);
        _timer.OnTimeExpired += TimeOver;
    }
    
    private void Update()
    {
        _timer?.Update(Time.deltaTime);
    }
    
    [ContextMenu("win")]
    public void Win()
    {
        audioManager.PlayWin();
        _winPopUp.style.display = DisplayStyle.Flex;
        _lvl++;
        _lvlLabel.text = "Lvl" + _lvl;
        _timer.AddTime(5);
        if (_lvl>10)
        {
            _timer.AddTime(5);
        }
        if (_lvl>15)
        {
            _timer.AddTime(5);
        }
        
    }
    
    private void Home()
    {
        menu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    private void StartNewGame()
    {
        _winPopUp.style.display = DisplayStyle.None;
        int boardSizeX, boardSizeY;

        if (_lvl < 5)
        {
            boardSizeX = 3;
            boardSizeY = 3;
        }
        else if (_lvl < 10)
        {
            boardSizeX = 4;
            boardSizeY = 4;
        }
        else
        {
            boardSizeX = Random.Range(3, 7);  // Random X size between 3 and 6
            boardSizeY = Random.Range(3, 7);   // Random Y size between 3 and 6
        }
    
        _boardManager.CreateBoard(boardSizeX, boardSizeY);
        _boardManager.FillBoardWithNumbers();
    }

    private void TimeOver()
    {
        if(PlayerPrefs.HasKey("HighScore"))
        {
            _timeOverPopUp.Q<Label>("record").text = "High Score: " + PlayerPrefs.GetInt("HighScore");
            if (_lvl > PlayerPrefs.GetInt("HighScore"))
            {
                PlayerPrefs.SetInt("HighScore", _lvl);
            }
        }
        else
        {
            _timeOverPopUp.Q<Label>("record").text = "High Score: " + _lvl;
            PlayerPrefs.SetInt("HighScore", _lvl);
        }
        _timeOverPopUp.style.display = DisplayStyle.Flex;
        _timeOverPopUp.Q<Label>("score").text = "Score: " + _lvl;
    }
}
