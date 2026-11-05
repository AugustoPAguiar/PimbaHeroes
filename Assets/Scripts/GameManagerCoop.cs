using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerCoop : MonoBehaviour
{
    public enum HitQuality { Perfect, Great, Good, Miss }

    public static GameManagerCoop Instance;

    [Header("Referências")]
    public NoteSpawner spawner;
    public HealthBar healthBar;
    public AudioSource musicSource;
    public Canvas canvas;
    public Image player1judgmentDisplay;
    public Image player2judgmentDisplay;

    [Header("Sprites: Perfect, Great, Good, Miss")]
    public Sprite perfectSprite;
    public Sprite greatSprite;
    public Sprite goodSprite;
    public Sprite missSprite;
    public float judgmentDisplayDuration = 0.7f;

    [Header("Qualidade dos acertos (distância em pixels)")]
    public float perfectTolerance = 8f;
    public float greatTolerance = 28f;
    public float goodTolerance = 80f;

    [Header("Pontos por qualidade")]
    public int perfectPoints = 10;
    public int greatPoints = 8;
    public int goodPoints = 5;

    [Header("Música desta Scene")]
    public SongChart currentSong;

    [Header("Barra de Vida")]
    public float P1hitAmount = 0.035f;
    public float P1missAmount = -0.05f;
    public float P2hitAmount = -0.03f;
    public float P2MissAmount = 0.015f;

    [Header("Comportamento do inimigo")]
    [Range(0f, 1f)] public float enemyMissChance = 0.8f;

    public EndGame fimDeJogo;

    private int scoreP1, scoreP2, notasTotaisP1 = 0, notasTotaisP2 = 0;
    private int qtdPerfectP1 = 0, qtdPerfectP2 = 0, qtdGreatP1 = 0, qtdGreatP2 = 0;
    private int qtdGoodP1 = 0, qtdGoodP2 = 0, qtdMissP1 = 0, qtdMissP2 = 0;
    private float porcentagemAcertoP1 = 0, porcentagemAcertoP2 = 0;

    private bool jogoAcabou = false;

    private Coroutine hideJudgmentRoutine;

    private void Awake()
    {
        player1judgmentDisplay.gameObject.SetActive(false);
        player2judgmentDisplay.gameObject.SetActive(false);

        Instance = this;
        if (player1judgmentDisplay == null && canvas != null && player2judgmentDisplay)
        {
            GameObject displayP1 = new GameObject("player1judgmentDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            displayP1.transform.SetParent(canvas.transform, false);
            RectTransform rectP1 = displayP1.GetComponent<RectTransform>();
            rectP1.anchorMin = rectP1.anchorMax = new Vector2(0.5f, 0.5f);
            rectP1.anchoredPosition = new Vector2(0f, -100f);
            rectP1.sizeDelta = new Vector2(240f, 80f);
            player1judgmentDisplay = displayP1.GetComponent<Image>();
            player1judgmentDisplay.raycastTarget = false;
            player1judgmentDisplay.enabled = false;

            GameObject displayP2 = new GameObject("player1judgmentDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            displayP1.transform.SetParent(canvas.transform, false);
            RectTransform rectP2 = displayP1.GetComponent<RectTransform>();
            rectP2.anchorMin = rectP2.anchorMax = new Vector2(0.5f, 0.5f);
            rectP2.anchoredPosition = new Vector2(0f, -100f);
            rectP2.sizeDelta = new Vector2(240f, 80f);
            player1judgmentDisplay = displayP1.GetComponent<Image>();
            player1judgmentDisplay.raycastTarget = false;
            player1judgmentDisplay.enabled = false;
        }
    }

    private void Start() => StartSong(currentSong);

    public void StartSong(SongChart chart)
    {
        scoreP1 = 0;
        if (spawner != null && chart != null)
            spawner.StartSong(chart, musicSource);
    }

    public void AddScore(int points)
    {
        scoreP1 += points;
        Debug.Log("Pontuação: " + scoreP1);
    }

    public HitQuality OnPlayerHit(float distance)
    {
        player1judgmentDisplay.gameObject.SetActive(true);

        notasTotaisP1++;

        HitQuality quality = GetQuality(distance);
        ShowJudgment(quality);
        switch (quality)
        {
            case HitQuality.Perfect:
                AddScore(perfectPoints);
                healthBar.ChangeHealth(P1hitAmount);
                qtdPerfectP1++;
                break;
            case HitQuality.Great:
                AddScore(greatPoints);
                healthBar.ChangeHealth(P1hitAmount * 0.8f);
                qtdGreatP1++;
                break;
            case HitQuality.Good:
                AddScore(goodPoints);
                healthBar.ChangeHealth(P1hitAmount * 0.5f);
                qtdGoodP1++;
                break;
            default:
                qtdMissP1++;
                healthBar.ChangeHealth(P1missAmount);
                break;
        }
        return quality;
    }

    private HitQuality GetQuality(float distance)
    {
        if (distance <= perfectTolerance) return HitQuality.Perfect;
        if (distance <= greatTolerance) return HitQuality.Great;
        if (distance <= goodTolerance) return HitQuality.Good;
        return HitQuality.Miss;
    }

    public void OnPlayerMiss()
    {
        // deixou a nota passar
        notasTotaisP1++;
        qtdMissP1++;

        ShowJudgment(HitQuality.Miss);
        healthBar.ChangeHealth(P1missAmount);
    }

    private void ShowJudgment(HitQuality quality)
    {
        if (player1judgmentDisplay == null)
            return;

        switch (quality)
        {
            case HitQuality.Perfect: player1judgmentDisplay.sprite = perfectSprite; break;
            case HitQuality.Great: player1judgmentDisplay.sprite = greatSprite; break;
            case HitQuality.Good: player1judgmentDisplay.sprite = goodSprite; break;
            case HitQuality.Miss: player1judgmentDisplay.sprite = missSprite; break;
        }

        player1judgmentDisplay.enabled = player1judgmentDisplay.sprite != null;
        if (hideJudgmentRoutine != null)
            StopCoroutine(hideJudgmentRoutine);
        hideJudgmentRoutine = StartCoroutine(HideJudgmentAfterDelay());
    }

    private IEnumerator HideJudgmentAfterDelay()
    {
        yield return new WaitForSeconds(judgmentDisplayDuration);
        if (player1judgmentDisplay != null)
            player1judgmentDisplay.enabled = false;
        hideJudgmentRoutine = null;
    }

    public void OnAllNotesFinished()
    {
        Debug.Log("OnAlNotesFinis");

        if (jogoAcabou) return;
        jogoAcabou = true;
        Debug.Log("Jogo = true");

        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();

        Debug.Log("Musica Parou");

        if (spawner != null)
            spawner.StopSong();

        StartCoroutine(ShowResultAfterDelay());
    }

    private IEnumerator ShowResultAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (healthBar.health <= 0.5f)
            LeftPlayerWin();
        else
            RightPlayerWin();
    }

    public void LeftPlayerWin()
    {

        jogoAcabou = true;

        spawner.gameObject.SetActive(false);

        fimDeJogo.SetUiText();

        fimDeJogo.textoFinal.text = "<color=green>Jogador 1 Venceu!</color>";

        Estatisticas();

        fimDeJogo.btnContinuar.onClick.AddListener(() => { SceneManager.LoadSceneAsync("1 - Menu Principal"); });

        spawner.StopSong();
    }

    public void RightPlayerWin()
    {
        jogoAcabou = true;

        spawner.gameObject.SetActive(false);

        fimDeJogo.SetUiText();

        fimDeJogo.textoFinal.text = "<color=red>Jogador 2 Venceu!</color>";

        Estatisticas();

        fimDeJogo.btnContinuar.onClick.AddListener(() => { SceneManager.LoadSceneAsync("1 - Menu Principal"); });

        spawner.StopSong();
    }

    private void Estatisticas()
    {
        int acertosP1 = qtdPerfectP1 + qtdGreatP1 + qtdGoodP1;

        int acertosP2 = qtdPerfectP2 + qtdGreatP2 + qtdGoodP2;

        porcentagemAcertoP1 = notasTotaisP1 > 0 ? (acertosP1 / (float)notasTotaisP1) * 100f : 0f;
        porcentagemAcertoP2 = notasTotaisP2 > 0 ? (acertosP2 / (float)notasTotaisP2) * 100f : 0f;

        string stats =
            $"P1 - Perfect: {qtdPerfectP1}   |    Player 2 - Perfect: {qtdPerfectP2}\n" +
            $"P1 - Great: {qtdGreatP1}       |    P2 - Great: {qtdGreatP2}\n" +
            $"P1 - Good: {qtdGoodP1}         |    P2 - Good: {qtdGoodP2}\n" +
            $"P1 - Miss: {qtdMissP1}         |    P2 - Miss: {qtdMissP2}\n" +
            $"P1 - Score: {scoreP1}          |    P2 - Score: {scoreP2}\n\n" +
            $"P1 - Acertos: {porcentagemAcertoP1:F1}%      |   P2 - Acertos{porcentagemAcertoP2:F1}%";

        fimDeJogo.textoEstatística.text = stats;
    }
}
