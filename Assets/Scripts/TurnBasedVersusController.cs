using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Local, turn-based Versus: the chart's sections alternate between arrow keys
/// and WASD. Add this component to the Versus scene's GameManager object.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class TurnBasedVersusController : MonoBehaviour
{
    [Header("Gameplay source (read from the existing scene setup)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private NoteSpawner noteSpawner;
    [SerializeField] private float hitWindowPixels = 80f;
    [SerializeField] private float perfectTolerance = 8f;
    [SerializeField] private float greatTolerance = 28f;
    [SerializeField] private float travelTime = 1f;
    [SerializeField] private float startY = -600f;

    [Header("Competitive rules")]
    [SerializeField] private float startingLife = 1f;
    [SerializeField] private float hitLifeGain = 0.035f;
    [SerializeField] private float missLifeLoss = 0.05f;
    [SerializeField] private int perfectPoints = 10;
    [SerializeField] private int greatPoints = 8;
    [SerializeField] private int goodPoints = 5;

    private readonly KeyCode[][] keys =
    {
        new[] { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow },
        new[] { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D }
    };
    private readonly int[] scores = new int[2];
    private readonly int[] perfects = new int[2];
    private readonly int[] greats = new int[2];
    private readonly int[] goods = new int[2];
    private readonly int[] misses = new int[2];
    private readonly float[] lives = new float[2];
    private readonly List<VersusNote> activeNotes = new List<VersusNote>();

    private RectTransform notesRoot;
    private AudioSource music;
    private SongChart chart;
    private int sectionIndex;
    private int activePlayer;
    private float sectionStartTime;
    private float lastNoteEnd;
    private int nextNoteIndex;
    private bool running;
    private bool finished;

    private void Awake()
    {
        if (gameManager == null) gameManager = GetComponent<GameManager>();
        if (noteSpawner == null && gameManager != null) noteSpawner = gameManager.spawner;

        if (gameManager == null || noteSpawner == null)
        {
            Debug.LogError("Versus: associa o GameManager e o NoteSpawner existentes.", this);
            enabled = false;
            return;
        }

        chart = gameManager.currentSong;
        music = gameManager.musicSource;
        // Keep the scene objects and their serialized references, while stopping
        // the old single-player manager and AI turn spawner from running.
        gameManager.enabled = false;
        GameManager.Instance = null;
        noteSpawner.enabled = false;

        PlayerInput oldInput = FindFirstObjectByType<PlayerInput>();
        if (oldInput != null) oldInput.enabled = false;

        Canvas canvas = noteSpawner.canvas;
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null || chart == null || music == null)
        {
            Debug.LogError("Versus: falta Canvas, música ou SongChart na cena.", this);
            enabled = false;
            return;
        }

        GameObject root = new GameObject("VersusNotesContainer", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        notesRoot = root.GetComponent<RectTransform>();
        notesRoot.anchorMin = Vector2.zero;
        notesRoot.anchorMax = Vector2.one;
        notesRoot.offsetMin = Vector2.zero;
        notesRoot.offsetMax = Vector2.zero;

        lives[0] = lives[1] = startingLife;
    }

    private void Start()
    {
        if (!enabled) return;
        if (chart.sections == null || chart.sections.Count == 0)
        {
            Debug.LogError("Versus: a música não tem secções no SongChart.", this);
            enabled = false;
            return;
        }

        music.clip = chart.music;
        music.Play();
        running = true;
        LoadSection();
    }

    private void Update()
    {
        if (!running || finished) return;

        SpawnDueNotes();
        ProcessPlayerInput();
        activeNotes.RemoveAll(note => note == null);

        float sectionTime = music.time - sectionStartTime;
        if (nextNoteIndex >= chart.sections[sectionIndex].notes.Count &&
            sectionTime > lastNoteEnd + travelTime + 0.8f && activeNotes.Count == 0)
        {
            sectionIndex++;
            if (sectionIndex >= chart.sections.Count) FinishMatch();
            else LoadSection();
        }
    }

    private void LoadSection()
    {
        activePlayer = sectionIndex % 2; // P1 arrows, P2 WASD, alternating by section.
        sectionStartTime = music.time;
        nextNoteIndex = 0;
        List<NoteData> notes = chart.sections[sectionIndex].notes;
        notes.Sort((a, b) => a.time.CompareTo(b.time));
        lastNoteEnd = 0f;
        foreach (NoteData note in notes)
            lastNoteEnd = Mathf.Max(lastNoteEnd, note.time + (note.isHold ? note.holdDuration : 0f));
    }

    private void SpawnDueNotes()
    {
        List<NoteData> notes = chart.sections[sectionIndex].notes;
        float time = music.time - sectionStartTime;
        while (nextNoteIndex < notes.Count && time >= notes[nextNoteIndex].time - travelTime)
        {
            NoteData data = notes[nextNoteIndex++];
            GameObject prefab = GetPrefab(data.direction);
            RectTransform target = GetTarget(data.direction, activePlayer == 1);
            if (prefab == null || target == null) continue;

            GameObject noteObject = Instantiate(prefab, notesRoot, false);
            RectTransform rect = noteObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            Vector3 targetCenter = target.TransformPoint(target.rect.center);
            Vector3 localTarget = notesRoot.InverseTransformPoint(targetCenter);
            rect.anchoredPosition = new Vector2(localTarget.x, startY);

            VersusNote note = noteObject.AddComponent<VersusNote>();
            note.Setup(this, activePlayer, keys[activePlayer][data.direction], target,
                noteSpawner.noteSpeed, hitWindowPixels, data.isHold, data.holdDuration);
            activeNotes.Add(note);
        }
    }

    private void ProcessPlayerInput()
    {
        for (int lane = 0; lane < 4; lane++)
        {
            KeyCode key = keys[activePlayer][lane];
            if (Input.GetKeyDown(key)) TryHit(key);
            if (Input.GetKeyUp(key))
                foreach (VersusNote note in activeNotes)
                    if (note != null && note.Player == activePlayer && note.Key == key)
                        note.Release();
        }
    }

    private void TryHit(KeyCode key)
    {
        VersusNote closest = null;
        float distance = float.MaxValue;
        foreach (VersusNote note in activeNotes)
        {
            if (note == null || note.Player != activePlayer || note.Key != key || note.WasHit) continue;
            float candidate = note.DistanceToTarget();
            if (candidate <= hitWindowPixels && candidate < distance)
            {
                closest = note;
                distance = candidate;
            }
        }
        if (closest == null) return;

        closest.Hit();
        if (distance <= perfectTolerance) { scores[activePlayer] += perfectPoints; perfects[activePlayer]++; }
        else if (distance <= greatTolerance) { scores[activePlayer] += greatPoints; greats[activePlayer]++; }
        else { scores[activePlayer] += goodPoints; goods[activePlayer]++; }
        lives[activePlayer] = Mathf.Clamp01(lives[activePlayer] + hitLifeGain);
    }

    public void OnNoteMiss(VersusNote note)
    {
        int player = note.Player;
        misses[player]++;
        lives[player] = Mathf.Clamp01(lives[player] - missLifeLoss);
        activeNotes.Remove(note);
        if (lives[player] <= 0f) FinishMatch();
    }

    private void FinishMatch()
    {
        if (finished) return;
        finished = true;
        running = false;
        if (music != null) music.Stop();
        foreach (VersusNote note in activeNotes) if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();

        int winner = lives[0] > lives[1] ? 0 : lives[1] > lives[0] ? 1 :
            scores[0] > scores[1] ? 0 : scores[1] > scores[0] ? 1 : -1;
        EndGame end = gameManager.fimDeJogo;
        if (end == null) { Debug.Log($"Versus terminou. Pontos: {scores[0]} x {scores[1]}"); return; }

        end.uiPrincipal.gameObject.SetActive(false);
        end.btnContinuar.GetComponentInChildren<TMP_Text>().text = "Menu";
        end.btnContinuar.onClick.RemoveAllListeners();
        end.btnContinuar.onClick.AddListener(() => SceneManager.LoadScene("1 - Menu Principal"));
    }

    private string Stats(int p) => $"Jogador {p + 1} — Pontos: {scores[p]} | Vida: {lives[p] * 100f:F0}%\nPerfect: {perfects[p]}  Great: {greats[p]}  Good: {goods[p]}  Miss: {misses[p]}";

    private GameObject GetPrefab(int d) => d switch { 0 => noteSpawner.leftArrowPrefab, 1 => noteSpawner.downArrowPrefab, 2 => noteSpawner.upArrowPrefab, 3 => noteSpawner.rightArrowPrefab, _ => null };
    private RectTransform GetTarget(int d, bool p2) => (p2, d) switch
    {
        (false, 0) => noteSpawner.playerLeftTarget, (false, 1) => noteSpawner.playerDownTarget,
        (false, 2) => noteSpawner.playerUpTarget, (false, 3) => noteSpawner.playerRightTarget,
        (true, 0) => noteSpawner.enemyLeftTarget, (true, 1) => noteSpawner.enemyDownTarget,
        (true, 2) => noteSpawner.enemyUpTarget, (true, 3) => noteSpawner.enemyRightTarget, _ => null
    };
}
