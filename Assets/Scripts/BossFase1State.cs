/// <summary>
/// Script: BossPhase1State
/// Autor: [Tu nombre]
/// Propósito: Controlar el comportamiento del jefe en su Fase 1.
/// Este script fue desarrollado como parte de un examen para aplicar conocimientos de IA, incluyendo generación procedural de enemigos (PCG),
/// ajuste dinámico de dificultad mediante pesos (DifficultyWeight y BalanceWeight),
/// uso de algoritmos de búsqueda tipo Greedy y visualización de estadísticas.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necesario para mostrar texto en pantalla

public class BossPhase1State : BaseState
{
    private BossFSM _fsmOwner; // Referencia al FSM principal del jefe
    private BossEnemy _bossOwner; // Referencia al objeto del jefe
    private GameObject _playerRef; // Referencia al jugador para los disparos

    // Timers para controlar tiempos entre acciones
    private float _timerDisparoNormal = 0f;
    private float _timerMisiles = 0f;
    private float _timerSpawnEnemigos = 0f;

    private bool _esperandoAdvertencia = false; // Controla si está en el tiempo de advertencia
    private bool _esperandoMuerteHorda = false; // Espera a que los enemigos mueran antes de continuar

    // Pesos utilizados por el algoritmo Greedy para ajustar dificultad vs. balance
    private float dificultadWeight = 0.6f;
    private float balanceWeight = 0.4f;

    // No se usa directamente pero fue parte del diseño original
    public float rangoSpawnX = 15f;
    public float rangoSpawnY = 8f;

    private int rondaActual = 0; // Número de ronda actual

    private GameObject textoRondaGO; // UI para mostrar la ronda
    private GameObject textoStatsGO; // UI para mostrar los stats generados por el PCG

    // Inicializa las referencias y prepara las UI de texto
    public void Initialize(BossFSM ownerFSM, BossEnemy bossOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _fsmOwner = ownerFSM;
        _bossOwner = bossOwner;
        _playerRef = playerRef;

        // Texto de ronda
        textoRondaGO = new GameObject("TextoRonda");
        textoRondaGO.transform.SetParent(GameObject.Find("Canvas").transform);
        Text texto = textoRondaGO.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 24;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        RectTransform rt = textoRondaGO.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 200);
        rt.sizeDelta = new Vector2(400, 50);
        textoRondaGO.SetActive(false);

