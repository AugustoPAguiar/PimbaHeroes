using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private Button jogarBtn;
    [SerializeField] private Button coopBtn;

    void Start()
    {
        jogarBtn.onClick.AddListener(OnJogarClick);
        coopBtn.onClick.AddListener(OnCoopClick);
    }

    void OnJogarClick()
    {
        SceneManager.LoadSceneAsync("History 1 - Emanuel");
    }

    void OnCoopClick()
    {
        SceneManager.LoadSceneAsync("3 - Menu Coop");
    }

}
