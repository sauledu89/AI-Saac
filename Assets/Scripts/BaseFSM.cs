using UnityEngine;

/// <summary>
/// Clase base para una Máquina de Estados Finita (FSM).
/// </summary>
public class BaseFSM : MonoBehaviour
{
    private BaseState _currentState = null;

    protected void Update()
    {
        if (_currentState == null)
            return;

        _currentState.OnUpdate();
    }

    protected virtual void Initialize()
    {
        // Aquí, las clases hijas inicializan sus estados
    }

    private void Start()
    {
        Initialize(); // Primero inicializar los estados.

        _currentState = GetInitialState(); // Luego pedir el estado inicial.

        if (_currentState == null)
        {
            Debug.LogError("_currentState es null, ¿olvidaste sobreescribir GetInitialState en esta clase hija de BaseFSM? Saliendo de la función.");
            enabled = false;
            return;
        }

        _currentState.OnEnter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }

    public void ChangeState(BaseState newState)
    {
        if (_currentState != null)
            _currentState.OnExit();

        _currentState = newState;

        if (_currentState != null)
            _currentState.OnEnter();
    }
}
