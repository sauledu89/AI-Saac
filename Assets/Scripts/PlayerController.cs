using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float speedMultiplier = 5f;
    [SerializeField]
   // int hearts = 6;

    public float suavizado = 0.1f;
    //public Animator animator;
    private Vector2 velocidadActual;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // gameObject referenciará al objeto dueño de este script
        // gameObject.GetComponent<Transform>().position = new Vector3(20, 0, 0);

        // animator = GetComponent<Animator>();

    }
    /*
    private void Heartache()
    {
        hearts -= 1;
        Debug.Log("auch " + hearts);

    }

    */

    // Update is called once per frame
    void FixedUpdate()
    {
        //Time.deltaTime actualiza conforme al tiempo general y no a los fps
        //gameObject.GetComponent<Transform>().position = new Vector3(gameObject.transform.position.x + 0.1f*Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        //Otra manera de hacerlo 
        //gameObject.transform.Translate(0.1f * Time.deltaTime, 0, 0);
        //Ahora con input
        //Mover 1 unidad eje X y Y
        //transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * speedMultiplier * Time.deltaTime;
        //Ahora haremos lo mismo con físicas

        //GetKey detecta todo el tiempo mientras se presiona la tecla
        //GetKeyDown 1 vez detecta por pulsación 

        /* 1ra versión del código de movimiento

               if (Input.GetKey("a"))
               {
                   gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(speedMultiplier * -1000f * Time.deltaTime, 0));
               }

               if (Input.GetKey("d"))
               {
                   gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(speedMultiplier * 1000f * Time.deltaTime, 0));
               }

               if (Input.GetKey("w"))
               {
                   gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, speedMultiplier * 1000f * Time.deltaTime));
               }

               if (Input.GetKey("s"))
               {
                   gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, speedMultiplier * -1000f * Time.deltaTime));
               }
               }
       */

        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");
        Vector2 direccionDeseada = new Vector2(inputHorizontal, inputVertical).normalized;
        Vector2 direccionSuavizada = Vector2.Lerp(velocidadActual, direccionDeseada, suavizado);
        MoverObjeto(direccionSuavizada);
        //ActualizarAnimaciones(direccionSuavizada.x, direccionSuavizada.y);
        velocidadActual = direccionSuavizada;

    }
    void MoverObjeto(Vector2 direccion)
    {
        Vector2 desplazamiento = direccion * speedMultiplier * Time.deltaTime;
        transform.Translate(desplazamiento);
    }
    /*
        void ActualizarAnimaciones(float inputHorizontal, float inputVertical)
        {
            animator.SetFloat("Horizontal", inputHorizontal);
            animator.SetFloat("vertical", inputVertical);
        }


        //En el rigidBody Corregimos detalles modificando linear drag y masa en el inspector
        //linear drag indica que tan rápido se frenan sus fuerzas de movimiento
        //Angular drag es lo mismo pero para fuerzas angulares
        //Si modificamos las collision detection de discretas a continuas, el motor checará si entre las actualizaciones
        //frame por frame, cambió algo drásticamente y corregirá lo que tenga que corregir, útil en caso de que haya cambios 
        //ocurriendo muy rápido entre frames, recalculará las físicas para arreglarlo.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.tag == "Enemy")
            {
                Heartache();
            }
        }

    */



}
