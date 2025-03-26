using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

/*
   Este enemigo Escapista combina dos comportamientos clave:
   - En estado Activo: puede perseguir, quedarse quieto si ve al jugador, y disparar.
   - En estado Cansado: no se mueve, pero sigue disparando, aunque con menos precisión y menor frecuencia.
   Además, el sistema se basa en NavMesh para navegación y coroutines para gestionar los temporizadores de estados.
*/

public class Escapista : MonoBehaviour
{
    [Header("Configuración de Visión")]
    public float radioDeteccion = 5f;         // Rango en el que huye del jugador
    public float radioVisionLejana = 10f;     // Rango en el que puede detectar al jugador (para dispararle o iniciar huida)
    public LayerMask capaJugador;             // Capa donde está el jugador
    public LayerMask capaObstaculos;          // Capa que bloquea la línea de visión
    public float velocidadRotacion = 5f;      // Qué tan rápido rota para mirar al jugador

    [Header("Ícono de detección")]
    public GameObject iconoAlerta; // Signo de admiración que se activa al detectar al jugador

  /*
    [Header("Componentes")]
    public SpriteRenderer conoDeVisionRenderer;                     // Sprite que usamos como cono visual del enemigo
    public Transform conoDeVisionTransform;                         // Transform que rota junto al enemigo
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);         // Color base del cono de visión
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);     // Color cuando detecta al jugador
  */
    private Transform objetivoJugador;         // Referencia al jugador detectado
    private NavMeshAgent agente;               // Agente de navegación que moverá al enemigo
    private Coroutine rutinaCansancio;         // Coroutine para manejar el cambio de estados

    public enum EstadoEscapista { Activo, Cansado }
    [SerializeField] private EstadoEscapista estadoActual = EstadoEscapista.Activo; // Estado actual del enemigo

    [Header("Cansancio")]
    public float tiempoCansancio = 3f;         // Cuánto tiempo tarda en cansarse después de huir
    public float tiempoRecuperacion = 3f;      // Cuánto tiempo dura el estado Cansado antes de recuperarse

    [Header("Disparo")]
    public GameObject balaEnemigo;             // Prefab de la bala
    public float fireRateActivo = 1f;          // Velocidad de disparo normal
    public float fireRateCansado = 1.5f;       // Velocidad de disparo más lenta cuando está cansado
    private float tiempoUltimoDisparo;         // Controla el cooldown del disparo
    public float errorPunteriaCansado = 10f;   // Error aleatorio que se aplica al disparar estando cansado

    private Vector3 GizmoPosicionFlee = Vector3.zero;  // Punto al que el enemigo huye (visualizado en Gizmos)

    void Start()
    {
        // Obtener y configurar el agente de navegación
        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.updateRotation = false; // Lo rotamos manualmente
            agente.updateUpAxis = false;
        }

        /* Inicializar color del cono de visión
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = colorNormal;
        }
        */

