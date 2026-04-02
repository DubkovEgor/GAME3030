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

        SoundManager.Instance.PlaySFX("MenuButton");


        SoundManager.Instance.PlayMusic("PauseMusic");

    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        UI_Screen.SetActive(true);
        Time.timeScale = 1f;

        SoundManager.Instance.PlaySFX("MenuButton");
        SoundManager.Instance.PlayMusic("StartAge/StartAge2");
    }

    public void MainMenu()
    {
        LoadingManager.Instance.gameObject.SetActive(true);
        // SceneManager.LoadScene(0);

        Time.timeScale = 1f;
        SoundManager.Instance.PlaySFX("MenuButton");
        SoundManager.Instance.PlayMusic("MenuMusic");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.gameObject.SetActive(true);
            LoadingManager.Instance.LoadScene(0);
        }
    }

    public void GameScene()
    {
        Time.timeScale = 1f;

        SoundManager.Instance.StopMusicAwake();
        SoundManager.Instance.PlaySFX("MenuButton");
        SoundManager.Instance.PlayMusic("StartAge/StartAge2");

        if (LoadingManager.Instance == null)
        {
            Debug.LogError("LoadingManager is missing in the scene!");
            return;
        }

        LoadingManager.Instance.gameObject.SetActive(true);
        LoadingManager.Instance.LoadScene(1);
    }
    public void WinScene()
    {
        LoadingManager.Instance.gameObject.SetActive(true);
        // SceneManager.LoadScene(3);
        Time.timeScale = 1f;

        SoundManager.Instance.PlaySFX("MenuButton");
        // SoundManager.Instance.PlayMusic("name");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.gameObject.SetActive(true);
            LoadingManager.Instance.LoadScene(3);
        }
    }
    public void LoseScene()
    {
        LoadingManager.Instance.gameObject.SetActive(true);
        // SceneManager.LoadScene(2);
        Time.timeScale = 1f;

        SoundManager.Instance.PlaySFX("MenuButton");
        // SoundManager.Instance.PlayMusic("name");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.gameObject.SetActive(true);
            LoadingManager.Instance.LoadScene(2);
        }
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

            SoundManager.Instance.PlaySFX("MenuButton");
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
        LoadingManager.Instance.gameObject.SetActive(true);
        // SceneManager.LoadScene(1);
        Time.timeScale = 1f;

        SoundManager.Instance.PlaySFX("MenuButton");
        SoundManager.Instance.PlayMusic("GameMusic");

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.gameObject.SetActive(true);
            LoadingManager.Instance.LoadScene(1);
        }
    }


}
