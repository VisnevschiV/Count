using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Timer 
{
    private Label _timerLabel;
    public float remainingTime;
    private bool _isRunning;
    
    public event Action OnTimeExpired;

    public void StartCountDown(Label timerLabel, int seconds)
    {
        _isRunning = true;
        _timerLabel = timerLabel;
        remainingTime = seconds;
        UpdateLabel();
    }

    public void Stop()
    {
        _isRunning = false;
    }
    
    public void Continue()
    {
        _isRunning = true;
    }

    public void AddTime(int seconds)
    {
        remainingTime += seconds;
        UpdateLabel();
        AnimateColor(Color.green);
    }

    public void Update(float deltaTime)
    {
        if (!_isRunning) return;

        remainingTime -= deltaTime;
        UpdateLabel();

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            _isRunning = false;
            OnTimeExpired?.Invoke();
        }
    }

    private void UpdateLabel()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.CeilToInt(remainingTime % 60);
        
        if (minutes > 0){
            if (_timerLabel.text != $"{minutes}:{seconds:D2}")
            {
                AnimateFontSize();
                AudioManager.Instance.PlayTimer();
            }
            _timerLabel.text = $"{minutes}:{seconds:D2}"; 
        }
        else
        {
            if (_timerLabel.text != seconds.ToString())
            {
               AnimateFontSize();
               ShakeLabel();
               AudioManager.Instance.PlayTimer();
               if (seconds < 10)
               {
                   AnimateColor(Color.red);
               }
            }
            _timerLabel.text = seconds.ToString();
        }

       
    }
    
    private async void AnimateFontSize()
    {
        // Remove any existing transition classes
        _timerLabel.RemoveFromClassList("font-shrink");
        _timerLabel.AddToClassList("font-grow");

        // Increase font size
        _timerLabel.style.fontSize = _timerLabel.resolvedStyle.fontSize - 10;

        await Task.Delay(900);

        // Switch to shrink transition
        _timerLabel.RemoveFromClassList("font-grow");
        _timerLabel.AddToClassList("font-shrink");

        // Decrease font size
        _timerLabel.style.fontSize = _timerLabel.resolvedStyle.fontSize + 10;
    }
    
    private void ShakeLabel()
    {
        float shakeAngle = 5f; // Rotation angle
        float duration = 0.15f; // Total duration

        // First shake (left)
        _timerLabel.style.rotate = new Rotate(new Angle(-shakeAngle));
        _timerLabel.schedule.Execute(() =>
        {
            // Second shake (right)
            _timerLabel.style.rotate = new Rotate(new Angle(shakeAngle));
        }).StartingIn((long)(duration / 3 * 1000));

        _timerLabel.schedule.Execute(() =>
        {
            // Reset to normal
            _timerLabel.style.rotate = new Rotate(new Angle(0));
        }).StartingIn((long)(2 * duration / 3 * 1000));
    }
    
    private void AnimateColor(Color targetColor)
    {
        // Change to green
        _timerLabel.style.color = targetColor;
        
        // Revert after 0.3s
        _timerLabel.schedule.Execute(() =>
        {
            _timerLabel.style.color =  StyleKeyword.Null;
        }).StartingIn(300);
    }

}