using System.Collections.Generic;
using UnityEngine;

/// <summary>Alternates each chart section between P2 (former enemy side) and P1.</summary>
public class CoopNoteSpawner : MonoBehaviour
{
    public enum Turn { Player1, Player2 }

    [Header("Note prefabs")]
    public GameObject leftArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject rightArrowPrefab;

    [Header("Player 1 targets (right side)")]
    public RectTransform player1LeftTarget;
    public RectTransform player1DownTarget;
    public RectTransform player1UpTarget;
    public RectTransform player1RightTarget;

    [Header("Player 2 targets (left side)")]
    public RectTransform player2LeftTarget;
    public RectTransform player2DownTarget;
    public RectTransform player2UpTarget;
    public RectTransform player2RightTarget;

    [Header("Settings")]
    public float noteSpeed = 500f;
    public float travelTime = 1f;
    public float startY = -600f;
    public float missY = 600f;
    public Canvas canvas;

    [Header("Character animations (optional)")]
    public Animator player1Animator;
    public string player1IdleState;
    public string player1AttackState;
    public Animator player2Animator;
    public string player2IdleState;
    public string player2AttackState;

    private SongChart chart;
    private AudioSource audioSource;
    private readonly List<NoteData> notesForTurn = new List<NoteData>();
    private readonly List<CoopNote> activeNotes = new List<CoopNote>();
    private RectTransform notesRoot;
    private int sectionIndex;
    private int nextNoteIndex;
    private float sectionStartTime;
    private float lastNoteEnd;
    private bool running;
    private Turn currentTurn = Turn.Player2;

    public Turn CurrentTurn => currentTurn;
    public int CurrentPlayerNumber => currentTurn == Turn.Player1 ? 1 : 2;
    public bool IsRunning => running;

    public void CopySettingsFrom(NoteSpawner source)
    {
        if (source == null) return;
        leftArrowPrefab = source.leftArrowPrefab;
        downArrowPrefab = source.downArrowPrefab;
        upArrowPrefab = source.upArrowPrefab;
        rightArrowPrefab = source.rightArrowPrefab;
        player1LeftTarget = source.enemyLeftTarget;
        player1DownTarget = source.enemyDownTarget;
        player1UpTarget = source.enemyUpTarget;
        player1RightTarget = source.enemyRightTarget;
        player2LeftTarget = source.playerLeftTarget;
        player2DownTarget = source.playerDownTarget;
        player2UpTarget = source.playerUpTarget;
        player2RightTarget = source.playerRightTarget;
        noteSpeed = source.noteSpeed;
        travelTime = source.travelTime;
        startY = source.startY;
        canvas = source.canvas;
        player1Animator = source.enemyAnimator;
        player1IdleState = source.enemyIdleState;
        player1AttackState = source.enemyAttackState;
        player2Animator = source.playerAnimator;
        player2IdleState = source.playerIdleState;
        player2AttackState = source.playerAttackState;
    }

    public void StartSong(SongChart song, AudioSource source)
    {
        if (song == null || source == null || song.music == null)
        {
            Debug.LogError("Coop: associa uma SongChart com música e um AudioSource.", this);
            return;
        }

        if (!EnsureNotesRoot()) return;
        StopSong();
        chart = song;
        audioSource = source;
        sectionIndex = 0;
        currentTurn = Turn.Player2; // P2 begins on the former enemy side.
        running = true;
        audioSource.clip = chart.music;
        audioSource.Play();
        LoadTurn();
    }

    public void StopSong()
    {
        running = false;
        foreach (CoopNote note in activeNotes)
            if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();
        notesForTurn.Clear();
        if (audioSource != null) audioSource.Stop();
    }

    private bool EnsureNotesRoot()
    {
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Coop: não encontrei um Canvas para as notas.", this);
            return false;
        }

        if (notesRoot != null) return true;
        GameObject root = new GameObject("CoopNotesContainer", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        notesRoot = root.GetComponent<RectTransform>();
        notesRoot.anchorMin = Vector2.zero;
        notesRoot.anchorMax = Vector2.one;
        notesRoot.offsetMin = Vector2.zero;
        notesRoot.offsetMax = Vector2.zero;
        return true;
    }

