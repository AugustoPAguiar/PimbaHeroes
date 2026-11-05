using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private Button jogarBtn;
    [SerializeField] private Button coopBtn;
    [SerializeField] private Button creditsBtn;

    public Button vaca1;
    public Button vaca2;

    void Start()
    {
        jogarBtn.onClick.AddListener(OnJogarClick);
        coopBtn.onClick.AddListener(OnCoopClick);

        creditsBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("Creditos"));

        vaca1.onClick.AddListener(() => SceneManager.LoadSceneAsync("EasterEgg 1"));
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
