using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class VisionCone : MonoBehaviour
{

    // Este fue el primer enemigo que funciona con NavMesh, este enemigo patrulla y persigue al jugador al detectarlo,
    // Si lo vuelve a perder, regresa a su patrullaje en la secuencia en la que se quedó. 

    // Propiedades de detección del cono de visión
    [Header("Configuración de Visión")]
    public float radioDeteccion = 5f;
    public float anguloVision = 60f;
    public LayerMask capaJugador;
    public float velocidadRotacion = 5f;   // Velocidad con la que gira hacia su objetivo

    [Header("Comportamiento")]
    public float tiempoPersecucion = 3f;   // Tiempo que persigue al jugador antes de detenerse
    public float tiempoParaRegresar = 2f;  // Tiempo antes de volver a patrullar
    public bool regresarAPatrulla = true;  // Define el estado de regresando a patrullar

    [Header("Patrullaje")]
    public GameObject[] waypoints;                // Se definen "n" waypoints de patrullaje como en el 1er parcial, que después se asignarán en el editor
    private int waypointActual = 0; 
    public float radioAceptacionWaypoint = 3f;    // Distancia en la que el enemigo considera que "llegó" a un waypoint
    private bool enModoPatrulla = true;           // Define el estado de patrullaje

    // Para añadir el sprite del vision cone, que ya es un GameObject en escena 
    [Header("Componentes")]
    public SpriteRenderer conoDeVisionRenderer;      // Para añadir el sprite del vision cone, que ya es un GameObject en escena 
    public Transform conoDeVisionTransform;          // Referencia al objeto del sprite del cono
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);

    private Transform objetivoJugador;
    private NavMeshAgent agente;
    private bool persiguiendoJugador = false;        // Por defecto el enemigo no se encuentra persiguiendo pues aún no lo ha visto
    private Vector3 posicionInicial;
    private Coroutine regresarRutina;

    void Start()
    {
        // Obtener referencia al NavMeshAgent y guardar la posición inicial
        agente = GetComponent<NavMeshAgent>();
        posicionInicial = transform.position;

        if (conoDeVisionRenderer != null)
        {
            // Configurar el color inicial del cono de visión
            conoDeVisionRenderer.color = colorNormal;
        }

        if (agente != null)
        {
            // Deshabilitar rotación automática del NavMeshAgent (para controlar la rotación manualmente)
            agente.updateRotation = false;
            agente.updateUpAxis = false;
        }
            // Si hay waypoints disponibles...
        if (waypoints.Length > 0)
        {
            IniciarPatrullaje();
        }
    }

    void Update()
    {
        // Comprobar si el jugador está dentro del cono de visión
        DetectarJugador();

        if (enModoPatrulla && !persiguiendoJugador)
        {
            Patrullar();
            RotarHaciaDireccion(agente.velocity.normalized);
        }

        // Si no está persiguiendo al jugador, rota según la dirección de movimiento
        if (!persiguiendoJugador)
        {
            Vector2 direccionMovimiento = agente.velocity.normalized;
            if (direccionMovimiento.sqrMagnitude > 0.01f)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }
    }

    // Detecta si el jugador entra en el radio de detección y el cono de visión.
    // Si el jugador es detectado, el enemigo comienza a perseguirlo.
    void DetectarJugador()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaJugador);

        if (jugadorDetectado)
        {
            Vector2 direccionJugador = (jugadorDetectado.transform.position - transform.position).normalized;
            float angulo = Vector2.Angle(transform.right, direccionJugador);

            if (angulo < anguloVision / 2)         // Si el jugador está dentro del cono de visión
            {
                objetivoJugador = jugadorDetectado.transform;
                persiguiendoJugador = true;
                enModoPatrulla = false;
                CambiarColorConoVision(colorDetectando);

                if (regresarRutina != null)
                {
                    StopCoroutine(regresarRutina);
                    regresarRutina = null;
                }

                // Hace que el enemigo persiga al jugador durante el tiempo definido.
              
                StartCoroutine(PerseguirObjetivo());

                // Rota el cono de visión hacia el jugador
                RotarHaciaDireccion(direccionJugador);
            }
            else
            {
                if (persiguiendoJugador)

                {  ///Luego regresa a su patrullaje si la opción está habilitada.
                    persiguiendoJugador = false;
                    regresarRutina = StartCoroutine(RegresarAPatrulla());
                }
                CambiarColorConoVision(colorNormal);
            }
        }
        else
        {
            if (persiguiendoJugador)
            {
                persiguiendoJugador = false;
                regresarRutina = StartCoroutine(RegresarAPatrulla());
            }
            CambiarColorConoVision(colorNormal);
        }
    }

    /*
      PerseguirObjetivo() y RegresarAPatrulla() son corrutinas (IEnumerator), 
      lo que significa que se ejecutan de forma asíncrona en Unity, 
      permitiendo pausas (yield return) sin bloquear el resto del código.
    */
    IEnumerator PerseguirObjetivo()
    {
        float tiempoFinal = Time.time + tiempoPersecucion;

        while (Time.time < tiempoFinal && objetivoJugador != null)
        {
            if (agente != null)
            {
                agente.SetDestination(objetivoJugador.position);
                RotarHaciaDireccion(objetivoJugador.position - transform.position);
            }
            //yield return null; hace que se espere hasta el siguiente frame antes de continuar con la persecución.
            yield return null;
        }

        if (regresarAPatrulla)
        {
            regresarRutina = StartCoroutine(RegresarAPatrulla());
        }
    }

    IEnumerator RegresarAPatrulla()
    {
        yield return new WaitForSeconds(tiempoParaRegresar);
        enModoPatrulla = true;
        persiguiendoJugador = false;
        if (agente != null && waypoints.Length > 0)
        {
            agente.SetDestination(waypoints[waypointActual].transform.position);
        }
    }

    void IniciarPatrullaje()
    {
        enModoPatrulla = true;
        if (agente != null && waypoints.Length > 0)
        {
            // Se establece el destino en waypoints desde el actual, por si pierde al jugador en media ruta 
            agente.SetDestination(waypoints[waypointActual].transform.position);
        }
    }

    void Patrullar()
    {
        // Si existen waypoints
        if (agente != null && waypoints.Length > 0)
        { 
            // Se calcula la distancia a éstos.
            float distanciaAlWaypoint = Vector3.Distance(transform.position, waypoints[waypointActual].transform.position);
            // Si la distancia hacia el waypoint es menor que el umbral de detección del waypoint
            if (distanciaAlWaypoint <= radioAceptacionWaypoint)
            {
                // Se mueve al siguiente waypoint en lista 
                waypointActual = (waypointActual + 1) % waypoints.Length;
            }

            agente.SetDestination(waypoints[waypointActual].transform.position);
        }
    }

    private void CambiarColorConoVision(Color nuevoColor)
    {
        if (conoDeVisionRenderer != null)
        {
            conoDeVisionRenderer.color = nuevoColor;
        }
    }

    private void RotarHaciaDireccion(Vector2 direccion)
    {
        if (direccion.sqrMagnitude > 0.01f) // Solo rota si hay movimiento
        {
            float anguloObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            Quaternion rotacionObjetivo = Quaternion.Euler(0, 0, anguloObjetivo);

            // Rotar el enemigo (si quieres que el sprite también rote)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime * 200f);

            // Rotar el cono de visión (si tiene un objeto hijo asignado)
            if (conoDeVisionTransform != null)
            {
                conoDeVisionTransform.rotation = Quaternion.RotateTowards(conoDeVisionTransform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime * 200f);
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;                                    
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);                                     // Gizmo de la distancia en la que se puede detectar al jugador.

        Vector2 anguloIzq = Quaternion.Euler(0, 0, -anguloVision / 2) * transform.right;               // Creación del gizmo del cono, dentro del radio de visión máximo
        Vector2 anguloDer = Quaternion.Euler(0, 0, anguloVision / 2) * transform.right;                // Con el ángulo personalizado. 

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + anguloIzq * radioDeteccion);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + anguloDer * radioDeteccion);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * radioDeteccion);


        if (waypoints.Length > 0)
        {
            // Color y dibujado de la ruta entre waypoints
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i].transform.position, 0.2f);
                if (i < waypoints.Length - 1)
                {
                    Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                }
            }
        }
    }
}
