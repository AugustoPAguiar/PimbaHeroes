using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    public float perfectTolerance = 8f;
    public float greatTolerance = 28f;
    public float goodTolerance = 80f;
    public int perfectPoints = 10;
    public int greatPoints = 8;
    public int goodPoints = 5;

    [Header("Música desta Scene")]
    public SongChart[] currentSong;
    public GameObject[] cenario;

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
    private int numeroJogo;

    private bool jogoAcabou = false;
    private readonly Dictionary<GameObject, float> hitFeedbackExpirations = new Dictionary<GameObject, float>();

    private void Update()
    {
        if (hitFeedbackExpirations.Count == 0) return;

        List<GameObject> expired = null;
        foreach (KeyValuePair<GameObject, float> item in hitFeedbackExpirations)
        {
            if (Time.time < item.Value) continue;
            if (item.Key != null) item.Key.SetActive(false);
            if (expired == null) expired = new List<GameObject>();
            expired.Add(item.Key);
        }
        if (expired != null)
            foreach (GameObject visual in expired) hitFeedbackExpirations.Remove(visual);
    }

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

    private void Start(){
        // Cenário e usica aleatorio
        cenario[Random.Range(0, currentSong.Length)].SetActive(true);
        StartSong(currentSong[Random.Range(0, currentSong.Length)]);
    }

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

    public HitQuality OnPlayerHit(float distance) => OnPlayerHit(distance, 1, KeyCode.None);

    public HitQuality OnPlayerHit(float distance, int playerNumber, KeyCode laneKey = KeyCode.None)
    {
        ShowHitFeedback(playerNumber, laneKey);
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

    private void ShowHitFeedback(int playerNumber, KeyCode laneKey)
    {
        if (coopSpawner == null || laneKey == KeyCode.None) return;
        RectTransform target = coopSpawner.GetHitTarget(playerNumber, laneKey);
        if (target == null) return;

        string direction = laneKey == KeyCode.A || laneKey == KeyCode.LeftArrow ? "Left" :
            laneKey == KeyCode.S || laneKey == KeyCode.DownArrow ? "Down" :
            laneKey == KeyCode.W || laneKey == KeyCode.UpArrow ? "Up" :
            laneKey == KeyCode.D || laneKey == KeyCode.RightArrow ? "Right" : null;
        if (direction == null) return;

        string visualName = direction + " Arrow Acerto";
        Transform root = target;
        while (root.parent != null)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name != visualName) continue;
                GameObject visual = child.gameObject;
                visual.SetActive(true);
                hitFeedbackExpirations[visual] = Time.time + 0.7f;
                return;
            }
            root = root.parent;
        }
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
        // Pega o display original (template)
        Image template = playerNumber == 1 ? player1judgmentDisplay : player2judgmentDisplay;
        if (template == null) return;

        // Instancia uma cópia
        Image display = Instantiate(template, template.transform.parent);
        display.gameObject.SetActive(true);

        // Posição com offset aleatório
        Vector3 randomOffset = new Vector3(Random.Range(-20f, 20f), Random.Range(-15f, 15f), 0f);
        display.transform.localPosition = template.transform.localPosition + randomOffset;

        // Configura sprite, escala e duração
        float scale = 1f;
        float duration = 0.6f; // tempo padrão mais curto

        switch (quality)
        {
            case HitQuality.Perfect:
                display.sprite = perfectSprite;
                scale = 1.75f;
                duration = 0.85f;   // Perfect some mais rápido agora
                break;
            case HitQuality.Great:
                display.sprite = greatSprite;
                scale = 1.25f;
                duration = 0.7f;
                break;
            case HitQuality.Good:
                display.sprite = goodSprite;
                scale = 1f;
                duration = 0.55f;
                break;
            case HitQuality.Miss:
                display.sprite = missSprite;
                scale = 0.75f;
                duration = 0.45f;
                break;
        }

        display.transform.localScale = new Vector3(scale, scale, 1f);
        display.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-15f, 15f));
        display.color = Color.white;
        display.enabled = display.sprite != null;

        // Inicia a coroutine que destrói o objeto depois do tempo
        StartCoroutine(HideAndDestroyJudgment(display, duration));
    }

    private IEnumerator HideAndDestroyJudgment(Image display, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (display != null)
        {
            // Fade out opcional (mais bonito)
            float fadeTime = 0.15f;
            float timer = 0f;
            Color startColor = display.color;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;
                display.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                yield return null;
            }

            Destroy(display.gameObject);
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

        fimDeJogo.uiPrincipal.gameObject.SetActive(false);
        fimDeJogo.uiVitoria.gameObject.SetActive(true);

        fimDeJogo.estatistica[0].text = "Player 1";

        Estatisticas();

        if (fimDeJogo != null && fimDeJogo.btnContinuar != null)
        {
            fimDeJogo.btnContinuar.onClick.RemoveAllListeners();
            fimDeJogo.btnContinuar.onClick.AddListener(() => SceneManager.LoadSceneAsync("1 - Menu Principal"));
        }
    }

    public void RightPlayerWin()
    {


        if (coopSpawner != null) coopSpawner.StopSong();

        fimDeJogo.uiPrincipal.gameObject.SetActive(false);
        fimDeJogo.uiVitoria.gameObject.SetActive(true);

        fimDeJogo.estatistica[0].text = "Player 2";

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

        string statsP1 =
            $"Player 2\n\n" +
            $"Perfect:{qtdPerfectP1}\n\n" +
            $"Great:{qtdGreatP1}\n\n" +
            $"Good:{qtdGoodP1}\n\n" +
            $"Miss:{qtdMissP1}\n\n" +
            $"Score:{scoreP1}\n\n\n" +
            $"Acertos:{porcentagemAcertoP1:F1}%";

        string statsP2 =
            $"Player 1\n\n" +
            $"Perfect:{qtdPerfectP2}\n\n" +
            $"Great:{qtdGreatP2}\n\n" +
            $"Good:{qtdGoodP2}\n\n" +
            $"Miss:{qtdMissP2}\n\n" +
            $"Score:{scoreP2}\n\n\n" +
            $"Acertos:{porcentagemAcertoP2:F1}%";

        fimDeJogo.estatistica[1].text = statsP2;
        fimDeJogo.estatistica[2].text = statsP1;
    }
}
