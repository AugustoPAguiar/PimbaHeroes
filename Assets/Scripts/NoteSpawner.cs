using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public enum Turn { Player, Enemy }

    [Header("Prefabs das Setas")]
    public GameObject leftArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject rightArrowPrefab;

    [Header("Configuração")]
    public float noteSpeed = 500f;
    public float travelTime = 1f;

    [Header("Posições X - Lado do Player")]
    public float playerLeftX = 300f;
    public float playerDownX = 570f;
    public float playerUpX = 450f;
    public float playerRightX = 720f;

    [Header("Posições X - Lado do Inimigo")]
    public float enemyLeftX = -720f;
    public float enemyDownX = -450f;
    public float enemyUpX = -570f;
    public float enemyRightX = -300f;

    public float startY = -600f;
    public Canvas canvas;

    // Estado atual
    private SongChart currentChart;
    private AudioSource audioSource;
    private int currentSectionIndex = 0;
    private Turn currentTurn = Turn.Player;
    private List<NoteData> currentNotes = new List<NoteData>();
    private Transform notesParent;
    private bool isPlaying = false;
    private float sectionStartTime = 0f;

    public Turn CurrentTurn => currentTurn;
    public bool IsPlayerTurn => currentTurn == Turn.Player;

    void Awake()
    {
        if (canvas == null)
        {
            Debug.LogError("Não encontrei nenhum Canvas na cena!");
            return;
        }

        // Cria o container DENTRO do Canvas
        GameObject container = new GameObject("NotesContainer");
        container.transform.SetParent(canvas.transform); // false = mantém posição local

        // Adiciona RectTransform
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        notesParent = container.transform;
    }

    public void StartSong(SongChart chart, AudioSource source)
    {
        currentChart = chart;
        audioSource = source;
        currentSectionIndex = 0;
        currentTurn = Turn.Player;
        isPlaying = true;

        audioSource.clip = chart.music;
        audioSource.Play();

        LoadCurrentSection();
    }

    public void StopSong()
    {
        isPlaying = false;
        if (audioSource != null) audioSource.Stop();
    }

    void LoadCurrentSection()
    {
        if (currentSectionIndex >= currentChart.sections.Count)
        {
            // Acabaram os takes → música terminou
            Debug.Log("Música terminou!");
            isPlaying = false;
            return;
        }

        currentNotes = new List<NoteData>(currentChart.sections[currentSectionIndex].notes);
        currentNotes.Sort((a, b) => a.time.CompareTo(b.time));

        sectionStartTime = audioSource.time;
        Debug.Log($"Iniciando Take {currentSectionIndex + 1} - Vez do {(currentTurn == Turn.Player ? "PLAYER" : "INIMIGO")}");
    }

    void Update()
    {
        if (!isPlaying || audioSource == null) return;

        float timeInSection = audioSource.time - sectionStartTime;

        // Spawna as notas do take atual
        while (currentNotes.Count > 0)
        {
            NoteData data = currentNotes[0];
            float spawnTime = data.time - travelTime;

            if (timeInSection >= spawnTime)
            {
                SpawnNote(data);
                currentNotes.RemoveAt(0);
            }
            else break;
        }

        // Quando acabarem as notas do take + um tempinho → passa a vez
        if (currentNotes.Count == 0)
        {
            // Espera a última nota sumir + margem
            if (timeInSection > GetLastNoteTime() + travelTime + 0.8f)
            {
                NextTurn();
            }
        }
    }

    float GetLastNoteTime()
    {
        if (currentChart.sections[currentSectionIndex].notes.Count == 0) return 0f;

        float max = 0f;
        foreach (var n in currentChart.sections[currentSectionIndex].notes)
        {
            float end = n.isHold ? n.time + n.holdDuration : n.time;
            if (end > max) max = end;
        }
        return max;
    }

    void NextTurn()
    {
        if (currentTurn == Turn.Player)
        {
            // Passa para o inimigo jogar o MESMO take
            currentTurn = Turn.Enemy;
            LoadCurrentSection(); // recarrega as mesmas notas
        }
        else
        {
            // Inimigo terminou → vai para o próximo take do player
            currentTurn = Turn.Player;
            currentSectionIndex++;
            LoadCurrentSection();
        }
    }

    void SpawnNote(NoteData data)
    {
        GameObject prefab = null;
        KeyCode key = KeyCode.None;
        float xPos = 0f;

        bool isEnemySide = currentTurn == Turn.Enemy;

        switch (data.direction)
        {
            case 0:
                prefab = leftArrowPrefab;
                key = KeyCode.A;
                xPos = isEnemySide ? enemyLeftX : playerLeftX;
                break;
            case 1:
                prefab = downArrowPrefab;
                key = KeyCode.S;
                xPos = isEnemySide ? enemyDownX : playerDownX;
                break;
            case 2:
                prefab = upArrowPrefab;
                key = KeyCode.W;
                xPos = isEnemySide ? enemyUpX : playerUpX;
                break;
            case 3:
                prefab = rightArrowPrefab;
                key = KeyCode.D;
                xPos = isEnemySide ? enemyRightX : playerRightX;
                break;
        }

        if (prefab == null) return;

        // Instancia como filho do container (importante para UI)
        GameObject noteObj = Instantiate(prefab, notesParent, false); // false = worldPositionStays false

        RectTransform rt = noteObj.GetComponent<RectTransform>();

        // Reseta escala e âncoras para evitar problemas
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = new Vector2(xPos, startY);

        Note note = noteObj.GetComponent<Note>();
        if (note == null) note = noteObj.AddComponent<Note>();

        note.Setup(key, noteSpeed, data.isHold, data.holdDuration);
        note.isEnemyNote = isEnemySide;
    }
}