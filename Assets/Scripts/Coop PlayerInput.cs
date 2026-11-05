using UnityEngine;

public class CoopPlayerInput : MonoBehaviour
{
    [SerializeField] private float hitWindowPixels = 80f;

    private void Update()
    {
        // jogar no AW
        CheckInput(KeyCode.W);
        CheckInput(KeyCode.D);

        // jogar nas Down e Right seta :)
        CheckInput(KeyCode.DownArrow);
        CheckInput(KeyCode.LeftArrow);
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
            if (note.isEnemyNote || note.wasHit)
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
}
