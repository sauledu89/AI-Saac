using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float velocidad = 5f;
    public float distanciaMaxima = 30f;
    private Vector3 posicionInicial;

    private int daño;

    [Header("Colisión con obstáculos")]
    public LayerMask capasQueBloqueanBala;

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
        transform.Translate(Time.deltaTime * velocidad * Vector2.right);

        float distanciaRecorrida = Vector3.Distance(posicionInicial, transform.position);
        if (distanciaRecorrida > distanciaMaxima)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out VidaJugador vidaJugador))
        {
            vidaJugador.RecibirDaño(daño);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & capasQueBloqueanBala) != 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(int nuevoDaño)
    {
        daño = nuevoDaño;
    }

}
