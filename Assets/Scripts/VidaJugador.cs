using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VidaJugador : MonoBehaviour
{
    public int VidaMaxima = 3;
    private int VidaActual;
    public Image[] VidaImagen;

    void Start()
    {
        VidaActual = VidaMaxima;
        actualizarInterfaz();
    }

    void actualizarInterfaz()
    {
        for (int i = 0; i < VidaImagen.Length; i++)
        {
            VidaImagen[i].enabled = i < VidaActual;
        }
        if (VidaActual <= 0)
        {
            ReiniciarEscena();
        }
    }

    void ReiniciarEscena()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public void RecibirDaño(int CantidadDaño)
    {
        VidaActual -= CantidadDaño;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();
    }

    public void ObtenerVida (int CuraTotal)
    {
        VidaActual += CuraTotal;
        VidaActual = Mathf.Clamp(VidaActual, 0, VidaMaxima);
        actualizarInterfaz();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
