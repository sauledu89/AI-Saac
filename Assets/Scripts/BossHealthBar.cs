using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de la barra de vida gigante del Boss.
/// Se sincroniza autom�ticamente con la vida actual del Boss.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossEnemy bossRef; // Referencia al Boss
    [SerializeField] private Image barraVida;   // Imagen que representa la vida (el fill rojo)

    private void Start()
    {
        // Si no asignamos manualmente en el Inspector, intenta buscar autom�ticamente
        if (bossRef == null)
        {
            bossRef = Object.FindFirstObjectByType<BossEnemy>(); //Object.FindFisrstObjectByType
            if (bossRef == null)
            {
                Debug.LogError("BossHealthBar: No se encontr� ning�n BossEnemy en escena.");
            }
        }

        if (barraVida == null)
        {
            Debug.LogError("BossHealthBar: No se asign� la barra de vida (Image).");
        }
    }

    private void Update()
    {
        if (bossRef != null && barraVida != null)
        {
            // Actualizamos el FillAmount (valor entre 0 y 1)
            barraVida.fillAmount = bossRef.GetCurrentLifePercentage();
        }
    }
}
