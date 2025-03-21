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
    private Quaternion ultimaRotacion;

    public float tiempoParaRegresar = 3f;
    public float tiempoDisparandoExtra = 2f;
    private Coroutine regresarRotacion;

    public SpriteRenderer conoDeVisionRenderer;
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);

  
    public bool disparandoTrasPerderJugador = false;

    private void Start()
    {
        if (conoDeVisionRenderer == null)
        {
            Debug.LogError("No se asignó el SpriteRenderer del cono de visión en " + gameObject.name);
        }

        rotacionInicial = transform.rotation;

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
            Vector2 direccionJugador = (jugadorDetectado.transform.position - transform.position).normalized;
            float angulo = Vector2.Angle(transform.right, direccionJugador);

            // Si el jugador está dentro del cono de visión
            if (angulo < anguloVision / 2)
            {
                objetivoJugador = jugadorDetectado.transform;
                persiguiendoJugador = true;
                disparandoTrasPerderJugador = false;

                if (regresarRotacion != null)
                {
                    StopCoroutine(regresarRotacion);
                    regresarRotacion = null;
                }

                CambiarColorConoVision(colorDetectando);
                RotarHaciaJugador(direccionJugador);

                ultimaRotacion = transform.rotation;

                if (Time.time > FireRate + tiempoUltimoDisparo)
                {
                    tiempoUltimoDisparo = Time.time;
                    Disparar();
                }
            }
            else // El jugador está en el radio, pero fuera del cono de visión
            {
                if (persiguiendoJugador)
                {
                    persiguiendoJugador = false;
                    disparandoTrasPerderJugador = true;
                    StartCoroutine(DispararTrasPerderJugador());
                }

                // Cambia el color a blanco ya que el jugador no está en el cono
                CambiarColorConoVision(colorNormal);
            }
        }
        else
        {
            if (persiguiendoJugador)
            {
                persiguiendoJugador = false;
                disparandoTrasPerderJugador = true;
                StartCoroutine(DispararTrasPerderJugador());
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

    private IEnumerator DispararTrasPerderJugador()
    {
        Quaternion direccionFinal = ultimaRotacion;
        float tiempoFinalDisparo = Time.time + tiempoDisparandoExtra;

        while (Time.time < tiempoFinalDisparo)
        {
            transform.rotation = direccionFinal;

            if (Time.time > FireRate + tiempoUltimoDisparo)
            {
                tiempoUltimoDisparo = Time.time;
                Disparar();
            }

            yield return null;
        }

        regresarRotacion = StartCoroutine(RegresarARotacionInicial());
    }

    private IEnumerator RegresarARotacionInicial()
    {
        yield return new WaitForSeconds(tiempoParaRegresar);

        while (Quaternion.Angle(transform.rotation, rotacionInicial) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionInicial, velocidadRotacion * Time.deltaTime);
            yield return null;
        }

        transform.rotation = rotacionInicial;
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
