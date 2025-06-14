using UnityEngine;
using System.Collections;

/// <summary>
/// Script para el misil enemigo que persigue al jugador con giro suave, cambia de color al ser dañado y puede ser destruido.
/// </summary>
public class MissileEnemy : MonoBehaviour
{
    public float velocidad = 5f;
    public float velocidadRotacion = 200f;
    public int vidaMisil = 3;
    public float tiempoMaximoVida = 10f;

    public LayerMask capasBalasJugador;
    public LayerMask capasJugador;

    private Transform objetivoJugador;
    private float tiempoVivo = 0f;
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    private bool estaDañado = false;

    private void Start()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
            objetivoJugador = jugador.transform;
        else
            Debug.LogWarning("MissileEnemy: No se encontró ningún objeto con tag 'Player'.");

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning("MissileEnemy: No se encontró Rigidbody2D.");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            colorOriginal = spriteRenderer.color;
    }

    private void Update()
    {
        if (objetivoJugador != null && rb != null)
        {
            // Calcular dirección
            Vector2 direccion = (objetivoJugador.position - transform.position).normalized;

            // Calcular el ángulo deseado
            float anguloDeseado = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

            // Rotar suavemente hacia el jugador
            float anguloActual = rb.rotation;
            float nuevoAngulo = Mathf.MoveTowardsAngle(anguloActual, anguloDeseado, velocidadRotacion * Time.deltaTime);
            rb.MoveRotation(nuevoAngulo);

            // Mover hacia adelante (hacia donde mira)
            rb.linearVelocity = transform.right * velocidad;
        }

        // Destruir después de cierto tiempo
        tiempoVivo += Time.deltaTime;
        if (tiempoVivo >= tiempoMaximoVida)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layerImpacto = collision.gameObject.layer;

        if ((capasBalasJugador.value & (1 << layerImpacto)) != 0)
        {
            vidaMisil--;

            if (!estaDañado)
                StartCoroutine(ParpadearRojo());

            Destroy(collision.gameObject);

            if (vidaMisil <= 0)
                Destroy(gameObject);
        }

        if ((capasJugador.value & (1 << layerImpacto)) != 0)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ParpadearRojo()
    {
        estaDañado = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = colorOriginal;
        }

        estaDañado = false;
    }
}
