using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeColorCow : MonoBehaviour
{
    [Header("Referências")]
    public Image imagemdeFundo;
    public Button imagemVaca;          // Arraste aqui a Image da vaca (não o Sprite!)
    public Canvas canvas;

    [Header("Configurações da Vaca")]
    public float velocidadeMovimento = 150f;
    public float velocidadeRotacao = 80f;

    // Variáveis internas
    private RectTransform rectVaca;
    private Vector2 direcao;
    private float tempoCor = 0f;

    void Start()
    {
        if (imagemVaca == null)
        {
            Debug.LogError("Arraste a Image da vaca no Inspector!");
            return;
        }

        imagemVaca.onClick.AddListener(() => SceneManager.LoadSceneAsync("1 - Menu Principal"));

        rectVaca = imagemVaca.image.rectTransform;

        // Direção inicial aleatória
        MudarDirecao();
    }

    void Update()
    {
        // === 1. Troca de cor do fundo a cada 0.5 segundos ===
        tempoCor += Time.deltaTime;
        if (tempoCor >= 0.2f)
        {
            tempoCor = 0f;
            imagemdeFundo.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f
            );
        }

        // === 2. Rotação contínua ===
        rectVaca.Rotate(0f, 0f, velocidadeRotacao * Time.deltaTime);

        // === 3. Movimento ===
        rectVaca.anchoredPosition += direcao * velocidadeMovimento * Time.deltaTime;

        // Mantém a vaca dentro da tela e inverte a direção ao bater
        ManterDentroDaTela();
    }

    void MudarDirecao()
    {
        // Nova direção aleatória
        direcao = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        // Evita direção quase zero
        if (direcao.magnitude < 0.1f)
            direcao = Vector2.right;
    }

    void ManterDentroDaTela()
    {
        Vector2 pos = rectVaca.anchoredPosition;
        Vector2 tamanho = rectVaca.rect.size * rectVaca.localScale.x;

        // Limites (ajuste se o Canvas for diferente)
        float limiteX = (Screen.width) - (tamanho.x);
        float limiteY = (Screen.height) - (tamanho.y);

        bool bateu = false;

        // Se bater na borda, inverte a direção nesse eixo
        if (pos.x > limiteX || pos.x < -limiteX)
        {
            direcao.x *= -1;
            pos.x = Mathf.Clamp(pos.x, -limiteX, limiteX);
            bateu = true;
        }

        if (pos.y > limiteY || pos.y < -limiteY)
        {
            direcao.y *= -1;
            pos.y = Mathf.Clamp(pos.y, -limiteY, limiteY);
            bateu = true;
        }

        // Garante que a direção continue normalizada
        if (bateu)
            direcao = direcao.normalized;

        rectVaca.anchoredPosition = pos;
    }
}