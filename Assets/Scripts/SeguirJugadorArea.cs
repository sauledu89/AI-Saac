using UnityEngine;
using System.Collections;

public class EnemigoEscapista : MonoBehaviour
{
    public float radioBusqueda;
    public LayerMask capaJugador;
    public Transform transformJugador;

    public float velocidadMovimiento;
    public float distanciaMaxima;
    public Vector3 puntoInicial;

    public float distanciaEvitar = 1f; // Distancia para detectar obst�culos
    public LayerMask capaObstaculo;   // Capa de obst�culos

    public GameObject balaEnemigo;  // Prefab de la bala
    public float FireRate = 1f;      // Tiempo entre disparos
    private float tiempoUltimoDisparo;

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
            // Cambiar direcci�n si hay un obst�culo enfrente
            Vector2 direccionEvitar = Vector2.Perpendicular(direccionAlejarse);
            nuevaPosicion += direccionEvitar * velocidadMovimiento * Time.deltaTime;
        }

        transform.position = nuevaPosicion;

        // Disparar mientras se aleja
        Disparar();

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

    private void Disparar()
    {
        if (Time.time > FireRate + tiempoUltimoDisparo && transformJugador != null)
        {
            tiempoUltimoDisparo = Time.time;

            // Calcular direcci�n hacia el jugador
            Vector2 direccionJugador = (transformJugador.position - transform.position).normalized;
            float angulo = Mathf.Atan2(direccionJugador.y, direccionJugador.x) * Mathf.Rad2Deg;
            Quaternion rotacionBala = Quaternion.Euler(0, 0, angulo);

            // Instanciar la bala en la direcci�n correcta
            Instantiate(balaEnemigo, transform.position, rotacionBala);
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
