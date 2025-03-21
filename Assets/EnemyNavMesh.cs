using UnityEngine;
using UnityEngine.AI;

// El due�o de este script usa el navMesh para moverse a su destino 
// El cual se asigna en el editor y en este caso el asignado es el jugador

public class EnemyNavMesh : MonoBehaviour
{
    public Transform objetivo;
    private NavMeshAgent agent;

    private void Awake()
    {
        // Obtiene la referencia del NavMeshAgent que est� en el mismo objeto que el script.
        // Esencial para que el enemigo pueda moverse autom�ticamente.
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.updateRotation = false;            //Evita que el NavMeshAgent rote autom�ticamente.
                agent.updateUpAxis = false;      // En juegos 2D, esto evita que el enemigo rote de forma incorrecta.
    }

    private void Update()
    {
        // En cada frame, el enemigo actualiza su destino para seguir al objetivo.
        agent.SetDestination(objetivo.position);
    }
}
