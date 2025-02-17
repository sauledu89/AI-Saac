using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VidaJugador : MonoBehaviour
{
    public int VidaMaxima = 6;
    public int VidaActual;
    public Image[] VidaImagen;

    private float tiempoUltimoDaño;
    public float tiempoEsperaRegeneracion = 7f; // Tiempo antes de iniciar regeneración
    public float intervaloRegeneracion = 1f;    // Tiempo entre cada punto de vida restaurado

    private Coroutine regeneracionVida;
    private bool regenerando = false;      // Evita que la regeneración se active varias veces

    private SpriteRenderer spriteRenderer; // Referencia al SpriteRenderer del cuerpo
    public SpriteRenderer spriteRenderer2; // Referencia al SpriteRenderer de la cabeza
    public float tiempoColorDaño = 10f;    // Duración del cambio de color al recibir daño

    void Start()
    {
        VidaActual = VidaMaxima;
        actualizarInterfaz();
        tiempoUltimoDaño = Time.time;

        // Queremos obtener los 
        // Obtener el SpriteRenderer del cuerpo
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró un SpriteRenderer en el jugador.");
        }

        // Obtener el SpriteRenderer de la cabeza (Objeto hijo)
        if (spriteRenderer2 == null) // Verifica si no ha sido asignado en el Inspector
        {
            spriteRenderer2 = transform.Find("Head").GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer2 == null)
        {
            Debug.LogError("No se encontró un SpriteRenderer en la cabeza.");
        }
    }

    void actualizarInterfaz()
    {
        for (int i = 0; i < VidaImagen.Length; i++)
        {
            VidaImagen[i].enabled = i < VidaActual;
        }
        if (VidaActual <= 0)
        {
            ReiniciarEscena();
            Debug.Log("Has Perdido");
        }
    }

    void ReiniciarEscena()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void RecibirDaño(int CantidadDaño)
    {
        VidaActual -= CantidadDaño;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();

        // Cambiar el color para indicar daño
        StartCoroutine(EfectoRecibirDaño());

        // Reiniciar el contador de regeneración de vida
        tiempoUltimoDaño = Time.time;

        // Si hay una regeneración en curso, detenerla
        if (regeneracionVida != null)
        {
            StopCoroutine(regeneracionVida);
            regeneracionVida = null;
            regenerando = false;
        }

        // Iniciar un nuevo temporizador para regeneración
        StartCoroutine(IniciarRegeneracion());
    }
    
    IEnumerator EfectoRecibirDaño()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Cambiar a rojo
        }
        if (spriteRenderer2 != null)
        {
            spriteRenderer2.color = Color.red; // Cambiar a rojo
        }

        yield return new WaitForSeconds(tiempoColorDaño); // Esperar

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // Restaurar el color original
        }
        if (spriteRenderer2 != null)
        {
            spriteRenderer2.color = Color.white; // Restaurar el color original
        }
    }

    IEnumerator IniciarRegeneracion()
    {
        regenerando = true; // Marcar que la regeneración está en proceso
        yield return new WaitForSeconds(tiempoEsperaRegeneracion);

        // Si el jugador recibe daño durante este tiempo, la regeneración se cancela
        if (Time.time - tiempoUltimoDaño < tiempoEsperaRegeneracion)
        {
            regenerando = false;
            yield break; // Sale de la corrutina si el jugador recibió daño recientemente
        }

        regeneracionVida = StartCoroutine(RegenerarVidaGradualmente());
    }

    IEnumerator RegenerarVidaGradualmente()
    {
        while (VidaActual < VidaMaxima)
        {
            ObtenerVida(1);
            yield return new WaitForSeconds(intervaloRegeneracion);

            // Si el jugador recibió daño recientemente, se detiene la regeneración
            if (Time.time - tiempoUltimoDaño < tiempoEsperaRegeneracion)
            {
                regenerando = false;
                regeneracionVida = null;
                yield break; // Sale de la corrutina sin seguir regenerando vida
            }
        }

        // Una vez que la vida está completa, detener la referencia a la corrutina
        regeneracionVida = null;
        regenerando = false;
    }

    public void ObtenerVida(int CuraTotal)
    {
        VidaActual += CuraTotal;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("Vidas: " + VidaActual);

        if (Input.GetKey("r"))
        {
            Debug.Log("Reset");
            ReiniciarEscena();
        }
    }
}
