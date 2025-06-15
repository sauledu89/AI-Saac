using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour
{
    public int vidaMaxima = 3;
    public int vidaActual;
    public float tiempoColorDaño = 0.2f;
    private SpriteRenderer spriteRenderer;

    public LayerMask capaJugador;
    public int dañoJugador = 1;

    public static List<BaseEnemy> EnemigosVivos = new List<BaseEnemy>();

    // NUEVO: color original que se conserva por ronda
    protected Color colorRonda = Color.white;

    private void Start()
    {
        vidaActual = vidaMaxima;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró un SpriteRenderer en " + gameObject.name);
        }

        gameObject.layer = LayerMask.NameToLayer("Enemy");

        // Registrar en lista de enemigos activos
        if (!EnemigosVivos.Contains(this))
            EnemigosVivos.Add(this);
    }

    //NUEVO: método para asignar el color desde el spawner (por ronda)
    public void EstablecerColorPorRonda(Color color)
    {
        colorRonda = color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorRonda;
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
            spriteRenderer.color = colorRonda;  //volver al color original de la ronda
        }
    }

    private void MatarEnemigo()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        if (EnemigosVivos.Contains(this))
            EnemigosVivos.Remove(this);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