        // Desactivar el ícono de alerta al inicio
        if (iconoAlerta != null)
        {
            iconoAlerta.SetActive(false);
        }
    }

    void Update()
    {
        DetectarJugador(); // Siempre intenta detectar al jugador y rota si es necesario

        switch (estadoActual)
        {
            case EstadoEscapista.Activo:
                if (objetivoJugador != null)
                {
                    ComportamientoActivo(); // Perseguir o quedarse quieto según la situación
                    Disparar(fireRateActivo, 0f); // Dispara con puntería perfecta
                }
                break;

            case EstadoEscapista.Cansado:
                // En el estado cansado, el enemigo no se mueve pero puede seguir disparando
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

            if (iconoAlerta != null)
            {
                iconoAlerta.SetActive(true); // Mostrar ícono
            }

            Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
            RotarHaciaJugador(direccion);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);
            Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);
        }
        else
        {
            if (iconoAlerta != null)
            {
                iconoAlerta.SetActive(false); // Ocultar ícono
            }

            objetivoJugador = null;
        }
    }

    void ComportamientoActivo()
    {
        if (objetivoJugador == null) return;

        float distanciaJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        // --- Si el jugador se acerca demasiado ---
        // Aunque estemos ya en estado activo, si entra al radio de detección corto, el enemigo huye
        if (distanciaJugador <= radioDeteccion)
        {
            Debug.Log("Jugador en radio cercano, HUYENDO.");
         // CambiarColorConoVision(colorDetectando);
            HuirDeJugador(); // Reutilizamos esta lógica
            return;          // No seguimos con el resto del comportamiento
        }

        // --- Si está en rango medio/largo ---
        if (distanciaJugador <= radioVisionLejana)
        {
            Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);

            Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                // Si tenemos visión directa al jugador, dejamos de movernos y giramos a mirarlo
                agente.isStopped = true;
              //CambiarColorConoVision(colorDetectando);
                RotarHaciaDireccion(direccion);
            }
            else
            {
                // Si el jugador está lejos pero no visible, intentamos acercarnos
                agente.isStopped = false;
                agente.SetDestination(objetivoJugador.position);
            }
        }
    }
    void Disparar(float fireRate, float errorPunteria)
    {
        // Esta función se llama constantemente desde Update, pero solo dispara si ha pasado el tiempo suficiente
        if (Time.time > fireRate + tiempoUltimoDisparo && objetivoJugador != null)
        {
            tiempoUltimoDisparo = Time.time;

            // Calculamos dirección al jugador
            Vector2 direccionJugador = (objetivoJugador.position - transform.position).normalized;

            // Si el enemigo está cansado, le agregamos un margen de error a su ángulo
            float anguloError = Random.Range(-errorPunteria, errorPunteria);
            float angulo = Mathf.Atan2(direccionJugador.y, direccionJugador.x) * Mathf.Rad2Deg + anguloError;

            Quaternion rotacionBala = Quaternion.Euler(0, 0, angulo);

            // Instanciamos la bala apuntando en esa dirección (con error si es necesario)
            Instantiate(balaEnemigo, transform.position, rotacionBala);
        }
    }
    void HuirDeJugador()
    {
        // Primero, verifica si alguna referencia necesaria es nula. Si el 'objetivoJugador' o el 'agente' (NavMeshAgent)
        // son nulos, la función termina inmediatamente, ya que no puede ejecutar la lógica de huida sin ellos.
        if (objetivoJugador == null || agente == null) return;

        // Calcula la dirección para huir del jugador, obteniendo un vector que apunta en dirección opuesta al jugador.
        // 'normalized' asegura que el vector tenga una longitud de 1, manteniendo la dirección pero ignorando la distancia.
        Vector3 direccionAlejarse = (transform.position - objetivoJugador.position).normalized;

        // Calcula un punto de destino donde el enemigo intentará huir. Este punto está a dos veces el 'radioDeteccion' 
        // del enemigo en la dirección opuesta al jugador.
        Vector3 puntoHuida = transform.position + direccionAlejarse * radioDeteccion * 2;

        // Estructura utilizada para almacenar información sobre los hits del NavMesh.
        NavMeshHit hit;

        // Intenta encontrar un punto en el NavMesh que esté cerca del punto de huida calculado.
        // 'SamplePosition' busca un punto en el NavMesh cerca de 'puntoHuida' dentro de una esfera de radio especificado.
        // Si encuentra un punto válido, 'hit.position' contendrá la ubicación exacta.
        if (NavMesh.SamplePosition(puntoHuida, out hit, radioDeteccion * 2, NavMesh.AllAreas))
        {
            // Si encuentra un lugar válido para huir, establece ese punto como el destino del agente de NavMesh.
            agente.SetDestination(hit.position);

            // Guarda este punto en 'GizmoPosicionFlee' para propósitos de visualización en el editor.
            GizmoPosicionFlee = hit.position;

            // Dibuja un rayo verde desde la posición actual hacia el punto de huida para visualizar la dirección de huida.
            Debug.DrawRay(transform.position, (hit.position - transform.position), Color.green, 1f);
        }
        else
        {
            // Si no encuentra un punto de huida válido en la dirección opuesta, intenta huir hacia el jugador como último recurso.
            Vector3 puntoAlternativo = transform.position + (objetivoJugador.position - transform.position).normalized * radioDeteccion * 2;
            if (NavMesh.SamplePosition(puntoAlternativo, out hit, radioDeteccion * 2, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                GizmoPosicionFlee = hit.position;
                Debug.DrawRay(transform.position, (hit.position - transform.position), Color.yellow, 1f);
            }
            else
            {
                // Si no hay un punto válido hacia el jugador tampoco, el enemigo se queda quieto.
                agente.SetDestination(transform.position);
                GizmoPosicionFlee = transform.position;
                Debug.DrawRay(transform.position, Vector3.up * 2, Color.red, 1f);
            }
        }

        // Si la rutina de cansancio no ha sido iniciada, empieza una nueva corrutina llamando a 'IniciarCansancio'.
        // Esto controla el cambio de estado del enemigo de activo a cansado después de huir.
        if (rutinaCansancio == null)
        {
            rutinaCansancio = StartCoroutine(IniciarCansancio());
        }
    }


    /*
    ¿Qué es un IEnumerator y para qué lo usamos?

    Nos permite crear funciones que se ejecutan  de manera pausada a lo largo del tiempo.
    En Unity, lo usamos junto con 'StartCoroutine()'  para esperar ciertos segundos sin detener el juego entero.

    Muy útil cuando queremos que algo ocurra después de un tiempo, como hacer que el enemigo 
    se canse durante 3 segundos y luego se recupere automáticamente sin usar Update() todo el tiempo para contar tiempo.

    Ejemplo típico:
    - yield return new WaitForSeconds(3f); ? Pausa la función 3 segundos antes de continuar.
*/

    // Esta función representa una **corrutina** (IEnumerator), que nos permite pausar y continuar
    // su ejecución a lo largo del tiempo, sin bloquear el resto del juego.
    // La usamos para controlar el flujo del enemigo cuando se cansa y luego se recupera.
    IEnumerator IniciarCansancio()
    {
        /*
            Primera pausa:
            Aquí esperamos el tiempo de "tiempoCansancio" antes de cambiar al estado CANSADO.
            Durante ese tiempo, el enemigo podría estar huyendo o haciendo alguna otra acción
            previa a entrar en el estado de cansancio real.
        */
        Debug.Log("Enemigo está entrando en estado CANSADO.");
        yield return new WaitForSeconds(tiempoCansancio);

        // Entramos oficialmente al estado Cansado
        estadoActual = EstadoEscapista.Cansado;

        // Nos aseguramos de detener completamente al enemigo
        agente.SetDestination(transform.position);
        Debug.Log("Enemigo está ahora CANSADO.");

        /*
            Segunda pausa:
            Aquí esperamos el tiempo de "tiempoRecuperacion" mientras el enemigo se mantiene
            cansado. No se mueve, pero puede disparar (con menos precisión).
        */
        yield return new WaitForSeconds(tiempoRecuperacion);

        // Recuperamos al enemigo: vuelve al estado Activo
        Debug.Log("Enemigo se ha RECUPERADO y vuelve a estado ACTIVO.");
        estadoActual = EstadoEscapista.Activo;

        // Importante: limpiamos la referencia de la rutina para evitar duplicaciones futuras
        rutinaCansancio = null;
    }

    void RotarHaciaJugador(Vector3 direccion)
    {
        // Convertimos la dirección a un ángulo en grados
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg +180;  // Ajuste de n grados

        // Aplicamos la rotación al transform del enemigo
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    void RotarHaciaDireccion(Vector2 direccion)
    {
        if (direccion.sqrMagnitude > 0.01f)
        {
            // Calcula el ángulo y conviértelo a grados
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

            // Crea la rotación objetivo
            Quaternion rotacionObjetivo = Quaternion.Euler(0, 0, angulo);

            // Interpola hacia la rotación objetivo
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime * 200f);
        }
    }

    /*
    void CambiarColorConoVision(Color nuevoColor)
    {
        // Cambiamos el color del SpriteRenderer que representa el cono de visión.
        // Esto es útil como retroalimentación visual para saber si el enemigo está detectando al jugador.
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = nuevoColor;
        }
    }

    */
    private void OnDrawGizmos()
    {
        // Esto nos permite ver en la ventana de Scene los radios de detección sin necesidad de ejecutar el juego

        // --- Dibuja el radio de detección corto (en rojo) ---
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        // --- Dibuja el radio de visión lejana (en azul) ---
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radioVisionLejana);

        // --- Dibuja la posición a la que huye el enemigo (en amarillo) ---
        // Esto se guarda en la variable GizmoPosicionFlee al huir
        if (GizmoPosicionFlee != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(GizmoPosicionFlee, 1f); // Dibuja una esfera donde el enemigo planea escapar
        }
    }

}
