using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;

    private Rigidbody2D rb;
    private float movimiento;
    private bool enAire;
    private Animator[] animaciones;
    private bool quiereSaltar;
    private bool enSuelo;

    [Header("Latigazo (tecla X)")]
    public AudioClip sonidoLatigo;
    private bool quiereAtacar;
    private Transform partesVisuales;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animaciones = GetComponentsInChildren<Animator>();

        Debug.Log("Animator encontrados: " + animaciones.Length);

        // Las partes del cuerpo se voltean para mirar hacia donde camina
        partesVisuales = transform.Find("Partes_DiabloHuma");
    }

    void Update()
    {
        movimiento = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                movimiento = -1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                movimiento = 1f;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                quiereSaltar = true;
            }

            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                quiereAtacar = true;
            }
        }

        if (partesVisuales != null && movimiento != 0f)
        {
            Vector3 escala = partesVisuales.localScale;
            escala.x = Mathf.Abs(escala.x) * Mathf.Sign(movimiento);
            partesVisuales.localScale = escala;
        }

        if (quiereAtacar && GameManager.Instance != null)
        {
            GameManager.Instance.ReproducirSonido(sonidoLatigo);
        }

        foreach (Animator anim in animaciones)
        {
            if (anim == null)
                continue;

            bool tieneVelocidad = false;
            bool tieneEnAire = false;
            bool tieneAtacar = false;

            foreach (AnimatorControllerParameter parametro in anim.parameters)
            {
                if (parametro.name == "Velocidad" &&
                    parametro.type == AnimatorControllerParameterType.Float)
                {
                    tieneVelocidad = true;
                }

                if (parametro.name == "EnAire" &&
                    parametro.type == AnimatorControllerParameterType.Bool)
                {
                    tieneEnAire = true;
                }

                if (parametro.name == "Atacar" &&
                    parametro.type == AnimatorControllerParameterType.Trigger)
                {
                    tieneAtacar = true;
                }
            }

            if (tieneVelocidad)
            {
                anim.SetFloat("Velocidad", Mathf.Abs(movimiento));
            }
            if (tieneEnAire)
            {
                anim.SetBool("EnAire", enAire);
            }

            if (tieneAtacar && quiereAtacar)
            {
                anim.SetTrigger("Atacar");
            }
        }

        quiereAtacar = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            movimiento * velocidad,
            rb.linearVelocity.y
        );

        if (quiereSaltar && enSuelo)
        {
            enSuelo = false;
            enAire = true;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                fuerzaSalto
            );
        }

        quiereSaltar = false;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        RevisarSuelo(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        RevisarSuelo(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        enSuelo = false;
    }

    void RevisarSuelo(Collision2D collision)
    {
        foreach (ContactPoint2D contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.5f && rb.linearVelocity.y <= 0f)
            {
                enSuelo = true;
                enAire = false;
                return;
            }
        }
    }

}
