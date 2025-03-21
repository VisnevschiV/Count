using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Timer 
{
    private Label _timerLabel;
    private float _remainingTime;
    private bool _isRunning;
    
    public event Action OnTimeExpired;

    public void StartCountDown(Label timerLabel, int seconds)
    {
        _isRunning = true;
        _timerLabel = timerLabel;
        _remainingTime = seconds;
        UpdateLabel();
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void AddTime(int seconds)
    {
        _remainingTime += seconds;
        UpdateLabel();
    }

    public void Update(float deltaTime)
    {
        if (!_isRunning) return;

        _remainingTime -= deltaTime;
        UpdateLabel();

        if (_remainingTime <= 0)
        {
            _remainingTime = 0;
            _isRunning = false;
            OnTimeExpired?.Invoke();
        }
    }

    private void UpdateLabel()
    {
        int minutes = Mathf.FloorToInt(_remainingTime / 60);
        int seconds = Mathf.CeilToInt(_remainingTime % 60);

        if (minutes > 0)
            _timerLabel.text = $"{minutes}:{seconds:D2}"; // Shows as "m:ss"
        else
            _timerLabel.text = seconds.ToString(); // Shows just "s" when less than a minute
    }
}