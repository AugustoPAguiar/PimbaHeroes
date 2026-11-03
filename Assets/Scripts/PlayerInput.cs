using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float hitWindow = 80f; // Distância Y em que a nota pode ser apertada

    void Update()
    {
        CheckInput(KeyCode.A, "Left");
        CheckInput(KeyCode.S, "Down");
        CheckInput(KeyCode.W, "Up");
        CheckInput(KeyCode.D, "Right");
    }

    void CheckInput(KeyCode key, string direction)
    {
        if (Input.GetKeyDown(key))
        {
            // Procura a nota mais próxima que pode ser apertada
            // Note[] notes = FindObjectsOfType<Note>();
            Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);
            Note closest = null;
            float closestDist = float.MaxValue;

            foreach (Note note in notes)
            {
                if (note.keyToPress == key && !note.wasHit)
                {
                    float dist = Mathf.Abs(note.GetComponent<RectTransform>().anchoredPosition.y - 250f); // 250 = zona de acerto
                    if (dist < hitWindow && dist < closestDist)
                    {
                        closestDist = dist;
                        closest = note;
                    }
                }
            }

            if (closest != null)
            {
                closest.Hit();
                GameManager.Instance.OnPlayerHit(closestDist); // quanto menor a dist, melhor o acerto
            }
            else
            {
                // Apertou no vazio → penalidade leve (opcional)
                // GameManager.Instance.OnPlayerMiss();
            }
        }
    }
}