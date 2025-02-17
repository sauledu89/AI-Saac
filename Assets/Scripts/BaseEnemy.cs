using UnityEngine;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    public int vidaMaxima = 3;
    private int vidaActual;
    public float tiempoColorDaño = 0.2f;
    private SpriteRenderer spriteRenderer;

    public LayerMask capaJugador;
    public int dañoJugador = 1; // Daño al tocar al jugador

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró un SpriteRenderer en " + gameObject.name);
        }
    }

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
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(tiempoColorDaño);
            spriteRenderer.color = Color.white;
        }
    }

    private void MatarEnemigo()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta al jugador usando LayerMask
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
