using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoopGameManager : MonoBehaviour
{
    public enum HitQuality { Perfect, Great, Good, Miss }

    public static CoopGameManager Instance;
    public CoopNoteSpawner coopSpawner;
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
    public float perfectTolerance = 8f;
    public float greatTolerance = 28f;
    public float goodTolerance = 80f;
    public int perfectPoints = 10;
    public int greatPoints = 8;
    public int goodPoints = 5;

    [Header("Música desta Scene")]
    public SongChart currentSong;

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

    private Coroutine hideJudgmentRoutineP1;
    private Coroutine hideJudgmentRoutineP2;

    private void Awake()
    {
        Instance = this;

        if (player1judgmentDisplay == null && canvas != null)
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
        }

        if (player2judgmentDisplay == null && canvas != null)
        {
            GameObject displayP2 = new GameObject("player2judgmentDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            displayP2.transform.SetParent(canvas.transform, false);
            RectTransform rectP2 = displayP2.GetComponent<RectTransform>();
            rectP2.anchorMin = rectP2.anchorMax = new Vector2(0.5f, 0.5f);
            rectP2.anchoredPosition = new Vector2(0f, -190f);
            rectP2.sizeDelta = new Vector2(240f, 80f);
            player2judgmentDisplay = displayP2.GetComponent<Image>();
            player2judgmentDisplay.raycastTarget = false;
            player2judgmentDisplay.enabled = false;
        }

        if (player1judgmentDisplay != null) player1judgmentDisplay.gameObject.SetActive(false);
        if (player2judgmentDisplay != null) player2judgmentDisplay.gameObject.SetActive(false);
    }

    public bool MatchEnded => jogoAcabou;
    public int CurrentPlayerNumber => coopSpawner != null ? coopSpawner.CurrentPlayerNumber : 0;

    private void Start() => StartSong(currentSong);

    public void StartSong(SongChart chart)
    {
        scoreP1 = scoreP2 = 0;
        jogoAcabou = false;
        notasTotaisP1 = notasTotaisP2 = 0;
        qtdPerfectP1 = qtdPerfectP2 = qtdGreatP1 = qtdGreatP2 = 0;
        qtdGoodP1 = qtdGoodP2 = qtdMissP1 = qtdMissP2 = 0;
        if (healthBar != null) healthBar.health = 0.5f;
        if (coopSpawner == null)
        {
            Debug.LogError("Coop: não encontrei um CoopNoteSpawner nem um NoteSpawner para converter.", this);
            return;
        }
        coopSpawner.StartSong(chart, musicSource);
    }

    public void AddScore(int points)
    {
        scoreP1 += points;
        Debug.Log("Pontuação: " + scoreP1);
    }

    public void OnHoldFinished(int playerNumber, float duration)
    {
        int points = 8 + Mathf.FloorToInt(duration);
        if (playerNumber == 1) scoreP1 += points;
        else scoreP2 += points;
    }

    public void OnPlayerHitMiss(int playerNumber) => OnPlayerMiss(playerNumber);

    public HitQuality OnPlayerHit(float distance) => OnPlayerHit(distance, 1);

    public HitQuality OnPlayerHit(float distance, int playerNumber)
    {
        bool player1 = playerNumber == 1;
        if (player1) notasTotaisP1++;
        else notasTotaisP2++;

        HitQuality quality = GetQuality(distance);
        ShowJudgment(quality, playerNumber);
        switch (quality)
        {
            case HitQuality.Perfect:
                AwardHit(player1, perfectPoints, player1 ? P1hitAmount : P2hitAmount);
                if (player1) qtdPerfectP1++; else qtdPerfectP2++;
                break;
            case HitQuality.Great:
                AwardHit(player1, greatPoints, (player1 ? P1hitAmount : P2hitAmount) * 0.8f);
                if (player1) qtdGreatP1++; else qtdGreatP2++;
                break;
            case HitQuality.Good:
                AwardHit(player1, goodPoints, (player1 ? P1hitAmount : P2hitAmount) * 0.5f);
                if (player1) qtdGoodP1++; else qtdGoodP2++;
                break;
            default:
                if (player1) { qtdMissP1++; ApplyHealthChange(P1missAmount); }
                else { qtdMissP2++; ApplyHealthChange(P2MissAmount); }
                break;
        }
        return quality;
    }

    private void AwardHit(bool player1, int points, float healthChange)
    {
        if (player1) scoreP1 += points; else scoreP2 += points;
        ApplyHealthChange(healthChange);
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
        OnPlayerMiss(1);
    }

    public void OnPlayerMiss(int playerNumber)
    {
        bool player1 = playerNumber == 1;
        if (player1) { notasTotaisP1++; qtdMissP1++; }
        else { notasTotaisP2++; qtdMissP2++; }
        ShowJudgment(HitQuality.Miss, playerNumber);
        ApplyHealthChange(player1 ? P1missAmount : P2MissAmount);
    }

    private void ApplyHealthChange(float amount)
    {
        if (healthBar == null || jogoAcabou)
            return;

        healthBar.health = Mathf.Clamp01(healthBar.health - amount);
        // P1 uses the right UI and lowers the bar; P2 uses the left UI and raises it.
        if (healthBar.health <= 0.05f) RightPlayerWin();
        else if (healthBar.health >= 0.95f) LeftPlayerWin();
    }

    private void ShowJudgment(HitQuality quality, int playerNumber)
    {
        Image display = playerNumber == 1 ? player1judgmentDisplay : player2judgmentDisplay;
        if (display == null)
            return;

        switch (quality)
        {
            case HitQuality.Perfect: display.sprite = perfectSprite; break;
            case HitQuality.Great: display.sprite = greatSprite; break;
            case HitQuality.Good: display.sprite = goodSprite; break;
            case HitQuality.Miss: display.sprite = missSprite; break;
        }

        display.enabled = display.sprite != null;
        display.gameObject.SetActive(true);
        if (playerNumber == 1)
        {
            if (hideJudgmentRoutineP1 != null) StopCoroutine(hideJudgmentRoutineP1);
            hideJudgmentRoutineP1 = StartCoroutine(HideJudgmentAfterDelay(display, playerNumber));
        }
        else
        {
            if (hideJudgmentRoutineP2 != null) StopCoroutine(hideJudgmentRoutineP2);
            hideJudgmentRoutineP2 = StartCoroutine(HideJudgmentAfterDelay(display, playerNumber));
        }
    }

    private IEnumerator HideJudgmentAfterDelay(Image display, int playerNumber)
    {
        yield return new WaitForSeconds(judgmentDisplayDuration);
        if (display != null) display.enabled = false;
        if (playerNumber == 1) hideJudgmentRoutineP1 = null;
        else hideJudgmentRoutineP2 = null;
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

        if (coopSpawner != null)
            coopSpawner.StopSong();

        StartCoroutine(ShowResultAfterDelay());
    }

    private IEnumerator ShowResultAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (healthBar != null && healthBar.health <= 0.5f)
            RightPlayerWin();
        else
            LeftPlayerWin();
    }

    public void LeftPlayerWin()
    {

        jogoAcabou = true;

        if (coopSpawner != null) coopSpawner.StopSong();

        Debug.Log("Jogador 2 (esquerda, WASD) venceu.");

        Estatisticas();

        if (fimDeJogo != null && fimDeJogo.btnContinuar != null)
        {
            fimDeJogo.btnContinuar.onClick.RemoveAllListeners();
            fimDeJogo.btnContinuar.onClick.AddListener(() => SceneManager.LoadSceneAsync("1 - Menu Principal"));
        }
    }

    public void RightPlayerWin()
    {
        jogoAcabou = true;

        if (coopSpawner != null) coopSpawner.StopSong();

        Debug.Log("Jogador 1 (direita, setas) venceu.");

        Estatisticas();

        if (fimDeJogo != null && fimDeJogo.btnContinuar != null)
        {
            fimDeJogo.btnContinuar.onClick.RemoveAllListeners();
            fimDeJogo.btnContinuar.onClick.AddListener(() => SceneManager.LoadSceneAsync("1 - Menu Principal"));
        }
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
            $"P1 - Score: {scoreP1}        |    P2 - Score: {scoreP2}\n\n" +
            $"P1 - Acertos: {porcentagemAcertoP1:F1}%      |   P2 - Acertos{porcentagemAcertoP2:F1}%";  

        if (fimDeJogo != null && fimDeJogo.estatistica != null)
            foreach (var info in fimDeJogo.estatistica)
                if (info != null) info.text = stats;
    }
}
