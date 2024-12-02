using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct tutorialElement{
    public string text;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "TutorialData", menuName ="Scriptable/TutorialData")]
public class TutorialData : ScriptableObject
{
    [SerializeField]
    public List<tutorialElement> tutorialElements;
}
