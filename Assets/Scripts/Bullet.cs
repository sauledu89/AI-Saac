using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocidad = 5f;
    public float distanciaMaxima = 10f;
    public Vector2 DireccionDisparo;
    public LayerMask capaEnemigos; // Detecci�n de enemigos
    public LayerMask capaObstaculos; // Capa de obst�culos s�lidos
    public LayerMask capaObstaculosDestructibles; // Capa de obst�culos que pueden ser destruidos

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
        // Detectar colisi�n con enemigos
        if (((1 << collision.gameObject.layer) & capaEnemigos) != 0)
        {
            BaseEnemy enemigo = collision.GetComponent<BaseEnemy>();
            if (enemigo != null)
            {
                enemigo.RecibirDa�o(1); // Hacer da�o al enemigo
            }

            Destroy(gameObject); // Destruir la bala
        }

        // Detectar colisi�n con obst�culos s�lidos (se destruye la bala)
        if (((1 << collision.gameObject.layer) & capaObstaculos) != 0)
        {
            Destroy(gameObject);
        }

        // Detectar colisi�n con obst�culos destructibles
        if (((1 << collision.gameObject.layer) & capaObstaculosDestructibles) != 0)
        {
            ObstaculoDestructible obstaculo = collision.GetComponent<ObstaculoDestructible>();
            if (obstaculo != null)
            {
                obstaculo.RecibirDa�o(1); // Hacer da�o al obst�culo
            }

            Destroy(gameObject); // Destruir la bala tras impactar
        }
    }
}
