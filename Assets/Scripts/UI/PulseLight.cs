using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulseLight : MonoBehaviour
{
    [Header("Luz")]
    private Light2D luz;

    [Header("Intensidade (Pulsa)")]
    public float intensidadeMin = 0.8f;
    public float intensidadeMax = 1.8f;
    public float velocidadePulso = 1.5f;

    float tempo;

    void Start()
    {
        luz = GetComponent<Light2D>();
    }

    void Update()
    {
        tempo += Time.deltaTime;

        // --- Pulsar a intensidade ---
        float intensidade = Mathf.Lerp(intensidadeMin, intensidadeMax, (Mathf.Sin(tempo * velocidadePulso) + 1f) / 2f);
        luz.intensity = intensidade;
    }
}
