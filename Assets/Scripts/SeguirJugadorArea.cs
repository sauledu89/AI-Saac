using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeguirJugadorArea : MonoBehaviour
{
    public float radioBusqueda;
    public LayerMask capaJugador;
    public Transform transformJugador;

    public float velocidadMovimiento;
    public float distanciaMaxima;
    public Vector3 puntoInicial;

    public float distanciaEvitar = 1f; // Distancia para detectar obstáculos
    public LayerMask capaObstaculo;   // Capa de obstáculos

    public EstadosMovimiento estadoActual;
    public enum EstadosMovimiento
    {
        Esperando,
        Alejandose,
        Volviendo,
    }

    private void Start()
    {
        puntoInicial = transform.position;
    }

    private void FixedUpdate()
    {
        switch (estadoActual)
        {
            case EstadosMovimiento.Esperando:
                EstadoEsperando();
                break;
            case EstadosMovimiento.Alejandose:
                EstadoAlejandose();
                break;
            case EstadosMovimiento.Volviendo:
                EstadoVolviendo();
                break;
        }
    }

    private void EstadoEsperando()
    {
        Collider2D jugadorCollider = Physics2D.OverlapCircle(transform.position, radioBusqueda, capaJugador);

        if (jugadorCollider)
        {
            transformJugador = jugadorCollider.transform;
            estadoActual = EstadosMovimiento.Alejandose;
        }
    }

    private void EstadoAlejandose()
    {
        if (transformJugador == null)
        {
            estadoActual = EstadosMovimiento.Volviendo;
            return;
        }

        Vector2 direccionAlejarse = (transform.position - transformJugador.position).normalized;
        Vector2 nuevaPosicion = (Vector2)transform.position + direccionAlejarse * velocidadMovimiento * Time.deltaTime;

        // Obstacle Avoidance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlejarse, distanciaEvitar, capaObstaculo);
        if (hit.collider != null)
        {
            // Cambiar dirección si hay un obstáculo enfrente
            Vector2 direccionEvitar = Vector2.Perpendicular(direccionAlejarse);
            nuevaPosicion += direccionEvitar * velocidadMovimiento * Time.deltaTime;
        }

        transform.position = nuevaPosicion;

        if (Vector2.Distance(transform.position, puntoInicial) > distanciaMaxima)
        {
            estadoActual = EstadosMovimiento.Volviendo;
            transformJugador = null;
        }
    }

    private void EstadoVolviendo()
    {
        transform.position = Vector2.MoveTowards(transform.position, puntoInicial, velocidadMovimiento * Time.deltaTime);
        if (Vector2.Distance(transform.position, puntoInicial) < 0.1f)
        {
            estadoActual = EstadosMovimiento.Esperando;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioBusqueda);
        Gizmos.DrawWireSphere(puntoInicial, distanciaMaxima);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.left * distanciaEvitar);
        Gizmos.DrawRay(transform.position, Vector2.right * distanciaEvitar);
        Gizmos.DrawRay(transform.position, Vector2.up * distanciaEvitar);
        Gizmos.DrawRay(transform.position, Vector2.down * distanciaEvitar);
    }
}
