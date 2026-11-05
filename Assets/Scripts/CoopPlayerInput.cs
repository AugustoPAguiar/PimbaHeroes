using UnityEngine;

/// <summary>
/// Input local para o modo cooperativo. O Jogador 1 usa A/W e o Jogador 2
/// usa Down/Left. As notas mantêm as teclas WASD internas do NoteSpawner;
/// este componente associa cada pista à tecla física correspondente.
/// </summary>
public class CoopPlayerInput : MonoBehaviour
{
    [Tooltip("Distância máxima para aceitar a tecla, em pixels.")]
    public float hitWindowPixels = 80f;

    [Header("Teclas por pista: esquerda, baixo, cima, direita")]
    public KeyCode[] coopKeys =
    {
        KeyCode.A,
        KeyCode.DownArrow,
        KeyCode.W,
        KeyCode.LeftArrow
    };

    private readonly KeyCode[] noteLaneKeys =
    {
        KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D
    };

    private void Awake()
    {
        // A cena Coop já contém o controlador individual. Desativá-lo evita
        // que A/W também sejam tratados pelo input antigo em paralelo.
        PlayerInput oldInput = FindFirstObjectByType<PlayerInput>();
        if (oldInput != null)
            oldInput.enabled = false;
    }

    private void Update()
    {
        if (coopKeys == null || coopKeys.Length < 4)
            return;

        for (int lane = 0; lane < 4; lane++)
        {
            KeyCode control = coopKeys[lane];
            if (Input.GetKeyDown(control))
                HitClosestNote(noteLaneKeys[lane]);

            if (Input.GetKeyUp(control))
                ReleaseHold(noteLaneKeys[lane]);
        }
    }

    private void HitClosestNote(KeyCode laneKey)
    {
        Note closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (Note note in FindObjectsByType<Note>(FindObjectsSortMode.None))
        {
            if (note == null || note.isEnemyNote || note.wasHit || note.keyToPress != laneKey)
                continue;

            float distance = note.GetDistanceToHitTargetPixels();
            if (distance <= hitWindowPixels && distance < closestDistance)
            {
                closestNote = note;
                closestDistance = distance;
            }
        }

        if (closestNote == null)
            return;

        closestNote.Hit();
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerHit(closestDistance, closestNote.keyToPress);
    }

    private void ReleaseHold(KeyCode laneKey)
    {
        foreach (Note note in FindObjectsByType<Note>(FindObjectsSortMode.None))
        {
            if (note != null && note.keyToPress == laneKey && note.isHold && note.wasHit)
                note.ReleaseHold();
        }
    }
}
