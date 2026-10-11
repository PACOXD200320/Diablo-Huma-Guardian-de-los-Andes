using UnityEngine;

// Obstáculo: si el jugador toca los pinchos pierde una vida y reaparece.
public class Pinchos : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D otro)
    {
        Danar(otro);
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        Danar(otro);
    }

    void Danar(Collider2D otro)
    {
        if (!otro.CompareTag("Player")) return;

        ReaparicionJugador jugador = otro.GetComponent<ReaparicionJugador>();
        if (jugador != null && GameManager.Instance != null)
            GameManager.Instance.JugadorHerido(jugador, false);
    }
}
