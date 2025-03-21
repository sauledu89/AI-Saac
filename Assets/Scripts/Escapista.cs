using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Escapista : MonoBehaviour
{
    [Header("Configuración de Visión")]
    public float radioDeteccion = 5f;
    public float radioVisionLejana = 10f;
    public LayerMask capaJugador;
    public LayerMask capaObstaculos;
    public float velocidadRotacion = 5f;

    [Header("Componentes")]
    public SpriteRenderer conoDeVisionRenderer;
    public Transform conoDeVisionTransform;
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);

    private Transform objetivoJugador;
    private NavMeshAgent agente;
    private Coroutine rutinaCansancio;

    public enum EstadoEscapista { Activo, Cansado }
    [SerializeField] private EstadoEscapista estadoActual = EstadoEscapista.Activo;

    [Header("Cansancio")]
    public float tiempoCansancio = 3f;    // Tiempo para entrar en estado Cansado
    public float tiempoRecuperacion = 3f; // Tiempo que dura el estado Cansado antes de volver a Activo

    [Header("Disparo")]
    public GameObject balaEnemigo;
    public float fireRateActivo = 1f;    // Fire rate en estado Activo
    public float fireRateCansado = 1.5f; // Fire rate en estado Cansado
    private float tiempoUltimoDisparo;
    public float errorPunteriaCansado = 10f; // Margen de error en estado Cansado


    private Vector3 GizmoPosicionFlee = Vector3.zero;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
        }

        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = colorNormal;
        }
    }

    void Update()
    {
        DetectarJugador(); // SIEMPRE intentamos detectar al jugador, incluso si está cansado

        switch (estadoActual)
        {
            case EstadoEscapista.Activo:
                if (objetivoJugador != null)
                {
                    ComportamientoActivo();
                    Disparar(fireRateActivo, 0f); // Sin error en puntería
                }
                break;

            case EstadoEscapista.Cansado:
                agente.SetDestination(transform.position); // Asegurar que no se mueva
                Disparar(fireRateCansado, errorPunteriaCansado); // Dispara más lento y con menos precisión
                break;
        }
    }

    void DetectarJugador()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioVisionLejana, capaJugador);

        if (jugadorDetectado)
        {
            objetivoJugador = jugadorDetectado.transform;
            float distancia = Vector3.Distance(transform.position, objetivoJugador.position);

            if (distancia <= radioDeteccion)
            {
                if (estadoActual == EstadoEscapista.Activo)
                {
                    CambiarColorConoVision(colorDetectando);
                    HuirDeJugador();
                }
            }
            else
            {
                Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);

                Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);

                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    agente.isStopped = true;
                    CambiarColorConoVision(colorDetectando);
                    RotarHaciaDireccion(direccion);

                    if (estadoActual == EstadoEscapista.Activo && rutinaCansancio == null)
                    {
                        rutinaCansancio = StartCoroutine(IniciarCansancio());
                    }
                }
                else
                {
                    agente.isStopped = false;
                    agente.SetDestination(objetivoJugador.position);
                }
            }
        }
        else
        {
            CambiarColorConoVision(colorNormal);
            agente.isStopped = false;
        }
    }

    void ComportamientoActivo()
    {
        if (objetivoJugador == null) return;

        float distanciaJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        //Si el jugador entra al radio de detección cercano, HUIR inmediatamente**
        if (distanciaJugador <= radioDeteccion)
        {
            Debug.Log("Jugador en radio cercano, HUYENDO.");
            CambiarColorConoVision(colorDetectando);
            HuirDeJugador();
            return;
        }

        //Si está en radio lejano, aplicar lógica de detección**
        if (distanciaJugador <= radioVisionLejana)
        {
            Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);

            Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                agente.isStopped = true;
                CambiarColorConoVision(colorDetectando);
                RotarHaciaDireccion(direccion);
            }
            else
            {
                agente.isStopped = false;
                agente.SetDestination(objetivoJugador.position);
            }
        }
    }

    void Disparar(float fireRate, float errorPunteria)
    {
        if (Time.time > fireRate + tiempoUltimoDisparo && objetivoJugador != null)
        {
            tiempoUltimoDisparo = Time.time;

            Vector2 direccionJugador = (objetivoJugador.position - transform.position).normalized;

            // Si está cansado, agregar un error aleatorio al ángulo de disparo
            float anguloError = Random.Range(-errorPunteria, errorPunteria);
            float angulo = Mathf.Atan2(direccionJugador.y, direccionJugador.x) * Mathf.Rad2Deg + anguloError;
            Quaternion rotacionBala = Quaternion.Euler(0, 0, angulo);

            Instantiate(balaEnemigo, transform.position, rotacionBala);
        }
    }


    void HuirDeJugador()
    {
        if (objetivoJugador == null || agente == null) return;

        Vector3 direccionAlejarse = (transform.position - objetivoJugador.position).normalized;
        Vector3 puntoHuida = transform.position + direccionAlejarse * radioDeteccion * 2;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(puntoHuida, out hit, radioDeteccion * 2, NavMesh.AllAreas))
        {
            agente.SetDestination(hit.position);
            GizmoPosicionFlee = hit.position;
            Debug.DrawRay(transform.position, (hit.position - transform.position), Color.green, 1f);
        }
        else
        {
            Vector3 puntoAlternativo = transform.position + (objetivoJugador.position - transform.position).normalized * radioDeteccion * 2;
            if (NavMesh.SamplePosition(puntoAlternativo, out hit, radioDeteccion * 2, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                GizmoPosicionFlee = hit.position;
                Debug.DrawRay(transform.position, (hit.position - transform.position), Color.yellow, 1f);
            }
            else
            {
                agente.SetDestination(transform.position);
                GizmoPosicionFlee = transform.position;
                Debug.DrawRay(transform.position, Vector3.up * 2, Color.red, 1f);
            }
        }

        if (rutinaCansancio == null)
        {
            rutinaCansancio = StartCoroutine(IniciarCansancio());
        }
    }

    IEnumerator IniciarCansancio()
    {
        Debug.Log("Enemigo está entrando en estado CANSADO.");
        yield return new WaitForSeconds(tiempoCansancio);

        estadoActual = EstadoEscapista.Cansado;
        agente.SetDestination(transform.position);
        Debug.Log("Enemigo está ahora CANSADO.");

        yield return new WaitForSeconds(tiempoRecuperacion);

        Debug.Log("Enemigo se ha RECUPERADO y vuelve a estado ACTIVO.");
        estadoActual = EstadoEscapista.Activo;
        rutinaCansancio = null;
    }

    void RotarHaciaDireccion(Vector2 direccion)
    {
        if (direccion.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            Quaternion rotacion = Quaternion.Euler(0, 0, angulo);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacion, velocidadRotacion * Time.deltaTime * 200f);
        }
    }

    void CambiarColorConoVision(Color nuevoColor)
    {
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = nuevoColor;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radioVisionLejana);

        if (GizmoPosicionFlee != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(GizmoPosicionFlee, 1f);
        }
    }
}
