using UnityEngine;

public class CoopPlayerInput : MonoBehaviour
{
    [SerializeField] private float hitWindowPixels = 80f;

    private void Update()
    {
        CoopGameManager manager = CoopGameManager.Instance;
        if (manager == null || manager.MatchEnded || manager.coopSpawner == null || !manager.coopSpawner.IsRunning)
            return;

        if (manager.CurrentPlayerNumber == 1)
        {
            CheckInput(KeyCode.LeftArrow, KeyCode.A, 1);
            CheckInput(KeyCode.DownArrow, KeyCode.S, 1);
            CheckInput(KeyCode.UpArrow, KeyCode.W, 1);
            CheckInput(KeyCode.RightArrow, KeyCode.D, 1);
            ReleaseHold(KeyCode.LeftArrow, KeyCode.A, 1);
            ReleaseHold(KeyCode.DownArrow, KeyCode.S, 1);
            ReleaseHold(KeyCode.UpArrow, KeyCode.W, 1);
            ReleaseHold(KeyCode.RightArrow, KeyCode.D, 1);
        }
        else
        {
            CheckInput(KeyCode.A, KeyCode.A, 2);
            CheckInput(KeyCode.S, KeyCode.S, 2);
            CheckInput(KeyCode.W, KeyCode.W, 2);
            CheckInput(KeyCode.D, KeyCode.D, 2);
            ReleaseHold(KeyCode.A, KeyCode.A, 2);
            ReleaseHold(KeyCode.S, KeyCode.S, 2);
            ReleaseHold(KeyCode.W, KeyCode.W, 2);
            ReleaseHold(KeyCode.D, KeyCode.D, 2);
        }
    }

    private void CheckInput(KeyCode controlKey, KeyCode laneKey, int playerNumber)
    {
        if (!Input.GetKeyDown(controlKey)) return;

        CoopNote closestNote = null;
        float closestDistance = float.MaxValue;
        foreach (CoopNote note in FindObjectsByType<CoopNote>(FindObjectsSortMode.None))
        {
            if (note == null || note.playerNumber != playerNumber || note.keyToPress != laneKey)
                continue;

            float distance = note.DistanceToTargetPixels();
            if (distance <= hitWindowPixels && distance < closestDistance)
            {
                closestNote = note;
                closestDistance = distance;
            }
        }

        if (closestNote != null && closestNote.Hit())
            CoopGameManager.Instance.OnPlayerHit(closestDistance, playerNumber);
    }

    private void ReleaseHold(KeyCode controlKey, KeyCode laneKey, int playerNumber)
    {
        if (!Input.GetKeyUp(controlKey)) return;
        foreach (CoopNote note in FindObjectsByType<CoopNote>(FindObjectsSortMode.None))
            if (note != null && note.playerNumber == playerNumber && note.keyToPress == laneKey)
                note.ReleaseHold();
    }
}
