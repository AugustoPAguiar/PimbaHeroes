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
        backBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("1 - Menu Principal"));
        versusBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("Multiplyer - Versus"));
        // coopBtn.onClick.AddListener();
    }
}
