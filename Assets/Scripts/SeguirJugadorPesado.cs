using UnityEngine;
using System.Collections;

public class EnemigoPesado : MonoBehaviour
{
    public float radioBusqueda;
    public LayerMask capaJugador;
    public Transform transformJugador;

    public float velocidadMaxima;
    public float aceleracion;
    public float desaceleracion; // Parámetro para hacer que le cueste frenar
    private float velocidadActual;
    public float distanciaMaxima;
    public Vector3 puntoInicial;

    public float tiempoPrediccion = 0.5f; // Tiempo en segundos para predecir la posición futura del jugador

    public Rigidbody2D rb2D;
    private bool chocoConPared = false;

    public EstadosMovimiento estadoActual;

    public enum EstadosMovimiento
    {
        Esperando,
        Siguiendo,
        Volviendo,
    }

    private void Start()
    {
        puntoInicial = transform.position;
        velocidadActual = 0; // Comienza sin velocidad
        //rb2D.constraints = RigidbodyConstraints2D.FreezeRotation; // Evita que rote
    }

    private void FixedUpdate()
    {
        switch (estadoActual)
        {
            case EstadosMovimiento.Esperando:
                EstadoEsperando();
                break;
            case EstadosMovimiento.Siguiendo:
                EstadoSiguiendo();
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
            estadoActual = EstadosMovimiento.Siguiendo;
        }
    }

    private void EstadoSiguiendo()
    {
        if (transformJugador == null)
        {
            estadoActual = EstadosMovimiento.Volviendo;
            return;
        }

        // Predecir la posición futura del jugador
        Vector2 posicionFutura = PredecirPosicionJugador();

        // Dirección hacia la posición futura del jugador
        Vector2 direccion = (posicionFutura - (Vector2)transform.position).normalized;

        // Aceleración progresiva
        velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadMaxima, aceleracion * Time.deltaTime);

        if (!chocoConPared)
        {
            rb2D.linearVelocity = direccion * velocidadActual; // Movimiento en X y Y
        }

        // Si se aleja demasiado del punto inicial o del jugador, regresa
        if (Vector2.Distance(transform.position, puntoInicial) > distanciaMaxima ||
            Vector2.Distance(transform.position, transformJugador.position) > distanciaMaxima)
        {
            estadoActual = EstadosMovimiento.Volviendo;
            transformJugador = null;
        }
    }

    private void EstadoVolviendo()
    {
        Vector2 direccionRegreso = ((Vector2)puntoInicial - (Vector2)transform.position).normalized;

        // Desaceleración progresiva antes de cambiar de dirección
        if (Vector2.Distance(transform.position, puntoInicial) > 1f)
        {
            velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadMaxima, aceleracion * Time.deltaTime);
        }
        else
        {
            velocidadActual = Mathf.MoveTowards(velocidadActual, 0, desaceleracion * Time.deltaTime);
        }

        rb2D.linearVelocity = direccionRegreso * velocidadActual;

        // Cuando llega a su punto inicial, se detiene completamente
        if (Vector2.Distance(transform.position, puntoInicial) < 0.1f)
        {
            rb2D.linearVelocity = Vector2.zero;
            velocidadActual = 0; // Reiniciar velocidad
            estadoActual = EstadosMovimiento.Esperando;
        }
    }

    private Vector2 PredecirPosicionJugador()
    {
        if (transformJugador == null) return transform.position;

        Rigidbody2D rbJugador = transformJugador.GetComponent<Rigidbody2D>();
        if (rbJugador == null) return transformJugador.position;

        // Calcular la posición futura del jugador basado en su velocidad actual
        Vector2 posicionFutura = (Vector2)transformJugador.position + (rbJugador.linearVelocity * tiempoPrediccion);
        return posicionFutura;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("El enemigo pesado chocó contra la pared");

            rb2D.linearVelocity = Vector2.zero;
            velocidadActual = 0; // Se detiene por completo
            chocoConPared = true;

            StartCoroutine(RecuperarMovimiento());
        }
    }

    IEnumerator RecuperarMovimiento()
    {
        yield return new WaitForSeconds(1f);
        chocoConPared = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioBusqueda);
        Gizmos.DrawWireSphere(puntoInicial, distanciaMaxima);
    }
}
