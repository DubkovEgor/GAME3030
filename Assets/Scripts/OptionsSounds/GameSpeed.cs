using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    public float[] speeds = { 0.5f, 1f, 2f, 4f };
    private int currentSpeedIndex = 1;
    private bool isPaused = false;
    private float previousSpeed = 1f;
    private bool isTurbo = false;

    void Start()
    {
        SetSpeed(currentSpeedIndex);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        { 
            ToggleTurbo();
        }
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

    public void ToggleTurbo()
    {
        if (isTurbo)
        {
            isTurbo = false;
            Time.timeScale = speeds[currentSpeedIndex];
        }
        else
        {
            isTurbo = true;
            Time.timeScale = 12f;
        }
        isPaused = false;
    }

}
