using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/*
   El TileGrid hecho en clase  
 
public class Node
{
    public Node()
    {
        parentRef = null;
    }

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWalkable = true;
        parentRef = null;
    }

    // Qu� necesitan tener nuestros nodos.
    // saber su posici�n o coordenadas X Y, estos valores tambi�n van a ser su ID.
    public int x;
    public int y;
    public bool isWalkable;
    public bool partOfRoute = false;

    // la referencia al nodo padre en el �rbol generado durante el proceso de pathfinding.
    public Node parentRef;

    // Saber qui�nes son sus vecinos (aristas hacia vecinos)
    // por simplicidad vamos que sus nodos de izquierda, derecha, arriba y abajo son sus vecinos.
    /* Esto de tener una referencia por vecino no lo vamos a hacer en este caso de la cuadr�cula, 
     * porque tomar�a muchos recursos y lo podemos sustituir a trav�s de sumar y restar posiciones en el array de la cuadr�cula.
     * Esta ventaja tiene el costo de tener que checar que no nos salgamos de la cuadr�cula 
     * (tener cuidado con las posiciones 0s en X y Y, y en las del final del array en X y Y).
     * 
     * public Node up;
    public Node right;
    public Node left;
    public Node down;
    // teniendo lo anterior en cuenta, sus vecinos est�n impl�citos en la posici�n de cada nodo.
    // Arriba: [x][y-1]
    // Abajo: [x][y+1]
    // Derecha: [x+1][y]
    // Izquierda: [x-1][y]

    // ejemplo, posici�n X2, Y2
    // tu vecino de arriba cu�l es? le sumas uno a la coordenada en Y
    // cu�l ser�a tu vecino de abajo? pues le restas 1 en Y
    // para la derecha? sumar 1 en X
    // para la izquierda? restar 1 en X

    // Ejemplo #2: posici�n X0, Y0
    // si nos intentamos ir hacia la izquierda o hacia arriba, estar�amos yendo de 0 a -1, la cual no es
    // una posici�n v�lida en un array, y eso ser�a un error de access violation (es un error muy grave).



    // m�s tarde: peso de este nodo.

}

public class TileGrid : MonoBehaviour
{
    // va a tener una cuadr�cula de width*height nodos 
    [SerializeField]
    protected int width = 5;
    [SerializeField]
    protected int height = 5;


    [SerializeField]
    protected int2 beginNodePos = new int2(0, 0);

    [SerializeField]
    protected int2 goalNodePos = new int2(1, 1);


    // es mejor que el primer [] sean las Y, y el segundo [] sean las X.
    // esto es mejor para el performance porque permite acceso secuencial a la memoria.
    protected Node[][] nodeGrid;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // hay que verificar que beginNodePos y goalNodePos sean v�lidas respecto a nuestro width y height.
        if (beginNodePos.x < 0 || beginNodePos.y < 0 || beginNodePos.x >= width || beginNodePos.y >= height)
        {
            Debug.LogError("posici�n de beginNodePos es inv�lido porque no est� dentro de los l�mites del nodeGrid");
            return; // nos salimos de la funci�n porque si no ejecutar�a lo de m�s abajo y tronar�a.
        }
        if (goalNodePos.x < 0 || goalNodePos.y < 0 || goalNodePos.x >= width || goalNodePos.y >= height)
        {
            Debug.LogError("posici�n de goalNodePos es inv�lido porque no est� dentro de los l�mites del nodeGrid");
            return; // nos salimos de la funci�n porque si no ejecutar�a lo de m�s abajo y tronar�a.
        }

        InitializeGrid();

        Debug.Log("antes de llamar a DFS recursivo");

        Node beginNode = nodeGrid[beginNodePos.y][beginNodePos.x];
        beginNode.isWalkable = true;
        Node goalNode = nodeGrid[goalNodePos.y][goalNodePos.x];
        goalNode.isWalkable = true;

        // IMPORTANTE: le ponemos al parent de nodeGrid que es igual a �l mismo, porque
        // si no, su padre es null y entonces hace un paso equivocado en el pathfinding.
        beginNode.parentRef = beginNode;

        // bool DFSResult = DepthFirstSearchRecursive(beginNode, goalNode);
        bool DFSResult = DepthFirstSearch(beginNode, goalNode);

        if (DFSResult)
        {
            Debug.Log("s� hubo camino");

            Node backtrackingNode = goalNode;
            // haciendo backtracking:
            while (backtrackingNode.parentRef != backtrackingNode)
            {
                Debug.Log($"el nodo X{backtrackingNode.x}, Y{backtrackingNode.y} fue parte del verdadero camino.");
                backtrackingNode = backtrackingNode.parentRef;
            }
        }
        else
        {
            Debug.Log("NO hubo camino");
        }


    }

    void InitializeGrid()
    {
        // hay que pedir memoria para nuestro nodeGrid.
        nodeGrid = new Node[height][];

        for (int y = 0; y < height; y++)
        {
            nodeGrid[y] = new Node[width]; // pedimos memoria para toda la fila

            for (int x = 0; x < width; x++)
            {
                nodeGrid[y][x] = new Node(x, y);
                // Ponemos randoms de caminable o no.
                float rand = UnityEngine.Random.Range(0, 1.0f);
                if (rand < 0.3f)
                {
                    nodeGrid[y][x].isWalkable = false;
                }
            }
        }

        Debug.Log("node grid inicializado");
    }

    bool EnqueueNodeRecursive(Node enqueuedNode, Node currentNode, Node goalNode)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            Debug.Log($" El nodo X{enqueuedNode.x} Y{enqueuedNode.y} ya est� siendo abierto/conocido.");

            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // entonces s� podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(enqueuedNode, goalNode);
            if (dfsResult)
            {
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }
        }

        // si no se pudo encolar ni encontr� el camino, retorna falso.
        return false;
    }

    // Esta funci�n NO es recursiva.
    bool EnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref Stack<Node> openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            Debug.Log($" El nodo X{enqueuedNode.x} Y{enqueuedNode.y} ya est� siendo abierto/conocido.");

            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // entonces s� podemos checar a este vecino.
            // en vez de la recursi�n, tenemos la pila/stack.
            openNodes.Push(enqueuedNode);
            return true;
        }

        // si no se pudo encolar ni encontr� el camino, retorna falso.
        return false;
    }


    // NOTA GRAN NOTA: Seg�n yo se necesita que chequemos y asignar el parent antes de mandar DFS otra vez, porque 
    // si no se cicla infinitamente.
    bool DepthFirstSearchRecursive(Node currentNode, Node goalNode)
    {
        // checamos si ya llegamos a la meta.
        if (currentNode == goalNode)
        {
            Debug.Log("s� hubo camino");
            // aqu� empezar�amos el backtracking (fin exitoso del la recursi�n)
            return true; // regresamos true porque s� llegamos a la meta. Estamos parados en la meta actualmente.
        }

        // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
        int x = currentNode.x;
        int y = currentNode.y;
        // checamos los 4 vecinos.
        // VECINO DE ARRIBA (y-1)
        // primero tenemos que checar que y-1 sea una posici�n v�lida en el array. 
        // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o m�s, entonces va a ser 0 o m�s.
        if (y < height - 1)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y + 1][x], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y + 1][x].partOfRoute = true;
                    Debug.Log($"s� hubo camino, y el nodo: X{x}, Y{y + 1} fue parte del camino");
                    return true;
                }
        }

        // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
        // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
        // sea de -2 que el l�mite de nuestro arreglo.

        // VECINO DERECHA
        if (x < width - 1)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y][x + 1], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y][x + 1].partOfRoute = true;
                    Debug.Log($"s� hubo camino, y el nodo: X{x + 1}, Y{y}, fue parte del camino");
                    return true;
                }
        }

        // VECINO DE ABAJO (y+1)
        if (y > 0)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y - 1][x], currentNode, goalNode);
            if (dfsResult)
            {
                nodeGrid[y - 1][x].partOfRoute = true;
                Debug.Log($"s� hubo camino, y el nodo: X{x}, Y{y - 1} fue parte del camino");
                return true;
            }
        }

        // VECINO IZQUIERDA
        if (x > 0)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y][x - 1], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y][x - 1].partOfRoute = true;
                    Debug.Log($"s� hubo camino, y el nodo: X{x - 1}, Y{y} fue parte del camino");
                    return true;
                }
        }

        Debug.Log($" El nodo X{x} Y{y} ya est� cerrado.");

        // en este camino (ninguno de sus hijos) no se encontr� el goal, as� que vamos hacia atr�s/arriba.
        return false;

    }

    bool DepthFirstSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // La primera condici�n de terminaci�n de nuestro ciclo es:
        // si ya llegu� a la meta, termino y retorno verdadero de que s� llegu� a la meta.

        // la otra condici�n de terminaci�n del ciclo es:
        // si ya no hay acciones por realizar, es decir: si ya no hay m�s nodos abiertos que visitar.
        // vamos a guardar nuestros nodos abiertos en una Stack (Pila).
        Stack<Node> openNodes = new Stack<Node>();
        // los nodos que ya no les que
        // Es decir, cuando sacas un nodo de la openStack lo pasas a los nodos cerrados.
        HashSet<Node> closedNodes = new HashSet<Node>();

        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Push(origin);

        Node currentNode = null;

        while (currentNode != goal && openNodes.Count > 0)
        {
            // current va a ser el nodo que est� hasta arriba de la pila en este momento.
            currentNode = openNodes.Peek();

            // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
            int x = currentNode.x;
            int y = currentNode.y;
            // checamos los 4 vecinos.
            // VECINO DE ARRIBA (y-1)
            // primero tenemos que checar que y-1 sea una posici�n v�lida en el array. 
            // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o m�s, entonces va a ser 0 o m�s.
            if (y < height - 1)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si s� se meti� un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
            // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
            // sea de -2 que el l�mite de nuestro arreglo.

            // VECINO DERECHA
            if (x < width - 1)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si s� se meti� un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // VECINO DE ABAJO (y+1)
            if (y > 0)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si s� se meti� un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // VECINO IZQUIERDA
            if (x > 0)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si s� se meti� un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            Debug.Log($" El nodo X{x} Y{y} ya est� cerrado.");

            // Cuando ya llegamos aqu� es que el currentNode ya no tiene m�s acciones disponibles
            // entonces pasa a estar cerrado
            Node closedNode = openNodes.Pop();
            closedNodes.Add(closedNode);  // este nodo cerrado ya nunca se tiene que modificar.
        }

        if (currentNode == goal)
        {
            Debug.Log("S� hubo camino de manera iterativa");
            return true;
        }
        else
        {
            Debug.Log("NO hubo camino de manera iterativa");
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        // tenemos que checar que ya hayamos pedido memoria para la cuadr�cula de nodos.
        // si no, entonces es null y nos salimos de esta funci�n.
        if (nodeGrid == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (nodeGrid[y][x].isWalkable)
                {
                    if (nodeGrid[y][x].partOfRoute == true)
                    {
                        Gizmos.DrawSphere(new Vector3(x, y, 0.0f), 0.5f);
                    }
                    Gizmos.DrawCube(new Vector3(x, y, 0.0f), Vector3.one * 0.5f);

                    // ahora dibujemos una l�nea de padre a hijo.
                    if (nodeGrid[y][x].parentRef != null)
                    {
                        Vector3 parentPos = new Vector3(nodeGrid[y][x].parentRef.x, nodeGrid[y][x].parentRef.y, 0);
                        Vector3 currentPos = new Vector3(x, y, 0.0f);
                        Gizmos.DrawLine(parentPos, currentPos);
                    }
                }
                else
                {
                    // si no es caminable lo dibujamos como una esfera.
                    Gizmos.DrawWireSphere(new Vector3(x, y, 0.0f), 0.5f);
                }
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(beginNodePos.x, beginNodePos.y, 0.0f), 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(goalNodePos.x, goalNodePos.y, 0.0f), 0.5f);

    }
}

*/