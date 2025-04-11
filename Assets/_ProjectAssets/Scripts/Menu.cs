using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{

    public UIDocument uiDocument;
    public GameObject game;

    private VisualElement _practiceWindow;
    private VisualElement _menuWindow;

    void OnEnable()
    {
        // Ensure the UIDocument is assigned
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned!");
            return;
        }

        // Get the root VisualElement of the UI
        VisualElement rootVisualElement = uiDocument.rootVisualElement;
        rootVisualElement.Q<Button>("Play").clicked += PlayCompetitive;
        rootVisualElement.Q<Button>("Career").clicked += PlayCareer;
        rootVisualElement.Q<Button>("practiceButton").clicked += Practice;
        rootVisualElement.Q<Button>("Play").clicked += PlayClick;
        rootVisualElement.Q<Button>("home").clicked += Home;

        rootVisualElement.Q<Label>("record").text = "High Score: " +
                                                    (PlayerPrefs.HasKey("HighScore")
                                                        ? PlayerPrefs.GetInt("HighScore").ToString()
                                                        : "0");
        
        _practiceWindow = rootVisualElement.Q<VisualElement>("practice");
        _menuWindow = rootVisualElement.Q<VisualElement>("buttonWrapper");
    }

    private void PlayClick()
    {
        AudioManager.Instance.PlayClick();
    }

    private void Practice()
    {
        _practiceWindow.style.display = DisplayStyle.Flex;
        _menuWindow.style.display = DisplayStyle.None;
        if (PlayerPrefs.HasKey("HighScore"))
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            Button button = _practiceWindow.Q<Button>("easy");
            button.clicked += () => PlayLevel(1);
            button.RemoveFromClassList("unavailableButton");
            button.Q<Label>().RemoveFromHierarchy();
            if (highScore > 4)
            {
                button = _practiceWindow.Q<Button>("medium");
                button.clicked += () => PlayLevel(6);
                button.RemoveFromClassList("unavailableButton");
                button.Q<Label>().RemoveFromHierarchy();
            }

            if (highScore > 9)
            {
                button = _practiceWindow.Q<Button>("hard");
                button.clicked += () => PlayLevel(15);
                button.RemoveFromClassList("unavailableButton");
                button.Q<Label>().RemoveFromHierarchy();
            }

        }
    }
    
    private void Home()
    {
        _practiceWindow.style.display = DisplayStyle.None;
        _menuWindow.style.display = DisplayStyle.Flex;
    }

    private void PlayCompetitive()
    {
        game.GetComponent<GameManager>().timerActive = true;
        game.SetActive(true);
        gameObject.SetActive(false);
    }

    private void PlayCareer(){
        game.GetComponent<GameManager>().timerActive = false;
        game.SetActive(true);
        gameObject.SetActive(false);
    }

    private void PlayLevel(int lvl)
    {
        game.GetComponent<GameManager>().level = lvl;
        game.GetComponent<GameManager>().timerActive = false;
        gameObject.SetActive(false);
        game.SetActive(true);
        
    }
}
