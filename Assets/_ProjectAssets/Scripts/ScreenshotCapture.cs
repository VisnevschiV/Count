using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    public string screenshotName = "Screenshot";
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Apasă 'P' pentru a face captura
        {
            string path = Application.dataPath + $"/{screenshotName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"Screenshot saved to: {path}");
        }
    }

    [ContextMenu("ss")]
    public void ScreenShot()
    {
        string path = Application.dataPath + $"/{screenshotName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"Screenshot saved to: {path}");
    }
}
