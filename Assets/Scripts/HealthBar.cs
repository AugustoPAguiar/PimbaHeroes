using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public RectTransform redFill;      // a parte vermelha
    public RectTransform greenFill;    // a parte verde
    //public RectTransform centerMarker; // o "branco" do meio (opcional)

    [Range(0f, 1f)]
    public float health = 0.5f; // 0 = player perdeu, 1 = player ganhou

    public float changeSpeed = 0.15f; // quão rápido a barra se move

    void Update()
    {
        // Suaviza a mudança da barra
        float current = redFill.anchorMax.x; // ou o valor que estiver usando
        // Vamos usar uma abordagem simples com scale ou anchor

        // Método simples e eficaz:
        greenFill.anchorMax = new Vector2(health, 1f);
        redFill.anchorMin = new Vector2(health, 0f);
    }

    public void ChangeHealth(float amount)
    {
        health = Mathf.Clamp01(health + amount);

        if (health <= 0.02f)
        {
            GameManager.Instance.PlayerLose();
        }
        else if (health >= 0.98f)
        {
            GameManager.Instance.PlayerWin();
        }
    }
}