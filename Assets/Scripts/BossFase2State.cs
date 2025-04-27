using UnityEngine;
using System.Collections;

/// <summary>
/// Estado de la segunda fase del Boss, donde vuela, se mueve y dispara varios patrones.
/// </summary>
public class BossPhase2State : BaseState
{
    private BossFSM _fsmOwner;
    private BossEnemy _bossOwner;
    private GameObject _playerRef;

    private float _timerAtaqueBasico = 0f;
    private float _timerAtaqueEspecial1 = 0f;
    private float _timerAtaqueEspecial2 = 0f;
    private float _timerUltimate = 0f;

    private bool _esperandoAdvertencia = false;

    [Header("Parámetros de Movimiento")]
    public float velocidadMovimiento = 2f;
    public float radioMovimiento = 5f;
    private Vector2 centroMovimiento;
    private float anguloMovimiento;

    public void Initialize(BossFSM ownerFSM, BossEnemy bossOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _fsmOwner = ownerFSM;
        _bossOwner = bossOwner;
        _playerRef = playerRef;

        if (_playerRef != null)
        {
            centroMovimiento = _playerRef.transform.position;
        }
    }

    void Start()
    {
        StateName = "Phase2State";
    }

    public override void OnUpdate()
    {
        if (_playerRef != null)
        {
            MoverseAlrededorDelJugador();
        }

        _timerAtaqueBasico += Time.deltaTime;
        _timerAtaqueEspecial1 += Time.deltaTime;
        _timerAtaqueEspecial2 += Time.deltaTime;
        _timerUltimate += Time.deltaTime;

        if (_timerAtaqueBasico >= 2f)
        {
            _timerAtaqueBasico = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYDispararBalas());
        }

        if (_timerAtaqueEspecial1 >= 6f)
        {
            _timerAtaqueEspecial1 = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYBulletHellDisperso());
        }

        if (_timerAtaqueEspecial2 >= 10f)
        {
            _timerAtaqueEspecial2 = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYRafagaGiratoria());
        }

        if (_timerUltimate >= 18f)
        {
            _timerUltimate = 0f;
            if (!_esperandoAdvertencia)
                OwnerFSMRef.StartCoroutine(MostrarAdvertenciaYLluviaDeBalas());
        }
    }

    private void MoverseAlrededorDelJugador()
    {
        anguloMovimiento += velocidadMovimiento * Time.deltaTime;
        float x = Mathf.Cos(anguloMovimiento) * radioMovimiento;
        float y = Mathf.Sin(anguloMovimiento) * radioMovimiento;

        if (_playerRef != null)
        {
            centroMovimiento = _playerRef.transform.position;
            _bossOwner.transform.position = Vector2.Lerp(
                _bossOwner.transform.position,
                centroMovimiento + new Vector2(x, y),
                Time.deltaTime * velocidadMovimiento
            );
        }
    }

    private IEnumerator MostrarAdvertenciaYDispararBalas()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        DispararRafagaBasica();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private IEnumerator MostrarAdvertenciaYBulletHellDisperso()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        BulletHellDisperso();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private IEnumerator MostrarAdvertenciaYRafagaGiratoria()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        RafagaGiratoria();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private IEnumerator MostrarAdvertenciaYLluviaDeBalas()
    {
        _esperandoAdvertencia = true;
        GameObject icono = GameObject.Instantiate(_bossOwner.iconoAdvertenciaPrefab, _bossOwner.puntoIcono.position, Quaternion.identity, _bossOwner.puntoIcono);
        yield return new WaitForSeconds(1.5f);
        LluviaDeBalas();
        GameObject.Destroy(icono);
        _esperandoAdvertencia = false;
    }

    private void DispararRafagaBasica()
    {
        foreach (Transform punto in _bossOwner.puntosDisparo)
        {
            Instantiate(_bossOwner.balaNormalPrefab, punto.position, Quaternion.identity);
        }
    }

    private void BulletHellDisperso()
    {
        for (int i = 0; i < 30; i++)
        {
            float angulo = i * (360f / 30);
            Vector3 direccion = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0);
            GameObject bala = Instantiate(_bossOwner.balaNormalPrefab, _bossOwner.transform.position, Quaternion.identity);
            bala.transform.right = direccion;
        }
    }

    private void RafagaGiratoria()
    {
        for (int i = 0; i < 20; i++)
        {
            float angulo = i * (360f / 20) + Time.time * 100f;
            Vector3 direccion = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0);
            GameObject bala = Instantiate(_bossOwner.balaNormalPrefab, _bossOwner.transform.position, Quaternion.identity);
            bala.transform.right = direccion;
        }
    }

    private void LluviaDeBalas()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-10f, 10f), 8f, 0f);
            GameObject bala = Instantiate(_bossOwner.balaNormalPrefab, spawnPos, Quaternion.identity);
            bala.transform.right = Vector3.down;
        }
    }
}
