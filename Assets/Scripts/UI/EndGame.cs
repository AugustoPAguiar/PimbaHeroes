using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{

    public EndGame() { }

    [Header("Fim de Jogo")]
    public Canvas uiPrincipal;
    public Canvas uiFimDeJogo;
    public TMP_Text textoFinal;
    public TMP_Text textoEstatística;
    public Button btnContinuar;
    public string proxFase;

    public void SetUiText()
    {
        uiPrincipal.gameObject.SetActive(false);
        uiFimDeJogo.gameObject.SetActive(true);
    }
}
