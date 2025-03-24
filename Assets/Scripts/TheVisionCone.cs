using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class VisionCone : MonoBehaviour
{

    // Este fue el primer enemigo que funciona con NavMesh, este enemigo patrulla y persigue al jugador al detectarlo,
    // Si lo vuelve a perder, regresa a su patrullaje en la secuencia en la que se qued�. 

    // Propiedades de detecci�n del cono de visi�n
    [Header("Configuraci�n de Visi�n")]
    public float radioDeteccion = 5f;
    public float anguloVision = 60f;
    public LayerMask capaJugador;
    public float velocidadRotacion = 5f;   // Velocidad con la que gira hacia su objetivo

    [Header("Comportamiento")]
    public float tiempoPersecucion = 3f;   // Tiempo que persigue al jugador antes de detenerse
    public float tiempoParaRegresar = 2f;  // Tiempo antes de volver a patrullar
    public bool regresarAPatrulla = true;  // Define el estado de regresando a patrullar

    [Header("Patrullaje")]
    public GameObject[] waypoints;                // Se definen "n" waypoints de patrullaje como en el 1er parcial, que despu�s se asignar�n en el editor
    private int waypointActual = 0; 
    public float radioAceptacionWaypoint = 3f;    // Distancia en la que el enemigo considera que "lleg�" a un waypoint
    private bool enModoPatrulla = true;           // Define el estado de patrullaje

    // Para a�adir el sprite del vision cone, que ya es un GameObject en escena 
    [Header("Componentes")]
    public SpriteRenderer conoDeVisionRenderer;      // Para a�adir el sprite del vision cone, que ya es un GameObject en escena 
    public Transform conoDeVisionTransform;          // Referencia al objeto del sprite del cono
    public Color colorNormal = new Color(1f, 1f, 1f, 0.5f);
    public Color colorDetectando = new Color(1f, 0f, 0f, 0.5f);

    private Transform objetivoJugador;
    private NavMeshAgent agente;
    private bool persiguiendoJugador = false;        // Por defecto el enemigo no se encuentra persiguiendo pues a�n no lo ha visto
    private Vector3 posicionInicial;
    private Coroutine regresarRutina;

    void Start()
    {
        // Obtener referencia al NavMeshAgent y guardar la posici�n inicial
        agente = GetComponent<NavMeshAgent>();
        posicionInicial = transform.position;

        if (conoDeVisionRenderer != null)
        {
            // Configurar el color inicial del cono de visi�n
            conoDeVisionRenderer.color = colorNormal;
        }

        if (agente != null)
        {
            // Deshabilitar rotaci�n autom�tica del NavMeshAgent (para controlar la rotaci�n manualmente)
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
        // Comprobar si el jugador est� dentro del cono de visi�n
        DetectarJugador();

        if (enModoPatrulla && !persiguiendoJugador)
        {
            Patrullar();
            RotarHaciaDireccion(agente.velocity.normalized);
        }

        // Si no est� persiguiendo al jugador, rota seg�n la direcci�n de movimiento
        if (!persiguiendoJugador)
        {
            Vector2 direccionMovimiento = agente.velocity.normalized;
            if (direccionMovimiento.sqrMagnitude > 0.01f)
            {
                RotarHaciaDireccion(direccionMovimiento);
            }
        }
    }

    // Detecta si el jugador entra en el radio de detecci�n y el cono de visi�n.
    // Si el jugador es detectado, el enemigo comienza a perseguirlo.
    void DetectarJugador()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, radioDeteccion, capaJugador);

        if (jugadorDetectado)
        {
            Vector2 direccionJugador = (jugadorDetectado.transform.position - transform.position).normalized;
            float angulo = Vector2.Angle(transform.right, direccionJugador);

            if (angulo < anguloVision / 2)         // Si el jugador est� dentro del cono de visi�n
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

                // Rota el cono de visi�n hacia el jugador
                RotarHaciaDireccion(direccionJugador);
            }
            else
            {
                if (persiguiendoJugador)

                {  ///Luego regresa a su patrullaje si la opci�n est� habilitada.
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
      lo que significa que se ejecutan de forma as�ncrona en Unity, 
      permitiendo pausas (yield return) sin bloquear el resto del c�digo.
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
            //yield return null; hace que se espere hasta el siguiente frame antes de continuar con la persecuci�n.
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
            // Se calcula la distancia a �stos.
            float distanciaAlWaypoint = Vector3.Distance(transform.position, waypoints[waypointActual].transform.position);
            // Si la distancia hacia el waypoint es menor que el umbral de detecci�n del waypoint
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

            // Rotar el enemigo (si quieres que el sprite tambi�n rote)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime * 200f);

            // Rotar el cono de visi�n (si tiene un objeto hijo asignado)
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

        Vector2 anguloIzq = Quaternion.Euler(0, 0, -anguloVision / 2) * transform.right;               // Creaci�n del gizmo del cono, dentro del radio de visi�n m�ximo
        Vector2 anguloDer = Quaternion.Euler(0, 0, anguloVision / 2) * transform.right;                // Con el �ngulo personalizado. 

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
