using UnityEngine;
using System.Collections;

public class ObstaculoDestructible : MonoBehaviour
{
    public int vidaMaxima = 3; // Cantidad de impactos que soporta el obstáculo antes de destruirse
    private int vidaActual;

    public float tiempoColorDaño = 0.2f; // Duración del cambio de color al recibir daño
    private SpriteRenderer spriteRenderer;

    public LayerMask capaBalasJugador; // Capa de balas del jugador
    public LayerMask capaJugador; // Capa del jugador
    public int dañoAlJugador = 1; // Daño que hace al jugador si lo toca

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró un SpriteRenderer en " + gameObject.name);
        }
    }

    // Función para recibir daño
    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        StartCoroutine(EfectoRecibirDaño());

        if (vidaActual <= 0)
        {
            DestruirObstaculo();
        }
    }

    private IEnumerator EfectoRecibirDaño()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Cambia a rojo
            yield return new WaitForSeconds(tiempoColorDaño); // Espera
            spriteRenderer.color = Color.cyan; // Vuelve al color original
        }
    }

    private void DestruirObstaculo()
    {
        Debug.Log(gameObject.name + " ha sido destruido.");
        Destroy(gameObject); // Destruir el obstáculo
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar colisión con el jugador
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            Debug.Log(gameObject.name + " ha colisionado con el jugador.");

            VidaJugador vidaJugador = collision.gameObject.GetComponent<VidaJugador>();
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDaño(dañoAlJugador); // Daño al jugador si lo toca
            }
        }
    }
}
