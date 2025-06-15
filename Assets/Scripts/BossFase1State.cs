using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // Necesario para mostrar texto

public class BossPhase1State : BaseState
{
    private BossFSM _fsmOwner;
    private BossEnemy _bossOwner;
    private GameObject _playerRef;

    private float _timerDisparoNormal = 0f;
    private float _timerMisiles = 0f;
    private float _timerSpawnEnemigos = 0f;

    private bool _esperandoAdvertencia = false;
    private bool _esperandoMuerteHorda = false;

    private float dificultadWeight = 0.6f;
    private float balanceWeight = 0.4f;

    public float rangoSpawnX = 15f;
    public float rangoSpawnY = 8f;

    private int rondaActual = 0; // 0 = verde, 1 = amarillo, 2 = rojo
    private int subOleada = 1; // Va de 1 a 6

    private Color[] coloresHorda = { Color.cyan, Color.yellow, Color.red };

    private GameObject textoRondaGO;

    public void Initialize(BossFSM ownerFSM, BossEnemy bossOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _fsmOwner = ownerFSM;
        _bossOwner = bossOwner;
        _playerRef = playerRef;

        // Crear objeto de texto en pantalla
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
    }

    void Start()
    {
        StateName = "Phase1State";
    }

    public override void OnUpdate()
    {
        if (_bossOwner.estaEnFase2)
        {
            _fsmOwner.ChangeState(_fsmOwner.GetPhase2State());
            return;
        }

        _timerDisparoNormal += Time.deltaTime;
        _timerMisiles += Time.deltaTime;
        _timerSpawnEnemigos += Time.deltaTime;

        if (_timerDisparoNormal >= _bossOwner.tiempoEntreDisparosNormales)
        {
            _timerDisparoNormal = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYDispararNormal());
        }

        if (_timerMisiles >= _bossOwner.tiempoEntreMisiles)
        {
            _timerMisiles = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYLanzarMisiles());
        }

        if (!_esperandoAdvertencia && !_esperandoMuerteHorda && BaseEnemy.EnemigosVivos.Count == 0)
        {
            if (rondaActual < coloresHorda.Length)
            {
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYSpawnearEnemigos());
            }
        }
    }

    private System.Collections.IEnumerator MostrarAdvertenciaYDispararNormal()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        DispararBalasNormales();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private System.Collections.IEnumerator MostrarAdvertenciaYLanzarMisiles()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        LanzarMisiles();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private System.Collections.IEnumerator MostrarAdvertenciaYSpawnearEnemigos()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);

        subOleada++;
        if (subOleada > 6)
        {
            subOleada = 1;
            rondaActual++;
            if (rondaActual >= coloresHorda.Length)
            {
                rondaActual = coloresHorda.Length - 1;
            }
        }

        SpawnearEnemigos();
        MostrarTextoRonda();

        GameObject.Destroy(icono);
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

    private System.Collections.IEnumerator DesaparecerTexto()
    {
        yield return new WaitForSeconds(2.5f);
        textoRondaGO.SetActive(false);
    }

    private void DispararBalasNormales()
    {
        foreach (Transform punto in _bossOwner.puntosDisparo)
        {
            if (punto == null) continue;
            GameObject bala = GameObject.Instantiate(_bossOwner.balaNormalPrefab, punto.position, Quaternion.identity);
            Vector2 direccion = (_playerRef.transform.position - punto.position).normalized;
            bala.transform.right = direccion;
        }
    }

    private void LanzarMisiles()
    {
        if (_bossOwner.puntosDisparo.Length >= 1)
        {
            GameObject misil = GameObject.Instantiate(_bossOwner.misilPrefab, _bossOwner.puntosDisparo[0].position, Quaternion.identity);
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
        Color colorHorda = coloresHorda[rondaActual];
        int cantidad = subOleada;
        Vector3 centro = _bossOwner.transform.position;

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicionSpawn = centro + new Vector3(
                Random.Range(-rangoSpawnX, rangoSpawnX),
                Random.Range(-rangoSpawnY, rangoSpawnY),
                0f
            );

            GameObject enemigoGO = GameObject.Instantiate(_bossOwner.enemigoExtraPrefab, posicionSpawn, Quaternion.identity);
            SpriteRenderer sr = enemigoGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = colorHorda;

            BaseEnemy baseScript = enemigoGO.GetComponent<BaseEnemy>();
            if (baseScript != null && !BaseEnemy.EnemigosVivos.Contains(baseScript))
            {
                baseScript.EstablecerColorPorRonda(colorHorda); //Asignamos color persistente
                BaseEnemy.EnemigosVivos.Add(baseScript);
            }

        }

        _bossOwner.esInvulnerable = false;
        _bossOwner.transform.position += Vector3.down * 2f;
        _bossOwner.CambiarColorAVulnerable();
        _bossOwner.VolverAPosicionInicialDespuesDeUnTiempo();

        OwnerFSMRef.StartCoroutine(EsperarMuerteHorda());
    }

    private System.Collections.IEnumerator EsperarMuerteHorda()
    {
        while (BaseEnemy.EnemigosVivos.Count > 0)
        {
            yield return null;
        }

        _esperandoMuerteHorda = false;
    }
}