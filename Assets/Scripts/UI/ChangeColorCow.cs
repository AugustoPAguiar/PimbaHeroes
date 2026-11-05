using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeColorCow : MonoBehaviour
{
    [Header("Referências")]
    public Image imagemdeFundo;
    public Button imagemVaca;          // Arraste aqui a Image da vaca (não o Sprite!)

    [Header("Configurações da Vaca")]
    public float velocidadeMovimento = 150f;
    public float velocidadeRotacao = 80f;
    public float tempoTrocaDirecao = 1.5f;   // tempo médio para mudar de direção

    // Variáveis internas
    private RectTransform rectVaca;
    private Vector2 direcao;
    private float tempoRestanteDirecao;
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

        // Começa com escala no meio do intervalo
        rectVaca.localScale = Vector3.one;

        // Direção inicial aleatória
        MudarDirecao();
    }

    void Update()
    {
        // === 1. Troca de cor do fundo a cada 0.5 segundos ===
        tempoCor += Time.deltaTime;
        if (tempoCor >= 0.5f)
        {
            tempoCor = 0f;
            imagemdeFundo.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f
            );
        }

        // === 2. Oscilação de tamanho (0.75 ↔ 1.5) ===
        float escala = Mathf.Lerp(0.75f, 1.5f, (Mathf.Sin(Time.time * 2f) + 1f) / 2f);
        rectVaca.localScale = new Vector3(escala, escala, 1f);

        // === 3. Rotação contínua ===
        rectVaca.Rotate(0f, 0f, velocidadeRotacao * Time.deltaTime);

        // === 4. Movimento aleatório pela tela ===
        tempoRestanteDirecao -= Time.deltaTime;
        if (tempoRestanteDirecao <= 0f)
        {
            MudarDirecao();
        }

        // Move a vaca
        rectVaca.anchoredPosition += direcao * velocidadeMovimento * Time.deltaTime;

        // Mantém a vaca dentro da tela (limites aproximados)
        ManterDentroDaTela();
    }

    void MudarDirecao()
    {
        // Nova direção aleatória
        direcao = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        // Tempo até a próxima mudança de direção
        tempoRestanteDirecao = Random.Range(tempoTrocaDirecao * 0.6f, tempoTrocaDirecao * 1.4f);
    }

    void ManterDentroDaTela()
    {
        Vector2 pos = rectVaca.anchoredPosition;
        Vector2 tamanho = rectVaca.rect.size * rectVaca.localScale.x;

        // Limites (ajuste se o Canvas for diferente)
        float limiteX = (Screen.width / 2f) - (tamanho.x / 2f);
        float limiteY = (Screen.height / 2f) - (tamanho.y / 2f);

        // Se bater na borda, inverte a direção nesse eixo
        if (pos.x > limiteX || pos.x < -limiteX)
        {
            direcao.x *= -1;
            pos.x = Mathf.Clamp(pos.x, -limiteX, limiteX);
        }

        if (pos.y > limiteY || pos.y < -limiteY)
        {
            direcao.y *= -1;
            pos.y = Mathf.Clamp(pos.y, -limiteY, limiteY);
        }

        rectVaca.anchoredPosition = pos;
    }
}