using UnityEngine;
using System.Collections;

/*
    Script para un obst�culo din�mico en 2D que puede moverse y/o rotar en el escenario.

    Acciones:
    - Rota sobre su eje Z si est� activado.
    - Se desliza entre dos posiciones.
    - Cambia su color para advertir que se va a mover.
    - Traza su trayectoria con Gizmos para poder visualizarla.
*/

public class ObstaculoDinamico : MonoBehaviour
{
    [Header("Movimiento")]
    public bool rotar = false;                          // Si debe rotar en eje Z
    public float velocidadRotacion = 90f;               // Qu� tan r�pido gira

    public bool moverse = true;                         // Si debe desplazarse
    public Vector2 direccionMovimiento = Vector2.right; // Direcci�n del movimiento
    public float distanciaMovimiento = 5f;              // Distancia desde el punto inicial
    public float velocidadMovimiento = 2f;              // Velocidad con la que se desliza

    [Header("Advertencia Visual")]
    public float tiempoDeAdvertencia = 1f;              // Tiempo que dura la advertencia antes de moverse
    public Color colorAdvertencia = Color.red;          // Color del sprite mientras advierte
    private Color colorOriginal;                        // Color original del sprite

    private Vector3 posicionInicial;                    // Punto desde donde parte el obst�culo
    private bool yendoHaciaAdelante = true;             // Direcci�n del movimiento
    private float temporizadorMovimiento;               // Control del tiempo entre cada movimiento
    private bool esperando = false;                     // Si est� en estado de advertencia

    private SpriteRenderer sr;

    void Start()
    {
        // Guardamos la posici�n inicial del obst�culo, desde donde comenzar� a moverse
        posicionInicial = transform.position;

        // Inicializamos el temporizador con el tiempo de advertencia
        // Esto sirve para que el primer movimiento ocurra despu�s de advertir al jugador
        temporizadorMovimiento = tiempoDeAdvertencia;

        // Obtenemos el componente SpriteRenderer para poder modificar el color del sprite
        // Esto es parte del sistema de advertencia visual
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Guardamos el color original para poder restaurarlo despu�s del aviso
            colorOriginal = sr.color;
        }
    }

    void Update()
    {
        // Si la opci�n "rotar" est� activada, el objeto rotar� sobre su eje Z constantemente
        // Utilizamos transform.Rotate() y Vector3.forward porque en 2D rotamos sobre el eje Z
        if (rotar)
        {
            transform.Rotate(Vector3.forward * velocidadRotacion * Time.deltaTime);
        }

        // Si se activ� el movimiento y no estamos en estado de espera (advertencia),
        // disminuimos el temporizador y activamos la corrutina cuando llegue a 0
        if (moverse && !esperando)
        {
            temporizadorMovimiento -= Time.deltaTime;

            if (temporizadorMovimiento <= 0)
            {
                // Iniciamos la corrutina que hace la advertencia visual y mueve al obst�culo
                StartCoroutine(AdvertenciaYMover());
            }
        }
    }

    IEnumerator AdvertenciaYMover()
    {
        esperando = true; // Entramos en estado de espera para evitar que el Update lo repita

        // Cambiamos el color del sprite como advertencia visual
        if (sr != null)
        {
            sr.color = colorAdvertencia;
        }

        // Esperamos el tiempo definido como advertencia antes de mover
        yield return new WaitForSeconds(tiempoDeAdvertencia);

        // Restauramos el color original del sprite despu�s de la advertencia
        if (sr != null)
        {
            sr.color = colorOriginal;
        }

        // Calculamos el destino hacia el cual se mover� el obst�culo
        // Si va hacia adelante, sumamos direcci�n * distancia; si no, regresa al punto inicial
        Vector3 destino = yendoHaciaAdelante
            ? posicionInicial + (Vector3)direccionMovimiento.normalized * distanciaMovimiento
            : posicionInicial;

        // Movimiento suave del objeto hacia el destino usando MoveTowards en cada frame
        // Esto evita teletransportaci�n y da la sensaci�n de deslizamiento
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
            yield return null; // Esperamos al siguiente frame para continuar el movimiento
        }

        // Al llegar exactamente al destino, actualizamos variables para el pr�ximo ciclo
        transform.position = destino;
        yendoHaciaAdelante = !yendoHaciaAdelante;         // Cambiamos direcci�n para la siguiente vez
        temporizadorMovimiento = tiempoDeAdvertencia;     // Reiniciamos el temporizador
        esperando = false;                                // Salimos del estado de advertencia
    }

    void OnDrawGizmos()
    {
        // Dibujamos en el editor la trayectoria del movimiento del obst�culo

        Gizmos.color = Color.yellow;

        if (Application.isPlaying)
        {
            // Si el juego est� corriendo, usamos la posici�n inicial almacenada en tiempo real
            Vector3 destino = yendoHaciaAdelante
                ? posicionInicial + (Vector3)direccionMovimiento.normalized * distanciaMovimiento
                : posicionInicial;

            Gizmos.DrawLine(transform.position, destino);      // L�nea de movimiento
            Gizmos.DrawWireSphere(destino, 0.3f);              // Marcador del punto destino
        }
        else
        {
            // Si estamos en modo editor (sin reproducir), usamos la posici�n actual del objeto
            Vector3 origen = transform.position;
            Vector3 destino = origen + (Vector3)direccionMovimiento.normalized * distanciaMovimiento;

            Gizmos.DrawLine(origen, destino);
            Gizmos.DrawWireSphere(destino, 0.3f);
        }

        // Si el objeto rota, dibujamos una flecha indicando el eje de rotaci�n
        if (rotar)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.forward);
        }

        // Dibujamos un marcador rojo para indicar la posici�n actual del obst�culo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}