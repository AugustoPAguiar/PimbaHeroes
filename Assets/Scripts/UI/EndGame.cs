using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{

    public EndGame() { }

    [Header("Fim de Jogo")]
    public Canvas uiPrincipal;
    public GameObject uiVitoria;
    public GameObject uiDerrota;
    public Button btnContinuar;
    public Button btnVoltar;
    public string proxFase;
    public TMP_Text[] estatistica;
}
