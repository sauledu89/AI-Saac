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

    private float tiempoUltimoDa�o;
    public float tiempoEsperaRegeneracion = 7f; // Tiempo antes de iniciar regeneraci�n
    public float intervaloRegeneracion = 1f;    // Tiempo entre cada punto de vida restaurado

    private Coroutine regeneracionVida;
    private bool regenerando = false; // Evita que la regeneraci�n se active varias veces

    void Start()
    {
        VidaActual = VidaMaxima;
        actualizarInterfaz();
        tiempoUltimoDa�o = Time.time;
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

    public void RecibirDa�o(int CantidadDa�o)
    {
        VidaActual -= CantidadDa�o;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();

        // Reiniciar el contador de regeneraci�n de vida
        tiempoUltimoDa�o = Time.time;

        // Si hay una regeneraci�n en curso, detenerla
        if (regeneracionVida != null)
        {
            StopCoroutine(regeneracionVida);
            regeneracionVida = null;
            regenerando = false;
        }

        // Iniciar un nuevo temporizador para regeneraci�n
        StartCoroutine(IniciarRegeneracion());
    }

    IEnumerator IniciarRegeneracion()
    {
        regenerando = true; // Marcar que la regeneraci�n est� en proceso
        yield return new WaitForSeconds(tiempoEsperaRegeneracion);

        // Si el jugador recibe da�o durante este tiempo, la regeneraci�n se cancela
        if (Time.time - tiempoUltimoDa�o < tiempoEsperaRegeneracion)
        {
            regenerando = false;
            yield break; // Sale de la corrutina si el jugador recibi� da�o recientemente
        }

        regeneracionVida = StartCoroutine(RegenerarVidaGradualmente());
    }

    IEnumerator RegenerarVidaGradualmente()
    {
        while (VidaActual < VidaMaxima)
        {
            ObtenerVida(1);
            yield return new WaitForSeconds(intervaloRegeneracion);

            // Si el jugador recibi� da�o recientemente, se detiene la regeneraci�n
            if (Time.time - tiempoUltimoDa�o < tiempoEsperaRegeneracion)
            {
                regenerando = false;
                regeneracionVida = null;
                yield break; // Sale de la corrutina sin seguir regenerando vida
            }
        }

        // Una vez que la vida est� completa, detener la referencia a la corrutina
        regeneracionVida = null;
        regenerando = false;
    }


    public void ObtenerVida (int CuraTotal)
    {
        VidaActual += CuraTotal;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Input.GetKey("r"))
        {
            Debug.Log("Reset");
            ReiniciarEscena();
        }

        Debug.Log("tiempoEsperaRegeneracion: " + tiempoEsperaRegeneracion);
        Debug.Log("Vidas: " + VidaActual);

    }
}
