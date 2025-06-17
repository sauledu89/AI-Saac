using UnityEngine;
using UnityEngine.AI;
using System.Collections;
/// <summary>
/// Este enemigo se comporta de forma evasiva (huye del jugador si está cerca),
/// puede disparar proyectiles, y entra en un estado de cansancio si huye por mucho tiempo.
/// comportamiento reactivo con estados (Activo/Cansado), navegación con NavMesh,
/// y generación de características con dificultad/balance calculado mediante Greedy Search.
/// El color del enemigo representa su nivel de vida inicial (cyan, naranja o rojo),
/// </summary>
public class Escapista : BaseEnemy
{

    private Color colorOriginal;

    [Header("Configuración de Visión")]
    public float radioDeteccion = 5f;
    public float radioVisionLejana = 10f;
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
    public float errorPunteriaCansado = 10f;
    private float tiempoUltimoDisparo;

    private Vector3 GizmoPosicionFlee = Vector3.zero;

    private PCGEnemyStats stats;
    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetStats(PCGEnemyStats newStats)
    {
        // Asignamos estadísticas generadas por IA (HP, velocidad, daño, etc.)
        stats = newStats;

        vidaMaxima = Mathf.RoundToInt(stats.HP);
        vidaActual = vidaMaxima;

        if (agente != null)
            agente.speed = stats.MovementSpeed;

        // Aplicamos color visual según la vida inicial del enemigo
        Color color = ObtenerColorPorHP(vidaMaxima);
        EstablecerColorPorRonda(color);
    }


    protected override void Start()
    {
        base.Start();

        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.updateRotation = false;
            agente.updateUpAxis = false;
        }

        if (iconoAlerta != null)
            iconoAlerta.SetActive(false);

       // spriteRenderer = GetComponent<SpriteRenderer>();
       // if (spriteRenderer != null)
       //     colorOriginal = spriteRenderer.color;

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
                    Disparar(0f); // sin error
                }
                break;

            case EstadoEscapista.Cansado:
                Disparar(errorPunteriaCansado); // con error
                break;
        }
    }

    private Color ObtenerColorPorHP(int hp)
    {
        if (hp <= 2)
            return Color.cyan;
        else if (hp <= 4)
            return new Color(1f, 0.5f, 0f); // Naranja
        else
            return Color.red;
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

    void Disparar(float errorPunteria)
    {
        if (stats == null || objetivoJugador == null) return;

        float fireRate = stats.AttackRate;

        if (Time.time > tiempoUltimoDisparo + fireRate)
        {
            tiempoUltimoDisparo = Time.time;

            Vector2 direccionJugador = (objetivoJugador.position - transform.position).normalized;
            float anguloError = Random.Range(-errorPunteria, errorPunteria);
            float angulo = Mathf.Atan2(direccionJugador.y, direccionJugador.x) * Mathf.Rad2Deg + anguloError;

            Quaternion rotacionBala = Quaternion.Euler(0, 0, angulo);
            GameObject bala = Instantiate(balaEnemigo, transform.position, rotacionBala);

            if (bala.TryGetComponent(out EnemyBullet balaScript))

            {
                balaScript.SetDamage(Mathf.RoundToInt(stats.Damage));
            }
        }
    }

    void HuirDeJugador()
    {
        if (objetivoJugador == null || agente == null) return;

        Vector3 direccionAlejarse = (transform.position - objetivoJugador.position).normalized;
        Vector3 puntoHuida = transform.position + direccionAlejarse * radioDeteccion * 2;

        if (NavMesh.SamplePosition(puntoHuida, out NavMeshHit hit, radioDeteccion * 2, NavMesh.AllAreas))
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
            rutinaCansancio = StartCoroutine(IniciarCansancio());
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
            spriteRenderer.color = colorOriginal;
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

    public PCGEnemyStats GetStats()
    {
        return stats;
    }

}
