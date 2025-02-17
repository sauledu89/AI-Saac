using UnityEngine;

public class EnemigoTorreta : MonoBehaviour
{
    public Transform controladorDisparo;
    public float radioDeteccion = 5f;  // Radio en el que la torreta puede detectar al jugador
    public float anguloVision = 60f;   // Ángulo del cono de visión
    public float velocidadRotacion = 5f; // Velocidad con la que gira la torreta
    public LayerMask capaJugador;

    public GameObject balaEnemigo;
    public float FireRate;
    private float tiempoUltimoDisparo;

    private Transform objetivoJugador; // Guarda la referencia del jugador detectado

    private void Update()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaJugador);

        if (jugadorDetectado)
        {
            objetivoJugador = jugadorDetectado.transform;
            Vector2 direccionJugador = (objetivoJugador.position - transform.position).normalized;
            float angulo = Vector2.Angle(transform.right, direccionJugador);

            if (angulo < anguloVision / 2) // Si está dentro del cono de visión
            {
                // Rotar suavemente hacia el jugador
                RotarHaciaJugador(direccionJugador);

                // Disparar si el cooldown ha pasado
                if (Time.time > FireRate + tiempoUltimoDisparo)
                {
                    tiempoUltimoDisparo = Time.time;
                    Disparar();
                }
            }
        }
        else
        {
            objetivoJugador = null; // Deja de apuntar si el jugador sale del rango
        }
    }

    private void RotarHaciaJugador(Vector2 direccionJugador)
    {
        float anguloObjetivo = Mathf.Atan2(direccionJugador.y, direccionJugador.x) * Mathf.Rad2Deg;
        Quaternion rotacionObjetivo = Quaternion.Euler(0, 0, anguloObjetivo);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
    }

    private void Disparar()
    {
        Instantiate(balaEnemigo, controladorDisparo.position, transform.rotation); // Dispara en la dirección actual
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        Vector2 anguloIzq = Quaternion.Euler(0, 0, -anguloVision / 2) * transform.right;
        Vector2 anguloDer = Quaternion.Euler(0, 0, anguloVision / 2) * transform.right;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + anguloIzq * radioDeteccion);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + anguloDer * radioDeteccion);
    }
}
