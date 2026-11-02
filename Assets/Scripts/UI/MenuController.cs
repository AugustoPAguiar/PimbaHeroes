using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Game Objects - UI")]
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject menuStory;
    [SerializeField] private GameObject menuCoop;

    [Header("Action Button")]
    [SerializeField] private Button storyBtn;
    [SerializeField] private Button coopBtn;

    [Header("Back Buttons")]
    [SerializeField] private List<Button> backButtons;

    void Start()
    {
        menuPrincipal.SetActive(true);
        menuStory.SetActive(false);
        menuCoop.SetActive(false);

        storyBtn.onClick.AddListener(StoryMode);        
        coopBtn.onClick.AddListener(CoopMode);

        foreach (var button in backButtons)
        {
            button.onClick.AddListener(BackButton);
        }

    }

    private void StoryMode()
    {
        menuPrincipal.SetActive(false);
        menuStory.SetActive(true);
    }

    private void CoopMode()
    {
        menuPrincipal.SetActive(false);
        menuCoop.SetActive(true);
    }

    private void BackButton()
    {

    }
}
