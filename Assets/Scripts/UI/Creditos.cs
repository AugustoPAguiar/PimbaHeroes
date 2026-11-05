using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Creditos : MonoBehaviour
{
    public RectTransform conteudo;
    public Button sairBtn;

    [Header("Configuração do Scroll")]
    public float velocidade = -80f;
    public float posicaoFinal = -1400f;

    private bool podeScrollar = true;

    void Start()
    {
        sairBtn.onClick.AddListener(VoltarAoMenu);

        if (conteudo == null)
        {
            Debug.LogError("Conteudo não foi atribuído no Inspector!");
        }
        else
        {
            Debug.Log("Posição inicial: " + conteudo.anchoredPosition);
        }
    }

    void Update()
    {
        if (!podeScrollar || conteudo == null) return;

        conteudo.anchoredPosition += Vector2.down * velocidade * Time.deltaTime;

        // Mostra no Console a posição atual
        Debug.Log("Y atual: " + conteudo.anchoredPosition.y);

        if (conteudo.anchoredPosition.y <= -posicaoFinal)
        {
            podeScrollar = false;
            Debug.Log("Chegou no final!");
        }
    }

    void VoltarAoMenu()
    {
        SceneManager.LoadSceneAsync("1 - Menu Principal");
    }
}