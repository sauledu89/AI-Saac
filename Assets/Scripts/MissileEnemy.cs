using UnityEngine;

/// <summary>
/// Script para el misil enemigo que persigue al jugador con giro suave y puede ser destruido.
/// </summary>
public class MissileEnemy : MonoBehaviour
{
    public float velocidad = 5f;             // Velocidad de avance
    public float velocidadRotacion = 200f;   // Qué tan rápido puede rotar hacia el jugador
    public int vidaMisil = 3;                // Vida inicial del misil (impactos que puede recibir)
    public float tiempoMaximoVida = 10f;     // Tiempo máximo antes de autodestruirse

    public LayerMask capasBalasJugador;      // Layers que dañan al misil
    public LayerMask capasJugador;           // Layer del jugador

    private Transform objetivoJugador;
    private float tiempoVivo = 0f;
    private Rigidbody2D rb;

    private void Start()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            objetivoJugador = jugador.transform;
        }
        else
        {
            Debug.LogWarning("MissileEnemy: No se encontró ningún objeto con tag 'Player'.");
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("MissileEnemy: No se encontró Rigidbody2D.");
        }
    }

    private void Update()
    {
        if (objetivoJugador != null && rb != null)
        {
            Vector2 direccion = (objetivoJugador.position - transform.position).normalized;

            // Calcular el ángulo deseado
            float anguloDeseado = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

            // Rotar suavemente hacia el jugador
            float anguloActual = rb.rotation;
            float nuevoAngulo = Mathf.MoveTowardsAngle(anguloActual, anguloDeseado, velocidadRotacion * Time.deltaTime);
            rb.MoveRotation(nuevoAngulo);

            // Mover siempre hacia adelante (hacia donde está mirando)
            rb.linearVelocity = transform.right * velocidad;
        }

        // Contador de vida del misil
        tiempoVivo += Time.deltaTime;
        if (tiempoVivo >= tiempoMaximoVida)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layerImpacto = collision.gameObject.layer;

        if ((capasBalasJugador.value & (1 << layerImpacto)) != 0)
        {
            vidaMisil--;

            if (vidaMisil <= 0)
            {
                Destroy(gameObject);
            }

            Destroy(collision.gameObject);
        }

        if ((capasJugador.value & (1 << layerImpacto)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
