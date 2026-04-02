using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [SerializeField] private CanvasGroup loadingCanvas;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMPro.TextMeshProUGUI progressText;
    [SerializeField] private float minimumLoadTime = 2.0f;
    private float displayedProgress = 0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        ShowLoadingUI();

        yield return FadeIn();
        displayedProgress = 0f;
        loadingSlider.value = 0f;
        progressText.text = "Loading... 0%";

        float timer = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float targetProgress = op.progress / 0.9f;

            displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, 5f * Time.unscaledDeltaTime);

            loadingSlider.value = displayedProgress;
            progressText.text = $"Loading... {(displayedProgress * 100f):F0}%";

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        while (timer < minimumLoadTime)
        {
            timer += Time.unscaledDeltaTime;

            displayedProgress = Mathf.Lerp(displayedProgress, 1f, 3f * Time.unscaledDeltaTime);

            loadingSlider.value = displayedProgress;
            progressText.text = $"Loading... {(displayedProgress * 100f):F0}%";

            yield return null;
        }

        loadingSlider.value = 1f;

        op.allowSceneActivation = true;

        yield return null;

        yield return FadeOut();
        HideLoadingUI();
    }

    private IEnumerator FadeIn()
    {
        loadingCanvas.blocksRaycasts = true;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            loadingCanvas.alpha = t / fadeDuration;
            yield return null;
        }

        loadingCanvas.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            loadingCanvas.alpha = 1 - (t / fadeDuration);
            yield return null;
        }

        loadingCanvas.alpha = 0;
        loadingCanvas.blocksRaycasts = false;

        displayedProgress = 0f;
        loadingSlider.value = 0f;
        progressText.text = "Loading... 0%";
    }
    public void ShowLoadingUI()
    {
        loadingCanvas.alpha = 1f;
        loadingCanvas.blocksRaycasts = true;
        gameObject.SetActive(true);
    }

    public void HideLoadingUI()
    {
        loadingCanvas.alpha = 0f;
        loadingCanvas.blocksRaycasts = false;
    }

}