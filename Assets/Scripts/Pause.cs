using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("Canvases")]
    public Canvas uiDoJogo;
    public Canvas uiPause;
    public Button voltarAoMenuBtn;

    [Header("Menu")]
    public string menuSceneName = "1 - Menu Principal";

    [Header("Volume da música")]
    public AudioSource audioSource;
    public Scrollbar volumeScrollbar;
    [Range(0f, 1f)] public float maxVolume = 0.25f;

    private bool isPaused;

    private void Start()
    {
        voltarAoMenuBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync(menuSceneName));
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (uiDoJogo != null) uiPause.gameObject.SetActive(true);
        if (uiPause != null) uiPause.gameObject.SetActive(false);

        maxVolume = Mathf.Clamp01(maxVolume);
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp(audioSource.volume, 0f, maxVolume);
            if (volumeScrollbar != null)
            {
                volumeScrollbar.value = maxVolume > 0f ? audioSource.volume / maxVolume : 0f;
                volumeScrollbar.onValueChanged.AddListener(SetVolume);
            }
        }
    }

    private void OnDestroy()
    {
        if (volumeScrollbar != null)
            volumeScrollbar.onValueChanged.RemoveListener(SetVolume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (uiDoJogo != null) uiDoJogo.gameObject.SetActive(false);
        if (uiPause != null) uiPause.gameObject.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (uiDoJogo != null) uiDoJogo.gameObject.SetActive(true);
        if (uiPause != null) uiPause.gameObject.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        if (audioSource == null) return;
        audioSource.volume = Mathf.Clamp01(volume) * maxVolume;
    }

    public void AdjustVolume(float amount)
    {
        float currentVolume = audioSource != null && maxVolume > 0f
            ? audioSource.volume / maxVolume
            : 0f;
        SetVolume(currentVolume + amount);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
