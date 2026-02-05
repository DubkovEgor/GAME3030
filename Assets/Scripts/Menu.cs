using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject UI_Screen;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        UI_Screen.SetActive(false);
        Time.timeScale = 0f;


        SoundManager.Instance.PlayMusic("PauseMusic");

    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        UI_Screen.SetActive(true);
        Time.timeScale = 1f;
        SoundManager.Instance.PlayMusic("StartAge/StartAge2");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
        SoundManager.Instance.PlayMusic("MenuMusic");
    }

    public void GameScene()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;

        SoundManager.Instance.StopMusicAwake();
        SoundManager.Instance.PlayMusic("StartAge/StartAge2");
    }
    public void WinScene()
    {
        SceneManager.LoadScene(3);
        Time.timeScale = 1f;
        // SoundManager.Instance.PlayMusic("name");
    }
    public void LoseScene()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1f;
        // SoundManager.Instance.PlayMusic("name");
    }

    public void QuitGame()
    {
        Application.Quit();

    }

    public void OpenPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    public void HardLevel()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;
        SoundManager.Instance.PlayMusic("GameMusic");
    }


}
