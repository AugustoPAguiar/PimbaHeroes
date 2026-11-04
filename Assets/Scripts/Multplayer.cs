using UnityEngine;

/// <summary>
/// Modo competitivo local: Jogador esquerdo usa as setas e jogador direito usa WASD.
/// Configure notas distintas para cada lado: setas para o lado esquerdo e WASD para o direito.
/// </summary>
public class Multplayer : MonoBehaviour
{
    public enum HitQuality { Perfect, Great, Good, Miss }

    [System.Serializable]
    public class PlayerState
    {
        [Tooltip("Teclas deste jogador, na ordem das pistas.")]
        public KeyCode[] keys;
        [Min(1)] public float startingLife = 1f;
        [HideInInspector] public float life;
        [HideInInspector] public int score;
    }

    [Header("Jogadores")]
    public PlayerState leftPlayer = new PlayerState
    {
        keys = new[] { KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow }
    };
    public PlayerState rightPlayer = new PlayerState
    {
        keys = new[] { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D }
    };

    [Header("Julgamento (distância em pixels)")]
    public float hitWindowPixels = 80f;
    public float perfectTolerance = 8f;
    public float greatTolerance = 28f;
    public float goodTolerance = 80f;

    [Header("Pontos")]
    public int perfectPoints = 10;
    public int greatPoints = 8;
    public int goodPoints = 5;

    [Header("Vida")]
    public float hitLifeGain = 0.035f;
    public float missLifeLoss = 0.05f;

    private bool gameFinished;

    private void Awake()
    {
        leftPlayer.life = leftPlayer.startingLife;
        rightPlayer.life = rightPlayer.startingLife;
    }

    private void Update()
    {
        if (gameFinished) return;
        CheckPlayerInput(leftPlayer);
        CheckPlayerInput(rightPlayer);
    }

    private void CheckPlayerInput(PlayerState player)
    {
        if (player.keys == null) return;

        foreach (KeyCode key in player.keys)
        {
            if (!Input.GetKeyDown(key)) continue;

            Note closest = null;
            float closestDistance = float.MaxValue;
            Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);

            foreach (Note note in notes)
            {
                // Notas inimigas não fazem parte do modo competitivo.
                if (note == null || note.isEnemyNote || note.wasHit || note.keyToPress != key)
                    continue;

                float distance = note.GetDistanceToHitTargetPixels();
                if (distance <= hitWindowPixels && distance < closestDistance)
                {
                    closest = note;
                    closestDistance = distance;
                }
            }

            if (closest == null) continue;

            closest.Hit();
            AwardHit(player, closestDistance);
        }
    }

    private void AwardHit(PlayerState player, float distance)
    {
        if (distance <= perfectTolerance) player.score += perfectPoints;
        else if (distance <= greatTolerance) player.score += greatPoints;
        else if (distance <= goodTolerance) player.score += goodPoints;

        player.life = Mathf.Clamp01(player.life + hitLifeGain);
    }

    /// <summary>Chame quando uma nota daquela pista passar sem ser acertada.</summary>
    public void OnPlayerMiss(bool leftSide)
    {
        if (gameFinished) return;

        PlayerState player = leftSide ? leftPlayer : rightPlayer;
        player.life = Mathf.Clamp01(player.life - missLifeLoss);
        if (player.life <= 0f)
            FinishGame();
    }

    /// <summary>Chame quando todas as notas dos dois lados terminarem.</summary>
    public void OnAllNotesFinished()
    {
        if (gameFinished) return;
        gameFinished = true;

        if (leftPlayer.score > rightPlayer.score)
            Debug.Log($"Jogador esquerdo venceu! {leftPlayer.score} x {rightPlayer.score}");
        else if (rightPlayer.score > leftPlayer.score)
            Debug.Log($"Jogador direito venceu! {rightPlayer.score} x {leftPlayer.score}");
        else
            Debug.Log($"Empate! {leftPlayer.score} x {rightPlayer.score}");
    }

    private void FinishGame()
    {
        gameFinished = true;

        if (leftPlayer.life <= 0f && rightPlayer.life <= 0f)
            Debug.Log($"Ambos perderam a vida. Resultado por pontos: {leftPlayer.score} x {rightPlayer.score}");
        else if (leftPlayer.life <= 0f)
            Debug.Log($"Jogador direito venceu! {leftPlayer.score} x {rightPlayer.score}");
        else
            Debug.Log($"Jogador esquerdo venceu! {leftPlayer.score} x {rightPlayer.score}");
    }
}
