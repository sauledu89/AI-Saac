using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    public int vidaMaxima = 3; // Vida máxima del enemigo
    private int vidaActual;

    public int dañoJugador = 1; // Daño que hace al tocar al jugador
    public float tiempoColorDaño = 1f; // Duración del cambio de color al recibir daño
    private SpriteRenderer spriteRenderer;

    public LayerMask capaBalasJugador; // Capa de balas del jugador
    public LayerMask capaJugador; // Capa del jugador

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
            MatarEnemigo();
        }
    }

    private IEnumerator EfectoRecibirDaño()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Cambia a rojo
            yield return new WaitForSeconds(tiempoColorDaño); // Espera
            spriteRenderer.color = Color.blue; // Vuelve al color original
        }
    }

    private void MatarEnemigo()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject); // Destruir enemigo
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si colisiona con una bala del jugador (usando LayerMask)
        if (((1 << collision.gameObject.layer) & capaBalasJugador) != 0)
        {
            Destroy(collision.gameObject); // Destruir la bala
            RecibirDaño(1); // Recibir daño
        }

        // Si colisiona con el jugador, le hace daño
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            VidaJugador vidaJugador = collision.GetComponent<VidaJugador>();
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDaño(dañoJugador);
            }
        }
    }
}
