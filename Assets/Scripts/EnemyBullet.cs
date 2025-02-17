using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float velocidad = 5f; // Velocidad de la bala
    public int da�o = 1; // Da�o que inflige al jugador

    public float distanciaMaxima = 10f; // Distancia m�xima que puede recorrer antes de destruirse
    private Vector3 posicionInicial; // Guarda la posici�n inicial de la bala

    private void Start()
    {
        // Al iniciar, guardamos la posici�n inicial de la bala
        posicionInicial = transform.position;
    }

    private void Update()
    {
        // Mueve la bala hacia adelante en la direcci�n en la que fue disparada
        transform.Translate(Time.deltaTime * velocidad * Vector2.right);

        // Calcula la distancia recorrida desde su posici�n inicial
        float distanciaRecorrida = Vector3.Distance(posicionInicial, transform.position);

        // Si la distancia recorrida supera la distancia m�xima, la bala se destruye
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
            // Si el jugador tiene el script VidaJugador, recibe da�o
            vidaJugador.RecibirDa�o(da�o);

            // La bala se destruye tras impactar con el jugador
            Destroy(gameObject);
        }
    }
}
