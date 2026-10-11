using UnityEngine;

public class ReaparicionJugador : MonoBehaviour
{
    public Transform puntoInicio;
    public float limiteCaida = -8f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (transform.position.y < limiteCaida)
        {
            // Caer al vacío cuesta una vida (lo controla el GameManager)
            if (GameManager.Instance != null)
                GameManager.Instance.JugadorHerido(this, true);
            else
                Reaparecer();
        }
    }

    public void Reaparecer()
    {
        transform.position = puntoInicio.position;
        rb.linearVelocity = Vector2.zero;
    }
}
