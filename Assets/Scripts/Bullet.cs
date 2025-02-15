using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocidad = 5f;
    public float distanciaMaxima = 10f;
    public Vector2 DireccionDisparo;

    private float distanciaRecorrida;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(DireccionDisparo * velocidad * Time.deltaTime);
        distanciaRecorrida += velocidad * Time.deltaTime;
        if (distanciaRecorrida > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }
}
