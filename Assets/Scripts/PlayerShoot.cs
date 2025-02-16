using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject prefabDisparo;
    public GameObject puntoPartida;
    public float cooldown = 1f;

    private float tiempoUltimoDisparo;
    public GameObject Head; // Referencia a la cabeza para actualizar su animación

    void Start()
    {
        if (Head == null)
        {
            Debug.LogError("Head no asignado en PlayerShoot. Asigna el objeto en el Inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - tiempoUltimoDisparo >= cooldown)
        {
            float inputHorizontal = Input.GetAxis("HorizontalButton");
            float inputVertical = Input.GetAxis("VerticalButton");

            if (Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f)
            {
                Vector2 direccionDisparo = new Vector2(inputHorizontal, inputVertical).normalized;

                if (puntoPartida != null)
                {
                    GameObject disparo = Instantiate(prefabDisparo, puntoPartida.transform.position, Quaternion.identity);
                    Bullet movimientoDisparo = disparo.GetComponent<Bullet>();
                    if (movimientoDisparo != null)
                    {
                        movimientoDisparo.DireccionDisparo = direccionDisparo;
                    }
                    tiempoUltimoDisparo = Time.time;

                    // Actualizar la animación de la cabeza con base en la dirección del disparo
                    ActualizarAnimacionCabeza(direccionDisparo);
                }
            }
        }
    }

    void ActualizarAnimacionCabeza(Vector2 direccionDisparo)
    {
        // Resetear todas las animaciones
        Head.GetComponent<Animator>().SetBool("lookright", false);
        Head.GetComponent<Animator>().SetBool("lookleft", false);
        Head.GetComponent<Animator>().SetBool("lookup", false);
        Head.GetComponent<Animator>().SetBool("lookdown", false);

        // Determinar la dirección del disparo
        if (direccionDisparo.x > 0.5f) // Derecha
        {
            Head.GetComponent<Animator>().SetBool("lookright", true);
        }
        else if (direccionDisparo.x < -0.5f) // Izquierda
        {
            Head.GetComponent<Animator>().SetBool("lookleft", true);
        }
        else if (direccionDisparo.y > 0.5f) // Arriba
        {
            Head.GetComponent<Animator>().SetBool("lookup", true);
        }
        else if (direccionDisparo.y < -0.5f) // Abajo
        {
            Head.GetComponent<Animator>().SetBool("lookdown", true);
        }
    }
}
