using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Nota")]
    public float speed = 9f;
    public KeyCode keyToPress;
    public bool isHold;
    public float holdDuration;
    public bool isEnemyNote;

    [Header("Julgamento")]
    public float missY = 1200f;
    public float enemyHitY = 0f;

    [Range(0f, 1f)]
    public float enemyMissChance = 0.8f;

    public bool wasHit;

    private RectTransform rect;
    private bool isBeingHeld;
    private float holdTimer;
    private float previousY;
    public RectTransform hitTarget;
    public float perfectTolerancePixels = 4f;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        previousY = rect.anchoredPosition.y;
    }

    public void Setup(KeyCode key, float noteSpeed, bool hold, float duration)
    {
        keyToPress = key;
        speed = noteSpeed;
        isHold = hold;
        holdDuration = duration;
    }

    private void Update()
    {
        if (isEnemyNote)
        {
            UpdateEnemyNote();
            return;
        }

        UpdatePlayerNote();
    }

    private void UpdateEnemyNote()
    {
        if (hitTarget == null)
        {
            Debug.LogError("Esta nota inimiga não tem Hit Target.", this);
            return;
        }

        MoveUp();

        float currentY = rect.anchoredPosition.y;

        // Converte a posição do peixe para o mesmo espaço local da nota.
        Vector3 targetCenter = hitTarget.TransformPoint(hitTarget.rect.center);
        float targetY = rect.parent.InverseTransformPoint(targetCenter).y;

        // A nota acerta quando cruza a altura do peixe.
        if (previousY < targetY && currentY >= targetY)
        {
            JudgeEnemyNote();
            return;
        }

        previousY = currentY;
    }

    private void JudgeEnemyNote()
    {
        wasHit = true;

        bool enemyMissed = enemyMissChance >= 1f || (enemyMissChance > 0f && Random.value < enemyMissChance);

        if (enemyMissed)
        {
            GameManager.Instance.OnEnemyMiss();
        }
        else
        {
            GameManager.Instance.OnEnemyHit();
        }

        Destroy(gameObject);
    }

    private void UpdatePlayerNote()
    {
        if (wasHit && !isHold)
            return;

        if (!isBeingHeld)
            MoveUp();

        if (!wasHit && rect.anchoredPosition.y > missY)
        {
            Miss();
            return;
        }

        if (isHold && isBeingHeld)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
                FinishHold();
        }
    }

    private void MoveUp()
    {
        rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;
    }

    public void Hit()
    {
        if (wasHit || isEnemyNote)
            return;

        wasHit = true;

        if (isHold)
        {
            isBeingHeld = true;
        }
        else
        {
            GameManager.Instance.AddScore(10);
            Destroy(gameObject);
        }
    }

    public void ReleaseHold()
    {
        if (isHold && isBeingHeld)
            FinishHold();
    }

    private void FinishHold()
    {
        isBeingHeld = false;

        int points = 8 + Mathf.FloorToInt(holdTimer);
        GameManager.Instance.AddScore(points);

        Destroy(gameObject);
    }

    public void Miss()
    {
        if (isEnemyNote)
            return;

        GameManager.Instance.OnPlayerMiss();
        Destroy(gameObject);
    }

    public float GetDistanceToHitTargetPixels()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (hitTarget == null)
            return float.MaxValue;

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null &&
                        canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : null;

        Vector3 noteCenter = rect.TransformPoint(rect.rect.center);
        Vector3 targetCenter = hitTarget.TransformPoint(hitTarget.rect.center);

        Vector2 noteScreenPosition =
            RectTransformUtility.WorldToScreenPoint(camera, noteCenter);

        Vector2 targetScreenPosition =
            RectTransformUtility.WorldToScreenPoint(camera, targetCenter);

        return Vector2.Distance(noteScreenPosition, targetScreenPosition);
    }
}