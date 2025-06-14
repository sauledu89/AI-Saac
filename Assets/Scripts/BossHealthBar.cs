using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de la barra de vida gigante del Boss.
/// Se sincroniza automáticamente con la vida actual del Boss.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossEnemy bossRef; // Referencia al Boss
    [SerializeField] private Image barraVida;   // Imagen que representa la vida (el fill rojo)

    private void Start()
    {
        // Si no asignamos manualmente en el Inspector, intenta buscar automáticamente
        if (bossRef == null)
        {
            bossRef = Object.FindFirstObjectByType<BossEnemy>(); //Object.FindFisrstObjectByType
            if (bossRef == null)
            {
                Debug.LogError("BossHealthBar: No se encontró ningún BossEnemy en escena.");
            }
        }

        if (barraVida == null)
        {
            Debug.LogError("BossHealthBar: No se asignó la barra de vida (Image).");
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
