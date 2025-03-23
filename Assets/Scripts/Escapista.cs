using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    [Header("Componentes")]
    public SpriteRenderer conoDeVisionRenderer;                     // Sprite que usamos como cono visual del enemigo
    public Transform conoDeVisionTransform;                         // Transform que rota junto al enemigo
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);         // Color base del cono de visión
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);     // Color cuando detecta al jugador

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

        // Inicializar color del cono de visión
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = colorNormal;
        }
    }

    void Update()
    {
        DetectarJugador(); // El enemigo siempre está "escaneando" al jugador

        switch (estadoActual)
        {
            case EstadoEscapista.Activo:
                if (objetivoJugador != null)
                {
                    ComportamientoActivo();                   // Perseguir o quedarse quieto
                    Disparar(fireRateActivo, 0f);             // Dispara con puntería perfecta
                }
                break;

            case EstadoEscapista.Cansado:
                agente.SetDestination(transform.position);          // No se mueve
                Disparar(fireRateCansado, errorPunteriaCansado);    // Dispara más lento y con menos precisión
                break;
        }
    }
    void DetectarJugador()
    {
        // Usamos un OverlapCircle (círculo de detección) para detectar si hay un jugador dentro del radio de visión lejana
        // Esta función nos devuelve el primer Collider2D que encuentra en esa área (más que suficiente para este enemigo).
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioVisionLejana, capaJugador);

        if (jugadorDetectado)
        {
            // Si el jugador fue detectado, lo guardamos como objetivo
            objetivoJugador = jugadorDetectado.transform;

            // Calculamos qué tan lejos está el jugador
            float distancia = Vector3.Distance(transform.position, objetivoJugador.position);

            // --- Si está muy cerca (dentro del radio de detección corto) ---
            // Solo si está en estado Activo, entonces **huye**
            if (distancia <= radioDeteccion && estadoActual == EstadoEscapista.Activo)
            {
                CambiarColorConoVision(colorDetectando); // Cambiamos el color del cono como retroalimentación visual
                HuirDeJugador();                         // Hacemos que huya, se explicará más abajo
            }
            else
            {
                // --- Si el jugador está lejos (pero en radio de visión) ---
                // Lanzamos un Raycast hacia el jugador para ver si hay obstáculos en medio

                Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, radioVisionLejana, capaJugador | capaObstaculos);

                // Dibujamos el raycast en la escena para debug visual
                Debug.DrawRay(transform.position, direccion * radioVisionLejana, Color.yellow);

                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    // Si el raycast impacta con el jugador (sin obstáculos en medio)

                    agente.isStopped = true;                         // Nos detenemos, ya que el jugador está visible directamente
                    CambiarColorConoVision(colorDetectando);         // Feedback visual (cono en rojo)
                    RotarHaciaDireccion(direccion);                  // Miramos hacia el jugador

                    // Comenzamos el temporizador para cansarnos, solo si estamos en estado Activo
                    if (estadoActual == EstadoEscapista.Activo && rutinaCansancio == null)
                    {
                        rutinaCansancio = StartCoroutine(IniciarCansancio());
                    }
                }
                else
                {
                    // Si el raycast no impactó directamente al jugador (hay obstáculos),
                    // intentamos movernos hacia él (lo perseguimos)

                    agente.isStopped = false;
                    agente.SetDestination(objetivoJugador.position);
                }
            }
        }
        else
        {
            // Si NO detectamos jugador, entonces detenemos el movimiento y regresamos la visión a su color base
            CambiarColorConoVision(colorNormal);
            agente.isStopped = false;
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
            CambiarColorConoVision(colorDetectando);
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
                CambiarColorConoVision(colorDetectando);
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
        if (objetivoJugador == null || agente == null) return;

        // Calculamos la dirección opuesta al jugador
        Vector3 direccionAlejarse = (transform.position - objetivoJugador.position).normalized;
        Vector3 puntoHuida = transform.position + direccionAlejarse * radioDeteccion * 2;

        NavMeshHit hit;

        // Intentamos encontrar un punto válido en el NavMesh en la dirección de huida
        if (NavMesh.SamplePosition(puntoHuida, out hit, radioDeteccion * 2, NavMesh.AllAreas))
        {
            agente.SetDestination(hit.position);
            GizmoPosicionFlee = hit.position;
            Debug.DrawRay(transform.position, (hit.position - transform.position), Color.green, 1f);
        }
        else
        {
            // Si no puede huir hacia atrás, intenta avanzar hacia el jugador como último recurso
            Vector3 puntoAlternativo = transform.position + (objetivoJugador.position - transform.position).normalized * radioDeteccion * 2;
            if (NavMesh.SamplePosition(puntoAlternativo, out hit, radioDeteccion * 2, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
                GizmoPosicionFlee = hit.position;
                Debug.DrawRay(transform.position, (hit.position - transform.position), Color.yellow, 1f);
            }
            else
            {
                // Si tampoco hay punto válido, se queda quieto
                agente.SetDestination(transform.position);
                GizmoPosicionFlee = transform.position;
                Debug.DrawRay(transform.position, Vector3.up * 2, Color.red, 1f);
            }
        }

        // Iniciar el ciclo de cansancio si aún no ha empezado
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

    void RotarHaciaDireccion(Vector2 direccion)
    {
        // Verificamos que la dirección tenga magnitud suficiente para rotar (no rotamos si es un vector nulo o casi cero)
        if (direccion.sqrMagnitude > 0.01f)
        {
            // Convertimos la dirección en un ángulo en grados (usamos Atan2 que devuelve el ángulo en radianes)
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

            // Creamos una rotación en el eje Z, ya que estamos en un entorno 2D
            Quaternion rotacion = Quaternion.Euler(0, 0, angulo);

            // Aplicamos la rotación gradualmente al enemigo (RotateTowards evita que gire de golpe)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacion,
                velocidadRotacion * Time.deltaTime * 200f // Controlamos qué tan rápido rota
            );
        }
    }
    void CambiarColorConoVision(Color nuevoColor)
    {
        // Cambiamos el color del SpriteRenderer que representa el cono de visión.
        // Esto es útil como retroalimentación visual para saber si el enemigo está detectando al jugador.
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = nuevoColor;
        }
    }
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
