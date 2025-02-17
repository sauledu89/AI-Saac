using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float velocidad = 5f; // Velocidad de la bala
    public int daño = 1; // Daño que inflige al jugador

    public float distanciaMaxima = 10f; // Distancia máxima que puede recorrer antes de destruirse
    private Vector3 posicionInicial; // Guarda la posición inicial de la bala

    private void Start()
    {
        // Al iniciar, guardamos la posición inicial de la bala
        posicionInicial = transform.position;
    }

    private void Update()
    {
        // Mueve la bala hacia adelante en la dirección en la que fue disparada
        transform.Translate(Time.deltaTime * velocidad * Vector2.right);

        // Calcula la distancia recorrida desde su posición inicial
        float distanciaRecorrida = Vector3.Distance(posicionInicial, transform.position);

        // Si la distancia recorrida supera la distancia máxima, la bala se destruye
        if (distanciaRecorrida > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica si la bala colisiona con el jugador
        if (other.TryGetComponent(out VidaJugador vidaJugador))
        {
            // Si el jugador tiene el script VidaJugador, recibe daño
            vidaJugador.RecibirDaño(daño);

            // La bala se destruye tras impactar con el jugador
            Destroy(gameObject);
        }
    }
}
