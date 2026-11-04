using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public RectTransform redFill;
    public RectTransform greenFill;

    [Range(0f, 1f)]
    public float health = 0.5f;

    public float changeSpeed = 0.15f;

    private float displayedHealth;

    void Start()
    {
        displayedHealth = health;
        UpdateBar(displayedHealth);
    }

    void Update()
    {
        displayedHealth = Mathf.MoveTowards( displayedHealth, health, changeSpeed * Time.deltaTime);

        UpdateBar(displayedHealth);
    }

    void UpdateBar(float value)
    {
        redFill.anchorMin = new Vector2(0f, 0f);
        redFill.anchorMax = new Vector2(value, 1f);
        redFill.offsetMin = Vector2.zero;
        redFill.offsetMax = Vector2.zero;

        greenFill.anchorMin = new Vector2(value, 0f);
        greenFill.anchorMax = new Vector2(1f, 1f);
        greenFill.offsetMin = Vector2.zero;
        greenFill.offsetMax = Vector2.zero;
    }

    public void ChangeHealth(float amount)
    {
        if (GameManager.Instance == null) return;

        health = Mathf.Clamp01(health - amount);

        // Só chama se o jogo ainda não acabou
        if (health <= 0.05f)
        {
            GameManager.Instance.PlayerWin();
        }
        else if (health >= 0.95f)
        {
            GameManager.Instance.PlayerLose();
        }
    }
}