    private void LoadTurn()
    {
        if (chart == null || chart.sections == null || sectionIndex >= chart.sections.Count)
        {
            running = false;
            if (CoopGameManager.Instance != null)
                CoopGameManager.Instance.OnAllNotesFinished();
            return;
        }

        Section section = chart.sections[sectionIndex];
        notesForTurn.Clear();
        if (section != null && section.notes != null)
            notesForTurn.AddRange(section.notes);
        notesForTurn.Sort((a, b) => a.time.CompareTo(b.time));
        nextNoteIndex = 0;
        sectionStartTime = audioSource.time;
        lastNoteEnd = 0f;
        foreach (NoteData note in notesForTurn)
            lastNoteEnd = Mathf.Max(lastNoteEnd, note.time + (note.isHold ? note.holdDuration : 0f));
        SetTurnAnimations();
        Debug.Log($"Secção {sectionIndex + 1}: turno do Jogador {CurrentPlayerNumber}", this);
    }

    private void Update()
    {
        if (!running || audioSource == null) return;

        float time = audioSource.time - sectionStartTime;
        while (nextNoteIndex < notesForTurn.Count && time >= notesForTurn[nextNoteIndex].time - travelTime)
            SpawnNote(notesForTurn[nextNoteIndex++]);

        activeNotes.RemoveAll(note => note == null);
        if (nextNoteIndex >= notesForTurn.Count && time > lastNoteEnd + travelTime && activeNotes.Count == 0)
            NextTurn();
    }

    private void NextTurn()
    {
        if (currentTurn == Turn.Player2)
            currentTurn = Turn.Player1;
        else
        {
            currentTurn = Turn.Player2;
            sectionIndex++;
        }
        LoadTurn();
    }

    private void SpawnNote(NoteData data)
    {
        int player = CurrentPlayerNumber;
        GameObject prefab = GetPrefab(data.direction);
        RectTransform target = GetTarget(data.direction, player);
        if (prefab == null || target == null)
        {
            Debug.LogError($"Coop: falta prefab ou target da direção {data.direction} para o Jogador {player}.", this);
            return;
        }

        GameObject instance = Instantiate(prefab, notesRoot, false);
        Note legacyNote = instance.GetComponent<Note>();
        if (legacyNote != null) legacyNote.enabled = false;
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        Vector3 worldTarget = target.TransformPoint(target.rect.center);
        Vector3 localTarget = notesRoot.InverseTransformPoint(worldTarget);
        rect.anchoredPosition = new Vector2(localTarget.x, startY);

        CoopNote note = instance.GetComponent<CoopNote>();
        if (note == null) note = instance.AddComponent<CoopNote>();
        note.Setup(player, GetKey(data.direction), noteSpeed, target, data.isHold, data.holdDuration, missY);
        activeNotes.Add(note);
    }

    private void SetTurnAnimations()
    {
        if (currentTurn == Turn.Player1)
        {
            PlayState(player1Animator, player1AttackState);
            PlayState(player2Animator, player2IdleState);
        }
        else
        {
            PlayState(player2Animator, player2AttackState);
            PlayState(player1Animator, player1IdleState);
        }
    }

    private static void PlayState(Animator animator, string state)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(state))
            animator.Play(state, 0, 0f);
    }

    private GameObject GetPrefab(int direction) => direction switch
    {
        0 => leftArrowPrefab, 1 => downArrowPrefab, 2 => upArrowPrefab, 3 => rightArrowPrefab, _ => null
    };

    private RectTransform GetTarget(int direction, int player) => (player, direction) switch
    {
        (1, 0) => player1LeftTarget, (1, 1) => player1DownTarget, (1, 2) => player1UpTarget, (1, 3) => player1RightTarget,
        (2, 0) => player2LeftTarget, (2, 1) => player2DownTarget, (2, 2) => player2UpTarget, (2, 3) => player2RightTarget,
        _ => null
    };

    private static KeyCode GetKey(int direction) => direction switch
    {
        0 => KeyCode.A, 1 => KeyCode.S, 2 => KeyCode.W, 3 => KeyCode.D, _ => KeyCode.None
    };
}
