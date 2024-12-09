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
    private int _tutorialStep;
    void Start()
    {
        // Ensure the UIDocument is assigned
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned!");
            return;
        }

        // Get the root VisualElement of the UI
        VisualElement rootVisualElement = uiDocument.rootVisualElement;
        rootVisualElement.Q<Button>("Play").clicked += Play;
        rootVisualElement.Q<Button>("HowTo").clicked += ActivateTutorial;
        rootVisualElement.Q<Button>("Next").clicked += TutorialNext;
        rootVisualElement.Q<Button>("Play").clicked += PlayClick;
        rootVisualElement.Q<Button>("HowTo").clicked += PlayClick;
        rootVisualElement.Q<Button>("Next").clicked += PlayClick;
        _tutorialWindow = rootVisualElement.Q<VisualElement>("tutorialWindow");
    }

    private void PlayClick()
    {
        click.Play();
    }

    private void Play(){
        game.SetActive(true);
        this.gameObject.SetActive(false);
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
