using UnityEngine;
using System.Collections;

public class EnemigoTorreta : MonoBehaviour
{
    public Transform controladorDisparo;
    public float radioDeteccion = 5f;
    public float anguloVision = 60f;
    public float velocidadRotacion = 5f;
    public LayerMask capaJugador;

    public GameObject balaEnemigo;
    public float FireRate;
    private float tiempoUltimoDisparo;

    private Transform objetivoJugador;
    private bool persiguiendoJugador = false;
    private Quaternion rotacionInicial;
    public float tiempoParaRegresar = 3f; // Tiempo antes de volver a la posición inicial
    private Coroutine regresarRotacion;

    public SpriteRenderer conoDeVisionRenderer;
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);

    private void Start()
    {
        if (conoDeVisionRenderer == null)
        {
            Debug.LogError("No se asignó el SpriteRenderer del cono de visión en " + gameObject.name);
        }

        // Guardar la rotación inicial de la torreta
        rotacionInicial = transform.rotation;

        // Asegurar que el cono de visión tenga el color normal al inicio
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = colorNormal;
        }
    }

    private void Update()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaJugador);

        if (jugadorDetectado)
        {
            objetivoJugador = jugadorDetectado.transform;
            persiguiendoJugador = true;

            if (regresarRotacion != null)
            {
                StopCoroutine(regresarRotacion); // Cancelar la rotación de regreso si el jugador vuelve
                regresarRotacion = null;
            }

            Vector2 direccionJugador = (objetivoJugador.position - transform.position).normalized;
            float angulo = Vector2.Angle(transform.right, direccionJugador);

            if (angulo < anguloVision / 2) // Si está dentro del cono de visión
            {
                CambiarColorConoVision(colorDetectando);
                RotarHaciaJugador(direccionJugador);

                if (Time.time > FireRate + tiempoUltimoDisparo)
                {
                    tiempoUltimoDisparo = Time.time;
                    Disparar();
                }
            }
        }
        else
        {
            if (persiguiendoJugador) // Si acaba de perder al jugador
            {
                persiguiendoJugador = false;
                regresarRotacion = StartCoroutine(RegresarARotacionInicial());
            }

            CambiarColorConoVision(colorNormal);
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
        Instantiate(balaEnemigo, controladorDisparo.position, transform.rotation);
    }

    private void CambiarColorConoVision(Color nuevoColor)
    {
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = nuevoColor;
        }
    }

    private IEnumerator RegresarARotacionInicial()
    {
        yield return new WaitForSeconds(tiempoParaRegresar);

        // Regresa gradualmente a la rotación original
        while (Quaternion.Angle(transform.rotation, rotacionInicial) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionInicial, velocidadRotacion * Time.deltaTime);
            yield return null;
        }

        transform.rotation = rotacionInicial; // Asegurar que quede perfectamente alineada
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
