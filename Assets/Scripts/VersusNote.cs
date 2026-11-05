using UnityEngine;

/// <summary>Visual and timing behaviour for notes owned by a human player.</summary>
[DefaultExecutionOrder(-9000)]
public class VersusNote : MonoBehaviour
{
    public int Player { get; private set; }
    public KeyCode Key { get; private set; }
    public bool WasHit { get; private set; }

    private TurnBasedVersusController owner;
    private RectTransform rect;
    private RectTransform target;
    private float speed;
    private float hitWindow;
    private bool isHold;
    private float holdDuration;
    private float heldFor;
    private bool failed;

    public void Setup(TurnBasedVersusController controller, int player, KeyCode key,
        RectTransform hitTarget, float noteSpeed, float window, bool hold, float duration)
    {
        owner = controller;
        Player = player;
        Key = key;
        target = hitTarget;
        speed = noteSpeed;
        hitWindow = window;
        isHold = hold;
        holdDuration = duration;
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rect == null || target == null || failed) return;

        if (!WasHit)
        {
            rect.anchoredPosition += Vector2.up * speed * Time.deltaTime;
            Vector3 targetWorld = target.TransformPoint(target.rect.center);
            float targetY = rect.parent.InverseTransformPoint(targetWorld).y;
            if (rect.anchoredPosition.y > targetY + hitWindow)
                Fail();
            return;
        }

        if (!isHold) return;
        if (!Input.GetKey(Key)) { Fail(); return; }
        heldFor += Time.deltaTime;
        if (heldFor >= holdDuration) Destroy(gameObject);
    }

    public float DistanceToTarget()
    {
        if (rect == null || target == null) return float.MaxValue;
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera : null;
        Vector2 notePosition = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        Vector2 targetPosition = RectTransformUtility.WorldToScreenPoint(camera, target.TransformPoint(target.rect.center));
        return Vector2.Distance(notePosition, targetPosition);
    }

    public void Hit()
    {
        if (WasHit || failed) return;
        WasHit = true;
        if (!isHold) Destroy(gameObject);
    }

    public void Release()
    {
        if (isHold && WasHit && heldFor < holdDuration) Fail();
    }

    private void Fail()
    {
        if (failed) return;
        failed = true;
        if (owner != null) owner.OnNoteMiss(this);
        Destroy(gameObject);
    }
}
