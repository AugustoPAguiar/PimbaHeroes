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
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 tamanhoCanvas = canvasRect.rect.size;

        Vector2 pos = rectVaca.anchoredPosition;

        // Metade do tamanho da vaca (levando em conta a escala)
        Vector2 metadeTamanho = rectVaca.rect.size * 0.5f * rectVaca.localScale.x;

        // Limites considerando que o pivô está no centro
        float limiteX = (tamanhoCanvas.x * 0.5f) - metadeTamanho.x;
        float limiteY = (tamanhoCanvas.y * 0.5f) - metadeTamanho.y;

        bool bateu = false;

        // Borda direita / esquerda
        if (pos.x > limiteX)
        {
            pos.x = limiteX;
            direcao.x *= -1;
            bateu = true;
        }
        else if (pos.x < -limiteX)
        {
            pos.x = -limiteX;
            direcao.x *= -1;
            bateu = true;
        }

        // Borda cima / baixo
        if (pos.y > limiteY)
        {
            pos.y = limiteY;
            direcao.y *= -1;
            bateu = true;
        }
        else if (pos.y < -limiteY)
        {
            pos.y = -limiteY;
            direcao.y *= -1;
            bateu = true;
        }

        if (bateu)
        {
            // Pequena variação aleatória na direção
            direcao += new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
            direcao = direcao.normalized;
        }

        rectVaca.anchoredPosition = pos;
    }
}