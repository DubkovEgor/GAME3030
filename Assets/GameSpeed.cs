using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    public float[] speeds = { 0.5f, 1f, 2f, 4f };
    private int currentSpeedIndex = 1;
    private bool isPaused = false;
    private float previousSpeed = 1f;

    void Start()
    {
        SetSpeed(currentSpeedIndex);
    }

    public void SetSpeed(int index)
    {
        if (index < 0 || index >= speeds.Length)
            return;

        currentSpeedIndex = index;
        Time.timeScale = speeds[index];
        isPaused = false;
    }

    public void Pause()
    {
        if (!isPaused)
        {
            previousSpeed = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            Time.timeScale = previousSpeed;
            isPaused = false;
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    //// Optional: cycle through speeds
    //public void NextSpeed()
    //{
    //    currentSpeedIndex++;
    //    if (currentSpeedIndex >= speeds.Length)
    //        currentSpeedIndex = 0;

    //    SetSpeed(currentSpeedIndex);
    //}
}
