using UnityEngine;
using System.Collections; 


/// <summary>
/// Script de control general del Boss Globo.
/// Guarda todas las variables p�blicas y funciones que los estados necesitar�n.
/// </summary>
public class BossEnemy : MonoBehaviour
{
    [Header("Configuraciones Generales")]
    public int vidaMaxima = 100;
    [HideInInspector] public int vidaActual;

    public bool esInvulnerable = true; // Al inicio, invulnerable
    public bool estaEnFase2 = false;   // Cambia cuando vida < 50% (o condici�n especial)

    [Header("Ataques y Cooldowns")]
    public GameObject balaNormalPrefab;
    public GameObject misilPrefab;
    public GameObject enemigoExtraPrefab; // Prefab del enemigo que spawnea

    public Transform[] puntosDisparo; // Puntos de donde dispara balas/misiles

    public float tiempoEntreDisparosNormales = 2f;
    public float tiempoEntreMisiles = 6f;
    public float tiempoEntreSpawns = 8f;

    [Header("Otros Efectos")]
    public GameObject iconoAdvertenciaPrefab; // Prefab del signo de admiraci�n
    public Transform puntoIcono;               // D�nde aparece el icono
    public Color colorNormal = Color.white;
    public Color colorFase2 = Color.red;
    public float tiempoSacudidaFase2 = 2f; // Cu�nto tiempo se sacude antes de entrar a Fase 2

    private SpriteRenderer spriteRenderer; // Referencia para cambiar color

    private Vector3 posicionInicial;

    void Start()
    {
        vidaActual = vidaMaxima;

        // Guarda la posición inicial
        posicionInicial = transform.position;
 
    // Obtener el sprite renderer autom�ticamente
    spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("BossEnemy: No se encontr� SpriteRenderer, no se podr� cambiar de color.");
        }
    }

    /// <summary>
    /// Llama esta funci�n cuando el boss recibe da�o.
    /// </summary>
    public void RecibirDaño(int cantidad)
    {
        if (esInvulnerable) return; // No recibe da�o si es invulnerable

        vidaActual -= cantidad;
        vidaActual = Mathf.Max(vidaActual, 0);

        if (!estaEnFase2 && vidaActual <= vidaMaxima / 2)
        {
            // Si baj� al 50%, entra en Fase 2
            EntrarFase2();
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    /// <summary>
    /// Devuelve la vida actual como porcentaje (entre 0 y 1).
    /// </summary>
    public float GetCurrentLifePercentage()
    {
        return (float)vidaActual / vidaMaxima;
    }

    /// <summary>
    /// Transici�n visual y de l�gica hacia la fase 2.
    /// </summary>
    private void EntrarFase2()
    {
        estaEnFase2 = true;
        esInvulnerable = false; // Desde ahora, siempre vulnerable

        // Cambiar color como indicador visual
        if (spriteRenderer != null)
        {
            StartCoroutine(CambiarColorTemporariamente());
        }
    }

    /// <summary>
    /// Corutina para cambiar color durante la transici�n a Fase 2.
    /// </summary>
    private System.Collections.IEnumerator CambiarColorTemporariamente()
    {
        float tiempoPasado = 0f;
        bool alternar = false;

        while (tiempoPasado < tiempoSacudidaFase2)
        {
            spriteRenderer.color = alternar ? colorFase2 : colorNormal;
            alternar = !alternar;
            tiempoPasado += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        spriteRenderer.color = colorFase2;
    }


public void DisparoBasico(GameObject objetivo)
{
    StartCoroutine(DisparoBasicoCoroutine(objetivo));
}

private IEnumerator DisparoBasicoCoroutine(GameObject objetivo)
{
    // 1. Mostrar el �cono de advertencia
    GameObject icono = Instantiate(iconoAdvertenciaPrefab, puntoIcono.position, Quaternion.identity, transform);

    // 2. Esperar 1.5 segundos
    yield return new WaitForSeconds(1.5f);

    // 3. Destruir el �cono
    Destroy(icono);

    // 4. Lanzar balas desde todos los puntos de disparo
    foreach (Transform punto in puntosDisparo)
    {
        if (punto == null) continue;

        // Calcular direcci�n hacia el jugador
        Vector2 direccion = (objetivo.transform.position - punto.position).normalized;

        // Instanciar bala
        GameObject bala = Instantiate(balaNormalPrefab, punto.position, Quaternion.identity);

        // Aplicar movimiento a la bala
        Rigidbody2D rb = bala.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direccion * 8f; // Velocidad de la bala (ajustable)
        }
    }
}
    /// <summary>
    /// Función general para mostrar el icono de advertencia y luego ejecutar un ataque.
    /// </summary>
    /// <param name="ataqueAccion">La función (acción) del ataque que queremos ejecutar después del aviso.</param>
    public void LanzarAdvertenciaYAtacar(System.Action ataqueAccion)
    {
        StartCoroutine(ProcesoAdvertencia(ataqueAccion));
    }

    private System.Collections.IEnumerator ProcesoAdvertencia(System.Action ataqueAccion)
    {
        // Instanciar el ícono de advertencia (!)
        GameObject icono = Instantiate(iconoAdvertenciaPrefab, puntoIcono.position, Quaternion.identity);
        icono.transform.SetParent(transform); // Para que se mueva junto con el jefe, si fuera necesario.

        // Esperar el tiempo de advertencia
        yield return new WaitForSeconds(1.5f);

        // Destruir el ícono después de advertir
        Destroy(icono);

        // Ejecutar el ataque real
        ataqueAccion?.Invoke();
    }

    /// <summary>
    /// Muerte del jefe.
    /// </summary>
    /// <summary>
    /// Muerte del jefe.
    /// </summary>
    private void Morir()
    {
        Debug.Log("El Boss ha sido derrotado.");

        // Opcional: Puedes aquí poner animación de muerte antes de desactivarlo
        StartCoroutine(ProcesoMuerte());
    }

    private IEnumerator ProcesoMuerte()
    {
        // (Opcional) Espera un pequeño tiempo si quieres animaciones
        yield return new WaitForSeconds(0.2f);

        // Resetea posición manualmente
        transform.position = posicionInicial;

        // Opcional: también resetear vida si quieres que al recargar esté limpio
        vidaActual = vidaMaxima;
        esInvulnerable = true;
        estaEnFase2 = false;

        // Ocultar al jefe para que no estorbe
        gameObject.SetActive(false);

        // Aquí ya no destruimos el objeto.
        // Unity al recargar la escena, recreará automáticamente el objeto y su posición inicial.
    }

}
