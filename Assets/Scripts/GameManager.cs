using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum HitQuality { Perfect, Great, Good, Miss }

    public static GameManager Instance;

    [Header("Referências")]
    public NoteSpawner spawner;
    public HealthBar healthBar;
    public AudioSource musicSource;
    public Canvas canvas;
    public Image judgmentDisplay;

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
    public float hitAmount = 0.035f;
    public float missAmount = -0.05f;
    public float enemyHitAmount = -0.03f;
    public float enemyMissAmount = 0.015f;

    [Header("Comportamento do inimigo")]
    [Range(0f, 1f)] public float enemyMissChance = 0.8f;

    public EndGame fimDeJogo;

    private int score;
    private int notasTotais = 0;
    private int qtdPerfect = 0;
    private int qtdGreat = 0;
    private int qtdGood = 0;
    private int qtdMiss = 0;
    private float porcentagemAcerto = 0;

    private Coroutine hideJudgmentRoutine;

    private void Awake()
    {
        judgmentDisplay.gameObject.SetActive(false);

        Instance = this;
        if (judgmentDisplay == null && canvas != null)
        {
            GameObject display = new GameObject("JudgmentDisplay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            display.transform.SetParent(canvas.transform, false);
            RectTransform rect = display.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -100f);
            rect.sizeDelta = new Vector2(240f, 80f);
            judgmentDisplay = display.GetComponent<Image>();
            judgmentDisplay.raycastTarget = false;
            judgmentDisplay.enabled = false;
        }
    }

    private void Start() => StartSong(currentSong);

    public void StartSong(SongChart chart)
    {
        score = 0;
        if (spawner != null && chart != null)
            spawner.StartSong(chart, musicSource);
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Pontuação: " + score);
    }

    public HitQuality OnPlayerHit(float distance)
    {
        judgmentDisplay.gameObject.SetActive(true);

        notasTotais++;

        HitQuality quality = GetQuality(distance);
        ShowJudgment(quality);
        switch (quality)
        {
            case HitQuality.Perfect:
                AddScore(perfectPoints);
                healthBar.ChangeHealth(hitAmount);
                qtdPerfect++;
                break;
            case HitQuality.Great:
                AddScore(greatPoints);
                healthBar.ChangeHealth(hitAmount * 0.8f);
                qtdGreat++;
                break;
            case HitQuality.Good:
                AddScore(goodPoints);
                healthBar.ChangeHealth(hitAmount * 0.5f);
                qtdGood++;
                break;
            default:
                qtdMiss++;
                healthBar.ChangeHealth(missAmount);
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
        notasTotais++;
        qtdMiss++;

        ShowJudgment(HitQuality.Miss);
        healthBar.ChangeHealth(missAmount);
    }

    private void ShowJudgment(HitQuality quality)
    {
        if (judgmentDisplay == null)
            return;

        switch (quality)
        {
            case HitQuality.Perfect: judgmentDisplay.sprite = perfectSprite; break;
            case HitQuality.Great: judgmentDisplay.sprite = greatSprite; break;
            case HitQuality.Good: judgmentDisplay.sprite = goodSprite; break;
            case HitQuality.Miss: judgmentDisplay.sprite = missSprite; break;
        }

        judgmentDisplay.enabled = judgmentDisplay.sprite != null;
        if (hideJudgmentRoutine != null)
            StopCoroutine(hideJudgmentRoutine);
        hideJudgmentRoutine = StartCoroutine(HideJudgmentAfterDelay());
    }

    private IEnumerator HideJudgmentAfterDelay()
    {
        yield return new WaitForSeconds(judgmentDisplayDuration);
        if (judgmentDisplay != null)
            judgmentDisplay.enabled = false;
        hideJudgmentRoutine = null;
    }

    public bool RollEnemyMiss() => enemyMissChance >= 1f || (enemyMissChance > 0f && Random.value < enemyMissChance);
    public void OnEnemyHit() => healthBar.ChangeHealth(enemyHitAmount);
    public void OnEnemyMiss() => healthBar.ChangeHealth(enemyMissAmount);

    public void PlayerWin()
    {
        spawner.gameObject.SetActive(false);

        fimDeJogo.SetUiText();

        fimDeJogo.textoFinal.text = "<color=green>VITÓRIA</color>";

        Estatisticas();

        fimDeJogo.btnContinuar.GetComponentInChildren<TMP_Text>().text = "Continuar";

        fimDeJogo.btnContinuar.onClick.AddListener(() => { SceneManager.LoadSceneAsync(fimDeJogo.proxFase); });

        spawner.StopSong();
    }

    public void PlayerLose()
    {
        spawner.gameObject.SetActive(false);

        fimDeJogo.SetUiText();

        fimDeJogo.textoFinal.text = "<color=red>DERROTA</color>";

        Estatisticas();

        fimDeJogo.btnContinuar.GetComponentInChildren<TMP_Text>().text = "Voltar";

        fimDeJogo.btnContinuar.onClick.AddListener(() => { SceneManager.LoadSceneAsync("1 - Menu Principal"); });

        spawner.StopSong();
    }

    private void Estatisticas()
    {
        int acertos = qtdPerfect + qtdGreat + qtdGood;

        porcentagemAcerto = notasTotais > 0
            ? (acertos / (float)notasTotais) * 100f
            : 0f;

        string stats =
            $"Perfect: {qtdPerfect}\n" +
            $"Great: {qtdGreat}\n" +
            $"Good: {qtdGood}\n" +
            $"Miss: {qtdMiss}\n" +
            $"Score: {score}\n\n" +
            $"Acertos: {porcentagemAcerto:F1}%";

        fimDeJogo.textoEstatística.text = stats;
    }
}
