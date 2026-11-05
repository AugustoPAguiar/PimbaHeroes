using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public enum Turn
    {
        Player,
        Enemy
    }

    [Header("Prefabs das notas")]
    public GameObject leftArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject rightArrowPrefab;

    [Header("Peixes receptores do jogador")]
    public RectTransform playerLeftTarget;
    public RectTransform playerDownTarget;
    public RectTransform playerUpTarget;
    public RectTransform playerRightTarget;

    [Header("Peixes receptores do inimigo")]
    public RectTransform enemyLeftTarget;
    public RectTransform enemyDownTarget;
    public RectTransform enemyUpTarget;
    public RectTransform enemyRightTarget;

    [Header("Configuração")]
    public float noteSpeed = 500f;
    public float travelTime = 1f;
    public float startY = -600f;
    public Canvas canvas;

    [Header("Animações dos personagens")]
    public Animator playerAnimator;
    public string playerIdleState;
    public string playerAttackState;
    public Animator enemyAnimator;
    public string enemyIdleState;
    public string enemyAttackState;

    private SongChart currentChart;
    private AudioSource audioSource;
    private int currentSectionIndex;
    private Turn currentTurn = Turn.Enemy;
    private List<NoteData> currentNotes = new List<NoteData>();
    private RectTransform notesParent;
    private bool isPlaying;
    private float sectionStartTime;

    public Turn CurrentTurn => currentTurn;
    public bool IsPlayerTurn => currentTurn == Turn.Player;

    private void Awake()
    {
        if (canvas == null)
        {
            Debug.LogError("Não encontrei nenhum Canvas na cena!", this);
            return;
        }

        GameObject container = new GameObject("NotesContainer");
        container.transform.SetParent(canvas.transform, false);

        notesParent = container.AddComponent<RectTransform>();
        notesParent.anchorMin = Vector2.zero;
        notesParent.anchorMax = Vector2.one;
        notesParent.pivot = new Vector2(0.5f, 0.5f);
        notesParent.offsetMin = Vector2.zero;
        notesParent.offsetMax = Vector2.zero;
    }

    public void StartSong(SongChart chart, AudioSource source)
    {
        if (notesParent == null)
        {
            Debug.LogError("O NotesContainer não foi criado. Verifique o Canvas.", this);
            return;
        }

        currentChart = chart;
        audioSource = source;
        currentSectionIndex = 0;
        currentTurn = Turn.Enemy;
        isPlaying = true;

        audioSource.clip = chart.music;
        audioSource.Play();

        LoadCurrentSection();
    }

    public void StopSong()
    {
        isPlaying = false;

        if (audioSource != null)
            audioSource.Stop();
    }

    private void LoadCurrentSection()
    {
        if (currentSectionIndex >= currentChart.sections.Count)
        {
            Debug.Log("Música terminou!");
            isPlaying = false;

            // Avisa o GameManager que acabou tudo
            if (GameManager.Instance != null)
                GameManager.Instance.OnAllNotesFinished();

            return;
        }

        currentNotes = new List<NoteData>(
            currentChart.sections[currentSectionIndex].notes
        );

        currentNotes.Sort((a, b) => a.time.CompareTo(b.time));

        sectionStartTime = audioSource.time;
        UpdateCharacterAnimations();

        string turnName = currentTurn == Turn.Player ? "PLAYER" : "INIMIGO";
        Debug.Log($"Iniciando Take {currentSectionIndex + 1} - Vez do {turnName}");
    }

    private void UpdateCharacterAnimations()
    {
        if (currentTurn == Turn.Player)
        {
            PlayState(playerAnimator, playerIdleState);
            PlayState(enemyAnimator, enemyAttackState);
        }
        else
        {
            PlayState(enemyAnimator, enemyIdleState);
            PlayState(playerAnimator, playerAttackState);
        }
    }

    private static void PlayState(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            return;

        animator.Play(stateName, 0, 0f);
    }

    private void Update()
    {
        if (!isPlaying || audioSource == null)
            return;

        float timeInSection = audioSource.time - sectionStartTime;

        while (currentNotes.Count > 0)
        {
            NoteData data = currentNotes[0];
            float spawnTime = data.time - travelTime;

            if (timeInSection < spawnTime)
                break;

            SpawnNote(data);
            currentNotes.RemoveAt(0);
        }

        if (currentNotes.Count == 0 &&
            timeInSection > GetLastNoteTime() + travelTime + 0.8f)
        {
            NextTurn();
        }
    }

    private float GetLastNoteTime()
    {
        List<NoteData> sectionNotes =
            currentChart.sections[currentSectionIndex].notes;

        if (sectionNotes.Count == 0)
            return 0f;

        float lastTime = 0f;

        foreach (NoteData note in sectionNotes)
        {
            float noteEndTime = note.isHold
                ? note.time + note.holdDuration
                : note.time;

            if (noteEndTime > lastTime)
                lastTime = noteEndTime;
        }

        return lastTime;
    }

    private void NextTurn()
    {
        if (currentTurn == Turn.Enemy)
        {
            // O jogador repete a mesma secção que acabou de jogar o inimigo.
            currentTurn = Turn.Player;
            LoadCurrentSection();
        }
        else
        {
            // Só avança para a próxima secção depois da vez do jogador.
            currentTurn = Turn.Enemy;
            currentSectionIndex++;
            LoadCurrentSection();
        }
    }

    private void SpawnNote(NoteData data)
    {
        bool isEnemySide = currentTurn == Turn.Enemy;

        GameObject prefab = GetPrefab(data.direction);
        KeyCode key = GetKey(data.direction);
        RectTransform hitTarget = GetHitTarget(data.direction, isEnemySide);

        GameObject noteObject = Instantiate(prefab, notesParent, false);

        RectTransform noteRect = noteObject.GetComponent<RectTransform>();

        noteRect.anchorMin = new Vector2(0.5f, 0.5f);
        noteRect.anchorMax = new Vector2(0.5f, 0.5f);
        noteRect.pivot = new Vector2(0.5f, 0.5f);

        Vector3 targetCenter = hitTarget.TransformPoint(hitTarget.rect.center);
        Vector3 targetPositionInNotes = notesParent.InverseTransformPoint(targetCenter);

        // A nota começa abaixo do peixe, na mesma posição horizontal.
        noteRect.anchoredPosition = new Vector2(targetPositionInNotes.x, startY);

        Note note = noteObject.GetComponent<Note>();

        if (note == null)
            note = noteObject.AddComponent<Note>();

        note.Setup(key, noteSpeed, data.isHold, data.holdDuration);
        note.isEnemyNote = isEnemySide;
        note.hitTarget = hitTarget;
    }

    private GameObject GetPrefab(int direction)
    {
        switch (direction)
        {
            case 0: return leftArrowPrefab;
            case 1: return downArrowPrefab;
            case 2: return upArrowPrefab;
            case 3: return rightArrowPrefab;
            default: return null;
        }
    }

    private KeyCode GetKey(int direction)
    {
        switch (direction)
        {
            case 0: return KeyCode.A;
            case 1: return KeyCode.S;
            case 2: return KeyCode.W;
            case 3: return KeyCode.D;
            default: return KeyCode.None;
        }
    }

    private RectTransform GetHitTarget(int direction, bool isEnemySide)
    {
        if (isEnemySide)
        {
            switch (direction)
            {
                case 0: return enemyLeftTarget;
                case 1: return enemyDownTarget;
                case 2: return enemyUpTarget;
                case 3: return enemyRightTarget;
                default: return null;
            }
        }

        switch (direction)
        {
            case 0: return playerLeftTarget;
            case 1: return playerDownTarget;
            case 2: return playerUpTarget;
            case 3: return playerRightTarget;
            default: return null;
        }
    }
}
