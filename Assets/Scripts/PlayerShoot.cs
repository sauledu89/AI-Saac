using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject prefabDisparo;
    public GameObject puntoPartida;
    public float cooldown = 1f;


    private float tiempoUltimoDisparo;
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputHorizontal2 = Input.GetAxis("Horizontal");
        float inputVertical2 = Input.GetAxis("Vertical");

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
                }

            }
            else
            {
               // Debug.Log("El punto de partida no está asignado en el script de Disparo");

            }
        }

    }
    void ActualizarAnimaciones(float inputHorizontal2, float inputVertical2)
    {
        animator.SetFloat("Horizontal", inputHorizontal2);
        animator.SetFloat("Vertical", inputVertical2);
    }
}
