using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 9f;
    public KeyCode keyToPress;
    public bool wasHit = false;
    public bool isHold = false;
    public float holdDuration = 0f;
    public bool isEnemyNote = false;

    private RectTransform rect;
    private bool isBeingHeld = false;
    private float holdTimer = 0f;
    private float pointsFromHold = 0f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Setup(KeyCode key, float noteSpeed, bool hold, float duration)
    {
        keyToPress = key;
        speed = noteSpeed;
        isHold = hold;
        holdDuration = duration;
    }

    void Update()
    {
        if (wasHit && !isHold) return;

        // Move a nota para cima
        if (!isBeingHeld)
        {
            rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;
        }

        // Miss se passou do ponto
        // Miss se passou do ponto
        if (!wasHit && rect.anchoredPosition.y > 550f)
        {
            if (isEnemyNote)
            {
                // Nota do inimigo que passou → a IA errou
                GameManager.Instance.OnEnemyMiss();
            }
            else
            {
                Miss();
            }
            Destroy(gameObject);
        }

        // Lógica de Hold
        if (isHold && isBeingHeld)
        {
            holdTimer += Time.deltaTime;
            pointsFromHold = holdTimer; // 1 ponto por segundo

            if (holdTimer >= holdDuration)
            {
                // Terminou o hold com sucesso
                FinishHold(true);
            }
        }
    }

    public void Hit()
    {
        if (wasHit) return;

        wasHit = true;

        if (isHold)
        {
            isBeingHeld = true;
            // Não destrói ainda, só começa a contar o hold
        }
        else
        {
            // Nota normal
            GameManager.Instance.AddScore(10);
            Destroy(gameObject);
        }
    }

    public void ReleaseHold()
    {
        if (isHold && isBeingHeld)
        {
            FinishHold(false);
        }
    }

    void FinishHold(bool completed)
    {
        isBeingHeld = false;

        int points = 8 + Mathf.FloorToInt(pointsFromHold);
        GameManager.Instance.AddScore(points);

        Destroy(gameObject);
    }

    public void Miss()
    {
        GameManager.Instance.OnPlayerMiss();
        Destroy(gameObject);
    }
}