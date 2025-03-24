using UnityEngine;
using System.Collections;

/*
    Script para un obstáculo dinámico en 2D que puede moverse y/o rotar en el escenario.

    Acciones:
    - Rota sobre su eje Z si está activado.
    - Se desliza entre dos posiciones.
    - Cambia su color para advertir que se va a mover.
    - Traza su trayectoria con Gizmos para poder visualizarla.
*/

public class ObstaculoDinamico : MonoBehaviour
{
    [Header("Movimiento")]
    public bool rotar = false;                          // Si debe rotar en eje Z
    public float velocidadRotacion = 90f;               // Qué tan rápido gira

    public bool moverse = true;                         // Si debe desplazarse
    public Vector2 direccionMovimiento = Vector2.right; // Dirección del movimiento
    public float distanciaMovimiento = 5f;              // Distancia desde el punto inicial
    public float velocidadMovimiento = 2f;              // Velocidad con la que se desliza

    [Header("Advertencia Visual")]
    public float tiempoDeAdvertencia = 1f;              // Tiempo que dura la advertencia antes de moverse
    public Color colorAdvertencia = Color.red;          // Color del sprite mientras advierte
    private Color colorOriginal;                        // Color original del sprite

    private Vector3 posicionInicial;                    // Punto desde donde parte el obstáculo
    private bool yendoHaciaAdelante = true;             // Dirección del movimiento
    private float temporizadorMovimiento;               // Control del tiempo entre cada movimiento
    private bool esperando = false;                     // Si está en estado de advertencia

    private SpriteRenderer sr;

    void Start()
    {
        // Guardamos la posición inicial del obstáculo, desde donde comenzará a moverse
        posicionInicial = transform.position;

        // Inicializamos el temporizador con el tiempo de advertencia
        // Esto sirve para que el primer movimiento ocurra después de advertir al jugador
        temporizadorMovimiento = tiempoDeAdvertencia;

        // Obtenemos el componente SpriteRenderer para poder modificar el color del sprite
        // Esto es parte del sistema de advertencia visual
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Guardamos el color original para poder restaurarlo después del aviso
            colorOriginal = sr.color;
        }
    }

    void Update()
    {
        // Si la opción "rotar" está activada, el objeto rotará sobre su eje Z constantemente
        // Utilizamos transform.Rotate() y Vector3.forward porque en 2D rotamos sobre el eje Z
        if (rotar)
        {
            transform.Rotate(Vector3.forward * velocidadRotacion * Time.deltaTime);
        }

        // Si se activó el movimiento y no estamos en estado de espera (advertencia),
        // disminuimos el temporizador y activamos la corrutina cuando llegue a 0
        if (moverse && !esperando)
        {
            temporizadorMovimiento -= Time.deltaTime;

            if (temporizadorMovimiento <= 0)
            {
                // Iniciamos la corrutina que hace la advertencia visual y mueve al obstáculo
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

        // Restauramos el color original del sprite después de la advertencia
        if (sr != null)
        {
            sr.color = colorOriginal;
        }

        // Calculamos el destino hacia el cual se moverá el obstáculo
        // Si va hacia adelante, sumamos dirección * distancia; si no, regresa al punto inicial
        Vector3 destino = yendoHaciaAdelante
            ? posicionInicial + (Vector3)direccionMovimiento.normalized * distanciaMovimiento
            : posicionInicial;

        // Movimiento suave del objeto hacia el destino usando MoveTowards en cada frame
        // Esto evita teletransportación y da la sensación de deslizamiento
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadMovimiento * Time.deltaTime);
            yield return null; // Esperamos al siguiente frame para continuar el movimiento
        }

        // Al llegar exactamente al destino, actualizamos variables para el próximo ciclo
        transform.position = destino;
        yendoHaciaAdelante = !yendoHaciaAdelante;         // Cambiamos dirección para la siguiente vez
        temporizadorMovimiento = tiempoDeAdvertencia;     // Reiniciamos el temporizador
        esperando = false;                                // Salimos del estado de advertencia
    }

    void OnDrawGizmos()
    {
        // Dibujamos en el editor la trayectoria del movimiento del obstáculo

        Gizmos.color = Color.yellow;

        if (Application.isPlaying)
        {
            // Si el juego está corriendo, usamos la posición inicial almacenada en tiempo real
            Vector3 destino = yendoHaciaAdelante
                ? posicionInicial + (Vector3)direccionMovimiento.normalized * distanciaMovimiento
                : posicionInicial;

            Gizmos.DrawLine(transform.position, destino);      // Línea de movimiento
            Gizmos.DrawWireSphere(destino, 0.3f);              // Marcador del punto destino
        }
        else
        {
            // Si estamos en modo editor (sin reproducir), usamos la posición actual del objeto
            Vector3 origen = transform.position;
            Vector3 destino = origen + (Vector3)direccionMovimiento.normalized * distanciaMovimiento;

            Gizmos.DrawLine(origen, destino);
            Gizmos.DrawWireSphere(destino, 0.3f);
        }

        // Si el objeto rota, dibujamos una flecha indicando el eje de rotación
        if (rotar)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.forward);
        }

        // Dibujamos un marcador rojo para indicar la posición actual del obstáculo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}