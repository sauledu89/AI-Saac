using UnityEngine;

/// <summary>
/// Estado de Idle del Boss.
/// El jefe flota ligeramente en el lugar hasta que entra en combate.
/// </summary>
public class BossIdleState : BaseState
{
    private BossFSM _fsmOwner;
    private BossEnemy _bossOwner;
    private GameObject _playerRef;

    private Vector3 _posicionInicial;
    private float _frecuenciaFlotacion = 1f; // Qu� tan r�pido flota (velocidad de la onda seno)
    private float _amplitudFlotacion = 1f;   // Qu� tanto se mueve arriba y abajo

    private float _tiempoPasado = 0f; // Para calcular la onda
    private float _tiempoAcumulado = 0f; // Acumulador de tiempo para salir del Idle
    private float _tiempoEspera = 3f;    // Tiempo en segundos antes de cambiar autom�ticamente a Phase 1

    public void Initialize(BossFSM ownerFSM, BossEnemy bossOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _fsmOwner = ownerFSM;
        _bossOwner = bossOwner;
        _playerRef = playerRef;
    }

    void Start()
    {
        StateName = "IdleState";
        _posicionInicial = transform.position; // Guardamos el punto de inicio para oscilar alrededor
    }

    public override void OnUpdate()
    {
        // Movimiento de "flotaci�n" tipo globo aerost�tico 
        _tiempoPasado += Time.deltaTime;
        float desplazamientoVertical = Mathf.Sin(_tiempoPasado * _frecuenciaFlotacion) * _amplitudFlotacion;
        transform.position = new Vector3(_posicionInicial.x, _posicionInicial.y + desplazamientoVertical, _posicionInicial.z);

        // --- Nuevo sistema de espera autom�tica ---
        _tiempoAcumulado += Time.deltaTime;
        if (_tiempoAcumulado >= _tiempoEspera)
        {
            Debug.Log("Tiempo de espera cumplido, iniciando fase 1.");
            OwnerFSMRef.ChangeState(_fsmOwner.GetPhase1State());
            return; // �Siempre hacemos return despu�s de cambiar de estado!
        }

        // --- (Opcional) Tambi�n puede salir si el jugador entra en rango ---
        if (_playerRef != null)
        {
            float distanciaJugador = Vector2.Distance(transform.position, _playerRef.transform.position);

            if (distanciaJugador <= 12f)
            {
                Debug.Log("El jugador ha entrado al rango, iniciando fase 1.");
                OwnerFSMRef.ChangeState(_fsmOwner.GetPhase1State());
                return;
            }
        }
    }
}
