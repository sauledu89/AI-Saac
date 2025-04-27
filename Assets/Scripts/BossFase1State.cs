using UnityEngine;

public class BossPhase1State : BaseState
{
    private BossFSM _fsmOwner;
    private BossEnemy _bossOwner;
    private GameObject _playerRef;

    private float _timerDisparoNormal = 0f;
    private float _timerMisiles = 0f;
    private float _timerSpawnEnemigos = 0f;

    private bool _esperandoAdvertencia = false;

    public void Initialize(BossFSM ownerFSM, BossEnemy bossOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _fsmOwner = ownerFSM;
        _bossOwner = bossOwner;
        _playerRef = playerRef;
    }

    void Start()
    {
        StateName = "Phase1State";
    }

    public override void OnUpdate()
    {
        // 1. Primero revisamos si ya debe pasar a fase 2
        if (_bossOwner.estaEnFase2)
        {
            _fsmOwner.ChangeState(_fsmOwner.GetPhase2State());
            return; // Importantísimo para no seguir ejecutando lógica de fase 1
        }

        // 2. Luego seguimos la lógica normal de ataques
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

        if (_timerSpawnEnemigos >= _bossOwner.tiempoEntreSpawns)
        {
            _timerSpawnEnemigos = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYSpawnearEnemigos());
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

        SpawnearEnemigos();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;

        _bossOwner.esInvulnerable = false;
    }

    private void DispararBalasNormales()
    {
        foreach (Transform punto in _bossOwner.puntosDisparo)
        {
            if (punto == null) continue;

            // Instanciar la bala
            GameObject bala = GameObject.Instantiate(_bossOwner.balaNormalPrefab, punto.position, Quaternion.identity);

            // Calcular dirección hacia el jugador
            Vector2 direccion = (_playerRef.transform.position - punto.position).normalized;

            // Rotar la bala para que apunte en esa dirección
            bala.transform.right = direccion;
        }
    }

    private void LanzarMisiles()
    {
        if (_bossOwner.puntosDisparo.Length >= 2)
        {
            GameObject misilIzquierda = GameObject.Instantiate(_bossOwner.misilPrefab, _bossOwner.puntosDisparo[0].position, Quaternion.identity);
            GameObject misilDerecha = GameObject.Instantiate(_bossOwner.misilPrefab, _bossOwner.puntosDisparo[1].position, Quaternion.identity);

            Rigidbody2D rb1 = misilIzquierda.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = misilDerecha.GetComponent<Rigidbody2D>();

            if (rb1 != null) rb1.AddTorque(50f, ForceMode2D.Force);
            if (rb2 != null) rb2.AddTorque(-50f, ForceMode2D.Force);
        }
        else
        {
            Debug.LogWarning("BossEnemy: No hay suficientes puntos de disparo para los misiles.");
        }
    }

    private void SpawnearEnemigos()
    {
        if (_bossOwner.puntoIcono != null && _bossOwner.enemigoExtraPrefab != null)
        {
            GameObject.Instantiate(_bossOwner.enemigoExtraPrefab, _bossOwner.puntoIcono.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("BossEnemy: No hay prefab o punto de spawn de enemigos extra asignado.");
        }
    }
}
