using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Escapista : MonoBehaviour
{
    [Header("Vida")]
    public int vidaMaxima = 3;
    public int vidaActual;

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal; //Guardamos el color de dificultad asignado

    [Header("Configuración de Visión")]
    public float radioDeteccion = 5f;
    public float radioVisionLejana = 10f;
    public LayerMask capaJugador;
    public LayerMask capaObstaculos;
    public float velocidadRotacion = 5f;

    [Header("Ícono de detección")]
    public GameObject iconoAlerta;

    private Transform objetivoJugador;
    private NavMeshAgent agente;
    private Coroutine rutinaCansancio;

    public enum EstadoEscapista { Activo, Cansado }
    [SerializeField] private EstadoEscapista estadoActual = EstadoEscapista.Activo;

    [Header("Cansancio")]
    public float tiempoCansancio = 3f;
    public float tiempoRecuperacion = 3f;

    [Header("Disparo")]
    public GameObject balaEnemigo;
    public float fireRateActivo = 1f;
    public float fireRateCansado = 1.5f;
    private float tiempoUltimoDisparo;
    public float errorPunteriaCansado = 10f;

    private Vector3 GizmoPosicionFlee = Vector3.zero;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
        }

        if (iconoAlerta != null)
        {
            iconoAlerta.SetActive(false);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color; //Guardamos color inicial (dificultad)
        }

        vidaActual = vidaMaxima;
    }

    void Update()
    {
        DetectarJugador();

        switch (estadoActual)
        {
            case EstadoEscapista.Activo:
                if (objetivoJugador != null)
                {
                    ComportamientoActivo();
                    Disparar(fireRateActivo, 0f);
                }
                break;

            case EstadoEscapista.Cansado:
                Disparar(fireRateCansado, errorPunteriaCansado);
                break;
        }
    }

    void DetectarJugador()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioVisionLejana, capaJugador);
        if (jugadorDetectado)
        {
            objetivoJugador = jugadorDetectado.transform;

            if (iconoAlerta != null)
                iconoAlerta.SetActive(true);

            Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
            RotarHaciaJugador(direccion);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);
            Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);
        }
        else
        {
            if (iconoAlerta != null)
                iconoAlerta.SetActive(false);

            objetivoJugador = null;
        }
    }

    void ComportamientoActivo()
    {
        if (objetivoJugador == null) return;

        float distanciaJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        if (distanciaJugador <= radioDeteccion)
        {
            HuirDeJugador();
            return;
        }

        if (distanciaJugador <= radioVisionLejana)
        {
            Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                agente.isStopped = true;
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
        }
        else
        {
            Vector3 puntoAlternativo = transform.position + (objetivoJugador.position - transform.position).normalized * radioDeteccion * 2;
            if (NavMesh.SamplePosition(puntoAlternativo, out hit, radioDeteccion * 2, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                GizmoPosicionFlee = hit.position;
            }
            else
            {
                agente.SetDestination(transform.position);
                GizmoPosicionFlee = transform.position;
            }
        }

        if (rutinaCansancio == null)
        {
            rutinaCansancio = StartCoroutine(IniciarCansancio());
        }
    }

    IEnumerator IniciarCansancio()
    {
        yield return new WaitForSeconds(tiempoCansancio);
        estadoActual = EstadoEscapista.Cansado;
        agente.SetDestination(transform.position);
        yield return new WaitForSeconds(tiempoRecuperacion);
        estadoActual = EstadoEscapista.Activo;
        rutinaCansancio = null;
    }

    void RotarHaciaJugador(Vector3 direccion)
    {
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg + 180;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    void RotarHaciaDireccion(Vector2 direccion)
    {
        if (direccion.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            Quaternion rotacionObjetivo = Quaternion.Euler(0, 0, angulo);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime * 200f);
        }
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Matar();
            return;
        }

        StartCoroutine(ParpadeoEntreColorYBlanco());
    }

    private IEnumerator ParpadeoEntreColorYBlanco()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < 2; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = colorOriginal; //Regresa al color original (verde/amarillo/rojo)
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Matar()
    {
        Debug.Log("El escapista ha muerto.");
        Destroy(gameObject);
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