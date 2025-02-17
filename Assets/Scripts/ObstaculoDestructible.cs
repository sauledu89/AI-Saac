using UnityEngine;
using System.Collections;

public class ObstaculoDestructible : MonoBehaviour
{
    public int vidaMaxima = 3; // Cantidad de impactos que soporta el obst�culo antes de destruirse
    private int vidaActual;

    public float tiempoColorDa�o = 0.2f; // Duraci�n del cambio de color al recibir da�o
    private SpriteRenderer spriteRenderer;

    public LayerMask capaBalasJugador; // Capa de balas del jugador
    public LayerMask capaJugador; // Capa del jugador
    public int da�oAlJugador = 1; // Da�o que hace al jugador si lo toca

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
            DestruirObstaculo();
        }
    }

    private IEnumerator EfectoRecibirDa�o()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red; // Cambia a rojo
            yield return new WaitForSeconds(tiempoColorDa�o); // Espera
            spriteRenderer.color = Color.cyan; // Vuelve al color original
        }
    }

    private void DestruirObstaculo()
    {
        Debug.Log(gameObject.name + " ha sido destruido.");
        Destroy(gameObject); // Destruir el obst�culo
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar colisi�n con el jugador
        if (((1 << collision.gameObject.layer) & capaJugador) != 0)
        {
            Debug.Log(gameObject.name + " ha colisionado con el jugador.");

            VidaJugador vidaJugador = collision.gameObject.GetComponent<VidaJugador>();
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDa�o(da�oAlJugador); // Da�o al jugador si lo toca
            }
        }
    }
}
