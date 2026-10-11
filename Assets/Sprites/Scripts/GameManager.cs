using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controla el reto del nivel: monedas Inti, vidas, meta, mensajes y sonidos.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Reto")]
    public Transform contenedorMonedas;   // Objeto padre que contiene todas las monedas
    public int vidasIniciales = 3;
    public float tiempoInvulnerable = 1.5f;

    [Header("Sonidos (opcional)")]
    public AudioClip sonidoMoneda;
    public AudioClip sonidoGolpe;
    public AudioClip sonidoVictoria;
    public AudioClip musicaFondo;

    private enum Estado { Jugando, Ganado, Perdido }
    private Estado estado = Estado.Jugando;

    private int totalMonedas;
    private int monedasRecogidas;
    private int vidas;
    private bool invulnerable;
    private string mensajeTemporal = "";
    private float tiempoMensaje;

    private AudioSource fuenteEfectos;
    private GUIStyle estiloTexto;
    private GUIStyle estiloSombra;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        fuenteEfectos = gameObject.AddComponent<AudioSource>();
        fuenteEfectos.playOnAwake = false;

        if (musicaFondo != null)
        {
            AudioSource fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteMusica.clip = musicaFondo;
            fuenteMusica.loop = true;
            fuenteMusica.volume = 0.35f;
            fuenteMusica.Play();
        }
    }

    void Start()
    {
        totalMonedas = contenedorMonedas != null ? contenedorMonedas.childCount : 0;
        vidas = vidasIniciales;
    }

    void Update()
    {
        // Al ganar o perder, la tecla R reinicia el nivel
        if (estado != Estado.Jugando && Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && fuenteEfectos != null)
            fuenteEfectos.PlayOneShot(clip);
    }

    public void RecogerMoneda()
    {
        if (estado != Estado.Jugando) return;

        monedasRecogidas++;
        ReproducirSonido(sonidoMoneda);

        if (monedasRecogidas == totalMonedas)
            MostrarMensaje("¡Tienes todos los Inti! Ve a la meta");
    }

    // Lo llaman los pinchos (esCaida = false) y la caída al vacío (esCaida = true)
    public void JugadorHerido(ReaparicionJugador jugador, bool esCaida)
    {
        if (estado != Estado.Jugando) return;

        // Durante la invulnerabilidad los pinchos no hacen daño,
        // pero si cae al vacío igual debe reaparecer.
        if (invulnerable)
        {
            if (esCaida) jugador.Reaparecer();
            return;
        }

        vidas--;
        ReproducirSonido(sonidoGolpe);

        if (vidas <= 0)
        {
            estado = Estado.Perdido;
            Time.timeScale = 0f;
            return;
        }

        jugador.Reaparecer();
        StartCoroutine(Parpadeo(jugador.gameObject));
    }

    public void LlegoMeta()
    {
        if (estado != Estado.Jugando) return;

        int faltan = totalMonedas - monedasRecogidas;
        if (faltan > 0)
        {
            MostrarMensaje("Te faltan " + faltan + " Inti para abrir la meta");
            return;
        }

        estado = Estado.Ganado;
        ReproducirSonido(sonidoVictoria);
        Time.timeScale = 0f;
    }

    void MostrarMensaje(string texto)
    {
        mensajeTemporal = texto;
        tiempoMensaje = Time.unscaledTime + 2.5f;
    }

    // El personaje parpadea en rojo mientras es invulnerable
    IEnumerator Parpadeo(GameObject jugador)
    {
        invulnerable = true;
        SpriteRenderer[] partes = jugador.GetComponentsInChildren<SpriteRenderer>();
        float fin = Time.time + tiempoInvulnerable;
        bool rojo = false;

        while (Time.time < fin)
        {
            rojo = !rojo;
            foreach (SpriteRenderer sr in partes)
                sr.color = rojo ? new Color(1f, 0.4f, 0.4f, 0.6f) : Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (SpriteRenderer sr in partes)
            sr.color = Color.white;
        invulnerable = false;
    }

    void OnGUI()
    {
        if (estiloTexto == null)
        {
            estiloTexto = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            estiloSombra = new GUIStyle(estiloTexto);
        }

        int tam = Mathf.Max(18, Screen.height / 22);
        estiloTexto.fontSize = tam;
        estiloSombra.fontSize = tam;
        estiloTexto.normal.textColor = new Color(1f, 0.85f, 0.2f);
        estiloSombra.normal.textColor = Color.black;
        estiloTexto.alignment = TextAnchor.UpperLeft;
        estiloSombra.alignment = TextAnchor.UpperLeft;

        Texto(new Rect(20, 15, 600, tam * 1.5f), "Inti: " + monedasRecogidas + " / " + totalMonedas);
        estiloTexto.normal.textColor = new Color(1f, 0.45f, 0.45f);
        Texto(new Rect(20, 15 + tam * 1.4f, 600, tam * 1.5f), "Vidas: " + new string('♥', Mathf.Max(0, vidas)));

        estiloTexto.fontSize = estiloSombra.fontSize = Mathf.Max(12, tam / 2);
        estiloTexto.normal.textColor = Color.white;
        Texto(new Rect(20, Screen.height - tam * 1.2f, 1000, tam),
              "← → moverse   ESPACIO saltar   X latigazo");

        estiloTexto.alignment = estiloSombra.alignment = TextAnchor.MiddleCenter;
        Rect centro = new Rect(0, Screen.height * 0.3f, Screen.width, tam * 3);

        if (estado == Estado.Ganado)
        {
            PantallaFinal(tam, "¡NIVEL COMPLETADO!", "El Diablo Huma protegió los Andes",
                          "Presiona R para jugar otra vez", new Color(1f, 0.85f, 0.2f));
        }
        else if (estado == Estado.Perdido)
        {
            PantallaFinal(tam, "GAME OVER", "Los Inti quedaron sin guardián",
                          "Presiona R para reintentar", new Color(1f, 0.35f, 0.35f));
        }
        else if (Time.unscaledTime < tiempoMensaje)
        {
            estiloTexto.fontSize = estiloSombra.fontSize = tam;
            estiloTexto.normal.textColor = Color.white;
            Texto(centro, mensajeTemporal);
        }
    }

    // Fondo oscuro + título, frase y subtítulo en filas separadas para que no se corten
    void PantallaFinal(int tam, string titulo, string frase, string subtitulo, Color color)
    {
        Color anterior = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = anterior;

        float y = Screen.height * 0.28f;

        estiloTexto.fontSize = estiloSombra.fontSize = Mathf.RoundToInt(tam * 1.8f);
        estiloTexto.normal.textColor = color;
        Texto(new Rect(0, y, Screen.width, tam * 2.6f), titulo);

        estiloTexto.fontSize = estiloSombra.fontSize = Mathf.RoundToInt(tam * 1.1f);
        Texto(new Rect(0, y + tam * 2.6f, Screen.width, tam * 1.8f), frase);

        estiloTexto.fontSize = estiloSombra.fontSize = tam;
        estiloTexto.normal.textColor = Color.white;
        Texto(new Rect(0, y + tam * 4.8f, Screen.width, tam * 1.6f), subtitulo);
    }

    void Texto(Rect r, string texto)
    {
        Rect sombra = new Rect(r.x + 2, r.y + 2, r.width, r.height);
        GUI.Label(sombra, texto, estiloSombra);
        GUI.Label(r, texto, estiloTexto);
    }
}
