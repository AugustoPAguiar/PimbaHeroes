using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Tooltip("Distância máxima para aceitar a tecla, em pixels.")]
    public float hitWindowPixels = 80f;

    private void Update()
    {
        CheckInput(KeyCode.A);
        CheckInput(KeyCode.S);
        CheckInput(KeyCode.W);
        CheckInput(KeyCode.D);
    }

    private void CheckInput(KeyCode key)
    {
        if (!Input.GetKeyDown(key))
            return;

        Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);

        Note closestNote = null;
        float closestDistance = float.MaxValue;

        foreach (Note note in notes)
        {
            if (note.isEnemyNote || note.wasHit || note.keyToPress != key)
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
        GameManager.Instance.OnPlayerHit(closestDistance);
    }
}
