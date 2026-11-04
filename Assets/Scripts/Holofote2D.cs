using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Holofote2D : MonoBehaviour
{
    [Header("Luz")]
    private Light2D luz;

    [Header("Intensidade (Pulsa)")]
    public float intensidadeMin = 0.8f;
    public float intensidadeMax = 1.8f;
    public float velocidadePulso = 1.5f;

    [Header("Rotação (Holofote)")]
    public bool girar = true;
    public float anguloMaximo = 7f;
    public float velocidadeGiro = 0.8f;

    float tempo;

    void Start()
    {
        luz = GetComponent<Light2D>();
    }

    void Update()
    {
        tempo += Time.deltaTime;

        // --- Pulsar a intensidade ---
        float intensidade = Mathf.Lerp(intensidadeMin, intensidadeMax,
            (Mathf.Sin(tempo * velocidadePulso) + 1f) / 2f);
        luz.intensity = intensidade;

        // --- Girar como holofote (sem mexer a posição) ---
        if (girar)
        {
            float angulo = Mathf.Sin(tempo * velocidadeGiro) * anguloMaximo;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
        }
    }
}