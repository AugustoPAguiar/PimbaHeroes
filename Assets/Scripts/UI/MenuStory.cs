using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuStory : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button easyBtn;
    [SerializeField] private Button mediumBtn;
    [SerializeField] private Button hardBtn;


    void Start()
    {
        backBtn.onClick.AddListener(OnBackClicked);
        easyBtn.onClick.AddListener(OnEasyMode);
        mediumBtn.onClick.AddListener(OnMediumMode);
        hardBtn.onClick.AddListener(OnHardMode);
    }

    void OnBackClicked()
    {
        SceneManager.LoadSceneAsync("1 - Menu Principal");
    }

    void OnEasyMode()
    {
        // TODO

    }

    void OnMediumMode()
    {
        // TODO

    }

    void OnHardMode()
    {
        // TODO

    }
}
