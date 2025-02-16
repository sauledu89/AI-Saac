using UnityEngine;
using System.Collections;

public class SeguirJugadorPesado : MonoBehaviour
{
    public float radioBusqueda;
    public LayerMask capaJugador;
    public Transform transformJugador;

    public float velocidadMaxima;   // Velocidad máxima del enemigo
    public float aceleracion;       // Cuánto acelera por segundo
    private float velocidadActual;  // Velocidad del enemigo en cada frame
    public float distanciaMaxima;
    public Vector3 puntoInicial;

    public Rigidbody2D rb2D;
    private bool chocoConPared = false; // Indica si chocó con una pared

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

        if (!chocoConPared) // Solo se mueve si no ha chocado con una pared
        {
            velocidadActual = Mathf.Lerp(velocidadActual, velocidadMaxima, aceleracion * Time.deltaTime); // Aceleración progresiva

            if (transform.position.x < transformJugador.position.x)
            {
                rb2D.linearVelocity = new Vector2(velocidadActual, rb2D.linearVelocity.y);
            }
            else
            {
                rb2D.linearVelocity = new Vector2(-velocidadActual, rb2D.linearVelocity.y);
            }
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
        velocidadActual = Mathf.Lerp(velocidadActual, velocidadMaxima, aceleracion * Time.deltaTime); // Aceleración progresiva

        if (transform.position.x < puntoInicial.x)
        {
            rb2D.linearVelocity = new Vector2(velocidadActual, rb2D.linearVelocity.y);
        }
        else
        {
            rb2D.linearVelocity = new Vector2(-velocidadActual, rb2D.linearVelocity.y);
        }

        if (Vector2.Distance(transform.position, puntoInicial) < 0.1f)
        {
            rb2D.linearVelocity = Vector2.zero;
            estadoActual = EstadosMovimiento.Esperando;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("El enemigo pesado chocó contra la pared");

            rb2D.linearVelocity = Vector2.zero; // Detener completamente el movimiento
            velocidadActual = 0; // Reiniciar la velocidad a 0 para que vuelva a acelerar lentamente
            chocoConPared = true;

            // Volver a moverse tras 1 segundo
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
