using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{

    public UIDocument uiDocument;
    public GameObject game;
    public TutorialData tutorialData;
    public AudioSource click;

    private VisualElement _tutorialWindow;
    private VisualElement _practice;
    private VisualElement _menu;
    private int _tutorialStep;

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
        rootVisualElement.Q<Button>("practiceButton").clicked += Practice;
        rootVisualElement.Q<Button>("HowTo").clicked += ActivateTutorial;
        rootVisualElement.Q<Button>("Next").clicked += TutorialNext;
        rootVisualElement.Q<Button>("Play").clicked += PlayClick;
        rootVisualElement.Q<Button>("HowTo").clicked += PlayClick;
        rootVisualElement.Q<Button>("Next").clicked += PlayClick;
        rootVisualElement.Q<Button>("home").clicked += Home;

        rootVisualElement.Q<Label>("record").text = "High Score: " +
                                                    (PlayerPrefs.HasKey("HighScore")
                                                        ? PlayerPrefs.GetInt("HighScore").ToString()
                                                        : "0");

        _tutorialWindow = rootVisualElement.Q<VisualElement>("tutorialWindow");
        _practice = rootVisualElement.Q<VisualElement>("practice");
        _menu = rootVisualElement.Q<VisualElement>("buttonWrapper");
    }

    private void PlayClick()
    {
        click.Play();
    }

    private void Practice()
    {
        _practice.style.display = DisplayStyle.Flex;
        _menu.style.display = DisplayStyle.None;
        if (PlayerPrefs.HasKey("HighScore"))
        {
            int highScore = PlayerPrefs.GetInt("HighScore");
            Button button = _practice.Q<Button>("easy");
            button.clicked += () => PlayLevel(1);
            button.RemoveFromClassList("unavailableButton");
            button.Q<Label>().RemoveFromHierarchy();
            if (highScore > 4)
            {
                button = _practice.Q<Button>("medium");
                button.clicked += () => PlayLevel(6);
                button.RemoveFromClassList("unavailableButton");
                button.Q<Label>().RemoveFromHierarchy();
            }

            if (highScore > 9)
            {
                button = _practice.Q<Button>("hard");
                button.clicked += () => PlayLevel(15);
                button.RemoveFromClassList("unavailableButton");
                button.Q<Label>().RemoveFromHierarchy();
            }

        }
    }
    
    private void Home()
    {
        _practice.style.display = DisplayStyle.None;
        _menu.style.display = DisplayStyle.Flex;
    }

    private void PlayCompetitive()
    {
        game.SetActive(true);
        gameObject.SetActive(false);
    }

    private void PlayLevel(int lvl)
    {
        game.GetComponent<GameManager>().level = lvl;
        gameObject.SetActive(false);
        game.SetActive(true);
        
    }

    private void ActivateTutorial(){
        _tutorialWindow.style.display = DisplayStyle.Flex;
        _tutorialStep=0;
        _tutorialWindow.Q<VisualElement>("img").style.backgroundImage = tutorialData.tutorialElements[_tutorialStep].sprite.texture;
        _tutorialWindow.Q<Label>().text = tutorialData.tutorialElements[_tutorialStep].text;
        _tutorialStep++;
    }

    private void TutorialNext(){
        if(_tutorialStep > tutorialData.tutorialElements.Count-1){
             _tutorialWindow.style.display = DisplayStyle.None;
             return;
        }
        _tutorialWindow.Q<VisualElement>("img").style.backgroundImage = tutorialData.tutorialElements[_tutorialStep].sprite.texture;
        _tutorialWindow.Q<Label>().text = tutorialData.tutorialElements[_tutorialStep].text;
        _tutorialStep++;
        
    }
}
