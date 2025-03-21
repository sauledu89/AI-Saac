using System;
using System.Collections;
using UnityEngine;

public class ProvocaDaño : MonoBehaviour
{
    public VidaJugador vidaJugador;

    [Header("Configuración de Daño")]
    public LayerMask capaJugador;  // Layer para el jugador
    public LayerMask capaEnemigo;  // Layer para los enemigos
    public float Cooldown = 1f;
    private bool PuedeDañar = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;

        // Si colisiona con el jugador
        if (((1 << otherLayer) & capaJugador) != 0 && PuedeDañar)
        {
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDaño(1);
                PuedeDañar = false;
                StartCoroutine(CooldownDaño());
                Debug.Log("-1 Vida al Jugador");
            }
        }

        // Si colisiona con un enemigo que tiene el script BaseEnemy
        if (((1 << otherLayer) & capaEnemigo) != 0 && PuedeDañar)
        {
            BaseEnemy enemigo = other.GetComponent<BaseEnemy>(); // Obtiene el script BaseEnemy del enemigo golpeado
            if (enemigo != null)
            {
                enemigo.RecibirDaño(1); // Llama a la función RecibirDaño de ese enemigo en específico
                PuedeDañar = false;
                StartCoroutine(CooldownDaño());
                Debug.Log($"-1 Vida al Enemigo: {other.gameObject.name}");
            }
        }
    }

    IEnumerator CooldownDaño()
    {
        yield return new WaitForSeconds(Cooldown);
        PuedeDañar = true;
    }
}