        // Texto para mostrar estadísticas del enemigo generado
        textoStatsGO = new GameObject("TextoStats");
        textoStatsGO.transform.SetParent(GameObject.Find("Canvas").transform);
        Text statsText = textoStatsGO.AddComponent<Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsText.fontSize = 20;
        statsText.alignment = TextAnchor.UpperCenter;
        statsText.color = Color.green;
        RectTransform statsRT = textoStatsGO.GetComponent<RectTransform>();
        statsRT.anchoredPosition = new Vector2(0, 140);
        statsRT.sizeDelta = new Vector2(500, 80);
        textoStatsGO.SetActive(false);
    }

    void Start()
    {
        StateName = "Phase1State";
    }

    public override void OnUpdate()
    {
        // Cambia a Fase 2 si la condición de vida se cumple
        if (_bossOwner.estaEnFase2)
        {
            _fsmOwner.ChangeState(_fsmOwner.GetPhase2State());
            return;
        }

        // Acumuladores de tiempo para las acciones
        _timerDisparoNormal += Time.deltaTime;
        _timerMisiles += Time.deltaTime;
        _timerSpawnEnemigos += Time.deltaTime;

        // Disparo básico cada cierto tiempo
        if (_timerDisparoNormal >= _bossOwner.tiempoEntreDisparosNormales)
        {
            _timerDisparoNormal = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYDispararNormal());
        }

        // Lanzamiento de misiles
        if (_timerMisiles >= _bossOwner.tiempoEntreMisiles)
        {
            _timerMisiles = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYLanzarMisiles());
        }

        // Spawnea enemigos si no hay vivos y no se está en animación de advertencia
        if (!_esperandoAdvertencia && !_esperandoMuerteHorda && BaseEnemy.EnemigosVivos.Count == 0)
        {
            OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYSpawnearEnemigos());
        }
    }

    private IEnumerator MostrarAdvertenciaYDispararNormal()
    {
        _esperandoAdvertencia = true;
        GameObject icono = Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        DispararBalasNormales();
        Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private IEnumerator MostrarAdvertenciaYLanzarMisiles()
    {
        _esperandoAdvertencia = true;
        GameObject icono = Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        LanzarMisiles();
        Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private IEnumerator MostrarAdvertenciaYSpawnearEnemigos()
    {
        _esperandoAdvertencia = true;
        GameObject icono = Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        SpawnearEnemigos();
        MostrarTextoRonda();
        Destroy(icono);
        _esperandoAdvertencia = false;
        _esperandoMuerteHorda = true;
    }

    private void MostrarTextoRonda()
    {
        if (textoRondaGO != null)
        {
            Text texto = textoRondaGO.GetComponent<Text>();
            texto.text = $"RONDA {rondaActual + 1}";
            textoRondaGO.SetActive(true);
            OwnerFSMRef.StartCoroutine(DesaparecerTexto());
        }
    }

    private void MostrarTextoStats(PCGEnemyStats stats)
    {
        if (textoStatsGO != null && stats != null)
        {
            Text texto = textoStatsGO.GetComponent<Text>();
            texto.text =
                $"HP: {stats.HP:F0}  " +
                $"Damage: {stats.Damage:F0}  " +
                $"AtkRate: {stats.AttackRate:F2}\n" +
                $"Range: {stats.AttackRange:F1}  " +
                $"Speed: {stats.MovementSpeed:F1}\n" +
                $"Dificultad: {stats.GetDifficultyV2():F2}   " +
                $"Balance: {stats.GetBalanceScore():F2}";
            textoStatsGO.SetActive(true);
            OwnerFSMRef.StartCoroutine(DesaparecerTextoStats());
        }
    }

    private IEnumerator DesaparecerTextoStats()
    {
        yield return new WaitForSeconds(2.5f);
        textoStatsGO.SetActive(false);
    }

    private IEnumerator DesaparecerTexto()
    {
        yield return new WaitForSeconds(2.5f);
        textoRondaGO.SetActive(false);
    }

    private void DispararBalasNormales()
    {
        foreach (Transform punto in _bossOwner.puntosDisparo)
        {
            if (punto == null) continue;
            GameObject bala = Instantiate(_bossOwner.balaNormalPrefab, punto.position, Quaternion.identity);
            Vector2 direccion = (_playerRef.transform.position - punto.position).normalized;
            bala.transform.right = direccion;
        }
    }

    private void LanzarMisiles()
    {
        if (_bossOwner.puntosDisparo.Length >= 1)
        {
            GameObject misil = Instantiate(_bossOwner.misilPrefab, _bossOwner.puntosDisparo[0].position, Quaternion.identity);
            Rigidbody2D rb = misil.GetComponent<Rigidbody2D>();
            if (rb != null) rb.AddTorque(50f, ForceMode2D.Force);
        }
        else
        {
            Debug.LogWarning("BossEnemy: No hay suficientes puntos de disparo para el misil.");
        }
    }

    private void SpawnearEnemigos()
    {
        // Calculamos la cantidad de enemigos a spawnear con base en la ronda actual.
        // Siempre se asegura que esté entre 1 y 6. Esto simula una dificultad creciente
        // sin saturar el escenario, y permite que la IA escale progresivamente.
        int cantidad = Mathf.Clamp(rondaActual + 1, 1, 6); // Máximo 6 enemigos

        // Aquí aplicamos un concepto importante de IA en juegos: pesos ajustables.
        // Incrementamos el peso de dificultad conforme avanzan las rondas (más enemigos desafiantes).
        // balanceWeight se ajusta automáticamente para que la suma sea 1 (normalización).
        dificultadWeight = Mathf.Clamp01(0.3f + rondaActual * 0.1f);
        balanceWeight = 1f - dificultadWeight;

        // Si no hay puntos de spawn configurados, se lanza una advertencia en consola.
        if (_bossOwner.puntosSpawnEnemigos.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados para enemigos.");
            return;
        }

        // Se copia la lista de puntos de spawn y se mezcla aleatoriamente.
        // Esto hace que los enemigos aparezcan en lugares diferentes cada vez,
        // lo cual añade variedad sin cambiar el diseño del mapa.
        List<Transform> puntosDisponibles = new List<Transform>(_bossOwner.puntosSpawnEnemigos);
        for (int i = 0; i < puntosDisponibles.Count; i++)
        {
            Transform temp = puntosDisponibles[i];
            int randomIndex = Random.Range(i, puntosDisponibles.Count);
            puntosDisponibles[i] = puntosDisponibles[randomIndex];
            puntosDisponibles[randomIndex] = temp;
        }

        // Evitamos instanciar más enemigos que puntos disponibles.
        cantidad = Mathf.Min(cantidad, puntosDisponibles.Count);

        // Este es el ciclo principal de generación de enemigos:
        // 1. Se toma un punto de spawn
        // 2. Se generan estadísticas mediante Greedy Search (otro concepto central del examen)
        // 3. Se instancia el prefab del enemigo
        // 4. Se asignan las estadísticas generadas
        // 5. Se registra el enemigo en la lista global y se muestran sus stats en pantalla
        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicionSpawn = puntosDisponibles[i].position;

            // Este método ejecuta la búsqueda greedy con los pesos de dificultad/balance actuales.
            PCGEnemyStats stats = GenerarStatsConGreedySearch(_bossOwner.configValues, dificultadWeight, balanceWeight);

            // Instanciamos el enemigo en la posición elegida
            GameObject enemigoGO = Instantiate(_bossOwner.enemigoExtraPrefab, posicionSpawn, Quaternion.identity);

            // Registramos el enemigo en la lista de enemigos vivos
            BaseEnemy baseScript = enemigoGO.GetComponent<BaseEnemy>();
            if (baseScript != null)
                BaseEnemy.EnemigosVivos.Add(baseScript);

            // Aplicamos las estadísticas al enemigo usando su método SetStats
            Escapista escapista = enemigoGO.GetComponent<Escapista>();
            if (escapista != null)
                escapista.SetStats(stats);

            // Mostramos sus estadísticas en pantalla por 2.5 segundos
            MostrarTextoStats(stats);
        }

        // Al finalizar el spawn, el jefe baja y se vuelve vulnerable momentáneamente.
        // Esto introduce una pausa estratégica en el combate (aprendido como diseño de comportamiento).
        _bossOwner.esInvulnerable = false;
        _bossOwner.transform.position += Vector3.down * 2f;
        _bossOwner.CambiarColorAVulnerable();
        _bossOwner.VolverAPosicionInicialDespuesDeUnTiempo();

        // Se inicia la rutina que espera a que todos los enemigos mueran antes de generar más.
        StartCoroutine(EsperarMuerteHorda());
    }


    private PCGEnemyStats GenerarStatsConGreedySearch(PCGConfigValuesScriptableObject config, float difWeight, float balWeight)
    {
        PCGEnemyStats origen = new PCGEnemyStats(config);
        PCGEnemyStats mejor = origen;

        int iteraciones = 0;
        int maxIteraciones = 100;

        // Búsqueda greedy para encontrar stats que optimicen la función de puntuación
        while (iteraciones < maxIteraciones)
        {
            iteraciones++;
            List<PCGEnemyStats> vecinos = mejor.GetNeighbors();

            foreach (var vecino in vecinos)
            {
                if (vecino.GetTotalScore(difWeight, balWeight) > mejor.GetTotalScore(difWeight, balWeight))
                {
                    mejor = vecino;
                }
            }
        }

        return mejor;
    }

    private IEnumerator EsperarMuerteHorda()
    {
        while (BaseEnemy.EnemigosVivos.Count > 0)
            yield return null;

        rondaActual++; // Solo avanzamos si sobrevivimos
        _esperandoMuerteHorda = false;
    }
}
