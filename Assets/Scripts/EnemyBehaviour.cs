using UnityEngine;
using UnityEngine.UI;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelStats;
    public Text textoStats;

    [Header("Referencia dinámica")]
    public BaseEnemy enemigoActual; // Se asigna automáticamente desde BossPhase1State

    private bool visible = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            visible = !visible;
            panelStats.SetActive(visible);
        }

        if (visible && enemigoActual != null)
        {
            PCGEnemyStats stats = enemigoActual.GetComponent<Escapista>()?.GetStats();

            if (stats != null)
            {
                string texto =
                    $"HP: {stats.HP:F1}\n" +
                    $"Damage: {stats.Damage:F1}\n" +
                    $"AttackRate: {stats.AttackRate:F2}\n" +
                    $"Range: {stats.AttackRange:F1}\n" +
                    $"Speed: {stats.MovementSpeed:F1}\n" +
                    $"Dificultad: {stats.GetDifficultyV2():F2}\n" +
                    $"Balance: {stats.GetBalanceScore():F2}";
                textoStats.text = texto;
            }
        }
    }
}
