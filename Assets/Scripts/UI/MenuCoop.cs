using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuCoop : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button versusBtn;
    [SerializeField] private Button coopBtn;


    void Start()
    {
        backBtn.onClick.AddListener(OnBackClicked);
        versusBtn.onClick.AddListener(OnVersusMode);
        coopBtn.onClick.AddListener(OnCoopMode);
    }

    void OnBackClicked()
    {
        SceneManager.LoadSceneAsync("1 - Menu Principal");
    }

    void OnVersusMode()
    {
        //TODO
    }

    void OnCoopMode()
    {
        //TODO
    }
}
