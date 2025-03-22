using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public AudioManager audioManager;
    public GameObject menu;

    public int level = 0;
    
    [SerializeField]
    private BoardManager _boardManager;
    
    
    private VisualElement _rootVisualElement;
    private Label _lvlLabel;
    private Label _restartsNrLabel;
    private VisualElement _winPopUp;
    private VisualElement _timeOverPopUp;
    
    private int _lvl = 1;
    private int _restartsAvaialble = 3;
    
    private Timer _timer = new Timer();
    
    
    
    public void OnEnableLate()
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
        _rootVisualElement.Query<Button>("restart").First().clicked += RestartLevelOnClick;
        _rootVisualElement.Query<Button>("clear").First().clicked += _boardManager.ClearPlacedNumbers;
        _rootVisualElement.Query<Button>("help").First().clicked += _boardManager.Help;
        _rootVisualElement.Query<Button>("music").First().clicked += audioManager.ToggleMusic;
        _rootVisualElement.Query<Button>("sound").First().clicked += audioManager.ToggleSound;
        _restartsNrLabel= _rootVisualElement.Query<Label>("restartsAvailable");
        
        _restartsAvaialble = 3;
        _restartsNrLabel.text =_restartsAvaialble.ToString();
        StartNewGame();
        if (level == 0)
        {
            _timer.StartCountDown(_rootVisualElement.Q<Label>("timer"), 20);
            _timer.OnTimeExpired += TimeOver;
            _lvlLabel.text = "Lvl" + _lvl;
        }
        else
        {
            _rootVisualElement.Q<Label>("timer").text = "";
            _lvlLabel.text = "";
        }
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
        if (level==0)
        {
            _lvlLabel.text = "Lvl" + _lvl;
        }

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
        level = 0;
    }

    private void RestartLevelOnClick()
    {
        
        if (_restartsAvaialble > 0)
        {
            StartNewGame();
            _restartsAvaialble--;
            _restartsNrLabel.text =_restartsAvaialble.ToString();
        }
        
    }
    
    private void StartNewGame()
    {
        _winPopUp.style.display = DisplayStyle.None;
        int boardSizeX, boardSizeY;

        if (level>0)
        {
            _lvl = level;
        }

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
        audioManager.PlayGameOver();
    }
}
