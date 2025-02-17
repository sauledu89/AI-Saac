using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    public int vidaMaxima = 3; // Vida m�xima del enemigo
    private int vidaActual;

    public int da�oJugador = 1; // Da�o que hace al tocar al jugador
    public float tiempoColorDa�o = 1f; // Duraci�n del cambio de color al recibir da�o
    private SpriteRenderer spriteRenderer;

    public LayerMask capaBalasJugador; // Capa de balas del jugador
    public LayerMask capaJugador; // Capa del jugador

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontr� un SpriteRenderer en " + gameObject.name);
        }
    }

    // Funci�n para recibir da�o
    public void RecibirDa�o(int cantidad)
    {
        vidaActual -= cantidad;
        StartCoroutine(EfectoRecibirDa�o());

        if (vidaActual <= 0)
        {
            MatarEnemigo();
        }
    }

    private IEnumerator EfectoRecibirDa�o()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Cambia a rojo
            yield return new WaitForSeconds(tiempoColorDa�o); // Espera
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
            RecibirDa�o(1); // Recibir da�o
        }

        // Si colisiona con el jugador, le hace da�o
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            VidaJugador vidaJugador = collision.GetComponent<VidaJugador>();
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDa�o(da�oJugador);
            }
        }
    }
}
