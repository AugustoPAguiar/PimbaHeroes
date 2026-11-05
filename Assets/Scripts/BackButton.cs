using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public Button voltarBtn;

    void Start()
    {
        voltarBtn.onClick.AddListener(VoltarAoMenu);
    }

    void VoltarAoMenu()
    {
        SceneManager.LoadSceneAsync("1 - Menu Principal");
    }
}
