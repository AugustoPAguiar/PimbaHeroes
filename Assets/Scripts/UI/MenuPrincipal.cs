using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private Button jogarBtn;
    [SerializeField] private Button coopBtn;
    [SerializeField] private Button creditsBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button vaca1;
    [SerializeField] private Button vaca2;

    void Start()
    {
        jogarBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("History 1 - Emanuel"));

        coopBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("3 - Menu Coop"));

        creditsBtn.onClick.AddListener(() => SceneManager.LoadSceneAsync("Creditos"));

        vaca1.onClick.AddListener(() => SceneManager.LoadSceneAsync("EasterEgg 1"));

        vaca2.onClick.AddListener(() => SceneManager.LoadSceneAsync("EasterEgg 2"));

        exitBtn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        });
    }
}
