using System.Collections;
using UnityEngine;

// Moneda coleccionable: gira, flota y al recogerla hace una animación de "pop".
[RequireComponent(typeof(Collider2D))]
public class MonedaInti : MonoBehaviour
{
    public float velocidadGiro = 3f;
    public float alturaFlotar = 0.12f;

    private Vector3 posicionInicial;
    private Vector3 escalaInicial;
    private bool recogida;
    private float desfase;

    void Start()
    {
        posicionInicial = transform.position;
        escalaInicial = transform.localScale;
        desfase = Random.Range(0f, 6f);
    }

    void Update()
    {
        if (recogida) return;

        float t = Time.time + desfase;
        // "Giro" falso en 2D: la moneda se achica y crece en X
        transform.localScale = new Vector3(escalaInicial.x * Mathf.Cos(t * velocidadGiro),
                                           escalaInicial.y, escalaInicial.z);
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(t * 2f) * alturaFlotar;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogida || !otro.CompareTag("Player")) return;

        recogida = true;
        GetComponent<Collider2D>().enabled = false;
        if (GameManager.Instance != null) GameManager.Instance.RecogerMoneda();
        StartCoroutine(AnimacionRecoger());
    }

    // Animación al recoger: sube, crece y se desvanece
    IEnumerator AnimacionRecoger()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 inicio = transform.position;
        float duracion = 0.35f;

        for (float t = 0; t < duracion; t += Time.deltaTime)
        {
            float p = t / duracion;
            transform.position = inicio + Vector3.up * p * 0.8f;
            transform.localScale = escalaInicial * (1f + p);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - p);
            yield return null;
        }

        Destroy(gameObject);
    }
}
