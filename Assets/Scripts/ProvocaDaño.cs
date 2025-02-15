using System;
using System.Collections;
using UnityEngine;

public class ProvocaDaño : MonoBehaviour
{

    public VidaJugador vidaJugador;
    private bool PuedeDañar = true;
    private float Cooldown = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PuedeDañar)
        {
            vidaJugador.RecibirDaño(1);
            PuedeDañar = false;
            StartCoroutine(CooldownDaño());
            Console.WriteLine("-1 Vida");
        }
    }

    IEnumerator CooldownDaño()
    {
        yield return new WaitForSeconds(Cooldown);
        PuedeDañar = true;
    }

}
