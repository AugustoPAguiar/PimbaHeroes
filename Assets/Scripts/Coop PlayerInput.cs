using UnityEngine;

public class CoopPlayerInput : MonoBehaviour
{
    [Tooltip("Distância máxima para aceitar a tecla, em pixels.")]
    public float hitWindowPixels = 80f;

    private void Update()
    {
        // jogar no awsd
        CheckInput(KeyCode.A);
        CheckInput(KeyCode.S);
        CheckInput(KeyCode.W);
        CheckInput(KeyCode.D);

        // jogar nas setinhas
        CheckInput(KeyCode.LeftArrow);
        CheckInput(KeyCode.DownArrow);
        CheckInput(KeyCode.UpArrow);
        CheckInput(KeyCode.RightArrow);
    }

    private void CheckInput(KeyCode key)
    {
        if (!Input.GetKeyDown(key))
            return;

        KeyCode laneKey = GetLaneKey(key);

        Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);

        Note closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (Note note in notes)
        {
            if (note.isEnemyNote || note.wasHit || note.keyToPress != laneKey)
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
        GameManager.Instance.OnPlayerHit(closestDistance, closestNote.keyToPress);
    }

    private KeyCode GetLaneKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return KeyCode.A;
            case KeyCode.DownArrow: return KeyCode.S;
            case KeyCode.UpArrow: return KeyCode.W;
            case KeyCode.RightArrow: return KeyCode.D;
            default: return key;
        }
    }
}
