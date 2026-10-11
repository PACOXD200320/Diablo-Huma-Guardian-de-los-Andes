using UnityEngine;

public class MetaNivel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Player"))
        {
            Debug.Log("El jugador llegó a la meta");

            if (GameManager.Instance != null)
                GameManager.Instance.LlegoMeta();
        }
    }
}
