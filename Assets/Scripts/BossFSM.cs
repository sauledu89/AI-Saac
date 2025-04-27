using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Máquina de Estados Finita (FSM) que controla el flujo entre la Fase 1 y la Fase 2 del Boss.
/// </summary>
public class BossFSM : BaseFSM
{
    [Header("Referencias necesarias")]
    [SerializeField] private GameObject playerRef;
    [SerializeField] private BossEnemy bossOwner;

    private BossIdleState idleState;
    private BossPhase1State phase1State;
    private BossPhase2State phase2State;

    public BossIdleState GetIdleState() => idleState;
    public BossPhase1State GetPhase1State() => phase1State;
    public BossPhase2State GetPhase2State() => phase2State;

    protected override void Initialize()
    {
        idleState = gameObject.AddComponent<BossIdleState>();
        idleState.Initialize(this, bossOwner, playerRef);

        phase1State = gameObject.AddComponent<BossPhase1State>();
        phase1State.Initialize(this, bossOwner, playerRef);

        phase2State = gameObject.AddComponent<BossPhase2State>();
        phase2State.Initialize(this, bossOwner, playerRef);
    }

    protected override BaseState GetInitialState()
    {
        return idleState;
    }

// Método privado para obtener el estado actual
private BaseState GetCurrentState()
    {
        var field = typeof(BaseFSM).GetField("_currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (BaseState)field.GetValue(this);
    }
}
