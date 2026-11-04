using UnityEngine;

public class Note : MonoBehaviour
{
    public float speed = 9f;
    public KeyCode keyToPress;
    public bool isHold;
    public float holdDuration;
    public bool isEnemyNote;
    public float missY = 600f;
    public bool wasHit;
    public RectTransform hitTarget;

    private RectTransform rect;
    private bool isBeingHeld;
    private float holdTimer;
    private float previousY;

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
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (isEnemyNote)
        {
            UpdateEnemyNote();
            return;
        }

        if (wasHit && !isHold)
            return;

        if (!isBeingHeld)
            MoveUp();

        if (!wasHit && rect.anchoredPosition.y > missY)
            Miss();

        if (isHold && isBeingHeld && (holdTimer += Time.deltaTime) >= holdDuration)
            FinishHold();
    }

    private void UpdateEnemyNote()
    {
        MoveUp();
        if (hitTarget == null)
            return;

        Vector3 targetCenter = hitTarget.TransformPoint(hitTarget.rect.center);
        float targetY = rect.parent.InverseTransformPoint(targetCenter).y;
        float currentY = rect.anchoredPosition.y;

        if (previousY < targetY && currentY >= targetY)
        {
            bool enemyMissed = GameManager.Instance.RollEnemyMiss();
            if (enemyMissed)
                GameManager.Instance.OnEnemyMiss();
            else
            {
                GameManager.Instance.OnEnemyHit();
                Destroy(gameObject);
            }
        }

        previousY = currentY;
    }

    private void MoveUp() => rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;

    public void Hit()
    {
        if (wasHit || isEnemyNote)
            return;

        wasHit = true;
        if (isHold)
            isBeingHeld = true;
        else
            Destroy(gameObject);
    }

    public void ReleaseHold()
    {
        if (isHold && isBeingHeld)
            FinishHold();
    }

    private void FinishHold()
    {
        isBeingHeld = false;
        GameManager.Instance.AddScore(8 + Mathf.FloorToInt(holdTimer));
        Destroy(gameObject);
    }

    public void Miss()
    {
        if (isEnemyNote || wasHit)
            return;

        wasHit = true;
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
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : null;
        Vector2 notePosition = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        Vector2 targetPosition = RectTransformUtility.WorldToScreenPoint(camera, hitTarget.TransformPoint(hitTarget.rect.center));
        return Vector2.Distance(notePosition, targetPosition);
    }
}
