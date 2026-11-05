using UnityEngine;

/// <summary>A note owned and judged by one of the two local players.</summary>
public class CoopNote : MonoBehaviour
{
    public int playerNumber;
    public KeyCode keyToPress;
    public RectTransform hitTarget;
    public float speed;
    public bool isHold;
    public float holdDuration;
    public float missY = 600f;

    private RectTransform rect;
    private bool wasHit;
    private bool isBeingHeld;
    private float holdTimer;

    public void Setup(int owner, KeyCode key, float noteSpeed, RectTransform target, bool hold, float duration, float missPosition)
    {
        playerNumber = owner;
        keyToPress = key;
        speed = noteSpeed;
        hitTarget = target;
        isHold = hold;
        holdDuration = duration;
        missY = missPosition;
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        if (wasHit && !isHold)
            return;

        if (!isBeingHeld)
            rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        if (!wasHit && rect.anchoredPosition.y > missY)
        {
            wasHit = true;
            if (CoopGameManager.Instance != null)
                CoopGameManager.Instance.OnPlayerMiss(playerNumber);
            Destroy(gameObject);
            return;
        }

        if (isHold && isBeingHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
                FinishHold();
        }
    }

    public float DistanceToTargetPixels()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (hitTarget == null || rect == null) return float.MaxValue;

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : null;
        Vector2 notePosition = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        Vector2 targetPosition = RectTransformUtility.WorldToScreenPoint(camera, hitTarget.TransformPoint(hitTarget.rect.center));
        return Vector2.Distance(notePosition, targetPosition);
    }

    public bool Hit()
    {
        if (wasHit) return false;
        wasHit = true;
        if (isHold)
            isBeingHeld = true;
        else
            Destroy(gameObject);
        return true;
    }

    public void ReleaseHold()
    {
        if (isHold && isBeingHeld)
            FinishHold();
    }

    private void FinishHold()
    {
        isBeingHeld = false;
        if (CoopGameManager.Instance != null)
            CoopGameManager.Instance.OnHoldFinished(playerNumber, holdTimer);
        Destroy(gameObject);
    }
}
