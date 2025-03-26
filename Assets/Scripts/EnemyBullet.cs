using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float velocidad = 5f;         // Velocidad de la bala
    public int daño = 1;                 // Daño que inflige al jugador
    public float distanciaMaxima = 10f;  // Distancia máxima antes de autodestruirse
    private Vector3 posicionInicial;     // Guarda la posición donde apareció la bala

    [Header("Colisión con obstáculos")]
    public LayerMask capasQueBloqueanBala; // Layers como "Wall" o "Obstacle" que bloquean la bala

    private void Start()
    {
        // Guardamos la posición inicial al momento de ser instanciada
        posicionInicial = transform.position;
    }

    private void Update()
    {
        // Mover hacia la derecha local (la bala rota al instanciarse)
        transform.Translate(Time.deltaTime * velocidad * Vector2.right);

        // Si la bala ha recorrido su distancia máxima, se destruye
        float distanciaRecorrida = Vector3.Distance(posicionInicial, transform.position);
        if (distanciaRecorrida > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Si impacta al jugador
        if (other.TryGetComponent(out VidaJugador vidaJugador))
        {
            vidaJugador.RecibirDaño(daño);
            Destroy(gameObject);
            return;
        }

        // 2. Si choca contra un objeto cuya layer esté incluida en el LayerMask
        if (((1 << other.gameObject.layer) & capasQueBloqueanBala) != 0)
        {
            Destroy(gameObject);
        }
    }
}
