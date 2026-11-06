using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
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

    [Header("Feedback visual dos acertos - esquerda, baixo, cima, direita")]
    public GameObject[] playerHitDirections = new GameObject[4]; // esquerda, baixo, cima, direita
    public GameObject[] enemyHitDirections = new GameObject[4]; // esquerda, baixo, cima, direita

    [Header("Qualidade dos acertos (distância em pixels)")]
    public float perfectTolerance = 5f;
    public float greatTolerance = 10f;
    public float goodTolerance = 30f;

    [Header("Pontos por qualidade")]
    public int perfectPoints = 10;
    public int greatPoints = 8;
    public int goodPoints = 5;

    [Header("Música desta Scene")]
    public SongChart[] currentSong;
    public GameObject[] cenarioCoop;
    public GameObject[] enemyCoop;
    public RawImage[] enemyHeadCoop;

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
    private bool jogoAcabou = false;

    private readonly Dictionary<GameObject, float> hitFeedbackExpirations = new Dictionary<GameObject, float>();

    private void Awake()
    {
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

        SetHitFeedbackActive(playerHitDirections, false);
        SetHitFeedbackActive(enemyHitDirections, false);
    }

    private void Update()
    {
        if (hitFeedbackExpirations.Count == 0)
            return;

        List<GameObject> expired = null;
        foreach (KeyValuePair<GameObject, float> item in hitFeedbackExpirations)
        {
            if (Time.time < item.Value)
                continue;

            if (item.Key != null)
                item.Key.SetActive(false);
            if (expired == null)
                expired = new List<GameObject>();
            expired.Add(item.Key);
        }

        if (expired != null)
            foreach (GameObject visual in expired)
                hitFeedbackExpirations.Remove(visual);
    }

    private void Start()
    {
        // Se existir mais de uma musica no Song, ele pega uma aleatória, senão ele vai na unica q tem (Só é util no coop)
        if(currentSong.Length > 1)
        {
            int inimigo = Random.Range(0, currentSong.Length);
            enemyHeadCoop[inimigo].gameObject.SetActive(true);
            cenarioCoop[inimigo].SetActive(true);
            enemyCoop[Random.Range(0, currentSong.Length)].SetActive(true);
            StartSong(currentSong[Random.Range(0, currentSong.Length)]);
        }
        else
            StartSong(currentSong[0]);
    }

    public void StartSong(SongChart chart)
    {
        judgmentDisplay.gameObject.SetActive(false);

        score = 0;
        
        if (spawner != null && chart != null)
            spawner.StartSong(chart, musicSource);
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Pontuação: " + score);
    }

    public HitQuality OnPlayerHit(float distance, KeyCode laneKey = KeyCode.None)
    {
        ShowHitFeedback(false, laneKey);
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

        Image newJudgment = Instantiate(judgmentDisplay, judgmentDisplay.transform.parent);
        newJudgment.color = Color.white;

        Vector3 randomOffset = new Vector3(Random.Range(-20f, 20f), Random.Range(-15f, 15f), 0f);
        newJudgment.transform.localPosition = judgmentDisplay.transform.localPosition + randomOffset;

        float scale = 1f;
        float duration = 0.5f;

        switch (quality)
        {
            case HitQuality.Perfect:
                newJudgment.sprite = perfectSprite;
                scale = 1.75f;
                duration = 1.3f;
                break;
            case HitQuality.Great:
                newJudgment.sprite = greatSprite;
                scale = 1.25f;
                duration = 1.0f;
                break;
            case HitQuality.Good:
                newJudgment.sprite = goodSprite;
                scale = 1f;
                duration = 0.7f;
                break;
            case HitQuality.Miss:
                newJudgment.sprite = missSprite;
                scale = 0.75f;
                duration = 0.5f;
                break;
        }

        newJudgment.transform.localScale = new Vector3(scale, scale, 1f);

        float randomZ = Random.Range(-15f, 15f);
        newJudgment.transform.localEulerAngles = new Vector3(0f, 0f, randomZ);

        newJudgment.enabled = true;

        StartCoroutine(HideAndDestroyJudgment(newJudgment, duration));
    }

    private IEnumerator HideAndDestroyJudgment(Image judgment, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (judgment != null)
        {
            // Fade out opcional (mais bonito)
            float fadeTime = 0.15f;
            float timer = 0f;
            Color startColor = judgment.color;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;
                judgment.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            Destroy(judgment.gameObject);
        }
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
            PlayerWin();
        else
            PlayerLose();
    }

    public bool RollEnemyMiss() => enemyMissChance >= 1f || (enemyMissChance > 0f && Random.value < enemyMissChance);
    public void OnEnemyHit(KeyCode laneKey = KeyCode.None)
    {
        ShowHitFeedback(true, laneKey);
        healthBar.ChangeHealth(enemyHitAmount);
    }
    public void OnEnemyMiss() => healthBar.ChangeHealth(enemyMissAmount);

    private void ShowHitFeedback(bool enemy, KeyCode laneKey)
    {
        GameObject[] directions = enemy ? enemyHitDirections : playerHitDirections;
        int directionIndex = -1;
        if (laneKey == KeyCode.A || laneKey == KeyCode.LeftArrow) directionIndex = 0;
        else if (laneKey == KeyCode.S || laneKey == KeyCode.DownArrow) directionIndex = 1;
        else if (laneKey == KeyCode.W || laneKey == KeyCode.UpArrow) directionIndex = 2;
        else if (laneKey == KeyCode.D || laneKey == KeyCode.RightArrow) directionIndex = 3;
        if (directions != null && directionIndex >= 0 && directionIndex < directions.Length && directions[directionIndex] != null)
            ShowHitFeedbackObject(directions[directionIndex]);
    }

    private void ShowHitFeedbackObject(GameObject visual)
    {
        visual.SetActive(true);
        hitFeedbackExpirations[visual] = Time.time + 0.7f;
    }

    private static void SetHitFeedbackActive(GameObject[] directions, bool active)
    {
        if (directions == null)
            return;

        foreach (GameObject direction in directions)
            direction.SetActive(active);
    }

    public void PlayerWin()
    {

        jogoAcabou = true;

        spawner.gameObject.SetActive(false);

        fimDeJogo.uiPrincipal.gameObject.SetActive(false);
        fimDeJogo.uiVitoria.gameObject.SetActive(true);

        Estatisticas();

        fimDeJogo.btnContinuar.onClick.AddListener(() => { SceneManager.LoadSceneAsync(fimDeJogo.proxFase); });

        spawner.StopSong();
    }

    public void PlayerLose()
    {
        jogoAcabou = true;

        spawner.gameObject.SetActive(false);

        fimDeJogo.uiPrincipal.gameObject.SetActive(false);
        fimDeJogo.uiDerrota.gameObject.SetActive(true);

        Estatisticas();

        fimDeJogo.btnVoltar.onClick.AddListener(() => { SceneManager.LoadSceneAsync("1 - Menu Principal"); });

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

        foreach (var info in fimDeJogo.estatistica)
        {
            info.text = stats;
        }
        
    }
}
