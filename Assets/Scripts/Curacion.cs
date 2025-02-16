using System;
using System.Collections;
using UnityEngine;

public class    Curacion : MonoBehaviour
{

    public VidaJugador vidaJugador;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            vidaJugador.ObtenerVida(1);
            Console.WriteLine("Curación +1");
            gameObject.SetActive(false);    

        }
    }


}
