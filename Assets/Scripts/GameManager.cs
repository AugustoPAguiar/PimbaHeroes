using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referências")]
    public NoteSpawner spawner;
    public HealthBar healthBar;
    public AudioSource musicSource;

    [Header("Música desta Scene")]
    public SongChart currentSong;

    [Header("Barra de Vida")]
    public float hitAmount = 0.035f;
    public float missAmount = -0.05f;
    public float enemyHitAmount = -0.03f;

    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartSong(currentSong);
    }

    public void StartSong(SongChart chart)
    {
        score = 0;
        spawner.StartSong(chart, musicSource);
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Pontuação: " + score);
    }

    public void OnPlayerHit(float distance)
    {
        float quality = 1f - (distance / 90f);
        quality = Mathf.Clamp01(quality);
        healthBar.ChangeHealth(hitAmount * quality);

    }

    public void OnPlayerMiss()
    {
        healthBar.ChangeHealth(missAmount);
    }

    public void OnEnemyHit()
    {
        healthBar.ChangeHealth(enemyHitAmount);
    }

    public void OnEnemyMiss()
    {
        healthBar.ChangeHealth(0.015f);
    }

    public void PlayerWin()
    {
        Debug.Log("<color=green>VOCÊ GANHOU!</color>");
        spawner.StopSong();
    }

    public void PlayerLose()
    {
        Debug.Log("<color=red>VOCÊ PERDEU!</color>");
        spawner.StopSong();
    }
}