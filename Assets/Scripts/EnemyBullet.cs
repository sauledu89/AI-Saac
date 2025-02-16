using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float velocidad;
    public int da�o;

    private void Update()
    {
        transform.Translate(Time.deltaTime * velocidad * Vector2.right);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out VidaJugador vidaJugador))
            {
            vidaJugador.RecibirDa�o(da�o);
            }
    }
}
