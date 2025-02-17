using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocidad = 5f;
    public float distanciaMaxima = 10f;
    public Vector2 DireccionDisparo;
    public LayerMask capaEnemigos; // Detección de enemigos
    public LayerMask capaObstaculos; // Capa de obstáculos sólidos
    public LayerMask capaObstaculosDestructibles; // Capa de obstáculos que pueden ser destruidos

    private float distanciaRecorrida;

    void Update()
    {
        transform.Translate(DireccionDisparo * velocidad * Time.deltaTime);
        distanciaRecorrida += velocidad * Time.deltaTime;

        if (distanciaRecorrida > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectar colisión con enemigos
        if (((1 << collision.gameObject.layer) & capaEnemigos) != 0)
        {
            BaseEnemy enemigo = collision.GetComponent<BaseEnemy>();
            if (enemigo != null)
            {
                enemigo.RecibirDaño(1); // Hacer daño al enemigo
            }

            Destroy(gameObject); // Destruir la bala
        }

        // Detectar colisión con obstáculos sólidos (se destruye la bala)
        if (((1 << collision.gameObject.layer) & capaObstaculos) != 0)
        {
            Destroy(gameObject);
        }

        // Detectar colisión con obstáculos destructibles
        if (((1 << collision.gameObject.layer) & capaObstaculosDestructibles) != 0)
        {
            ObstaculoDestructible obstaculo = collision.GetComponent<ObstaculoDestructible>();
            if (obstaculo != null)
            {
                obstaculo.RecibirDaño(1); // Hacer daño al obstáculo
            }

            Destroy(gameObject); // Destruir la bala tras impactar
        }
    }
}
