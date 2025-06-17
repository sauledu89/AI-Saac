using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public int vidaMaxima;
    public int vidaActual;
    public float tiempoColorDaño = 0.2f;
    protected SpriteRenderer spriteRenderer;

    public LayerMask capaJugador;
    public int dañoJugador = 1;

    public static List<BaseEnemy> EnemigosVivos = new List<BaseEnemy>();

    protected Color colorRonda = Color.white;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogError("No se encontró un SpriteRenderer en " + gameObject.name);

        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public virtual void EstablecerColorPorRonda(Color color)
    {
        colorRonda = color;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = colorRonda;
    }

    public virtual void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        StartCoroutine(EfectoRecibirDaño());

        if (vidaActual <= 0)
            MatarEnemigo();
    }

    private IEnumerator EfectoRecibirDaño()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(tiempoColorDaño);
            spriteRenderer.color = colorRonda;
        }
    }

    protected virtual void MatarEnemigo()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        if (EnemigosVivos.Contains(this))
            EnemigosVivos.Remove(this);

        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
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
