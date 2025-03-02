using UnityEngine;
using System.Collections.Generic;
//using Unity.Mathematics;

/* 
   Anteriormente usamos "Unity.Mathematics" en el proyecto 3D para utilizar int 2, 
   y as� representar un vector de dos enteros en el entorno 3D
   Como estamos trabajando en un entorno 2D, en su lugar usamos Vector2Int que pertenece a
   "using UnityEngine" por lo que dejamos de necesitar la libreria Mathematics.
   protected int2 beginNodePos = new int2(0, 0); ->  private Vector2Int beginNodePos = new Vector2Int(0, 0);
*/

/*
   En TileGrid2D a diferencia de en TileGrid para 3D, incorporamos sprites en lugar de gizmos,
   se us� BFS (Breadth First Search) en lugar de DFS (Deep First Search).
   Tambi�n se realiz� un cambio y adaptaci�n en la detecci�n de vecinos.
   DFS no garantiza encontrar el camino m�s corto, puede perderse en una direcci�n antes de explorar otra.
   BFS encuentra siempre el camino m�s corto por que explora todos los nodos a la misma profundidad antes de avanzar.
   DFS No almacena nodos cerrados, por lo que puede explorar el mismo nodo varias veces.
   BFS Utiliza un Hashet<Node> para evitar explorar el mismo nodo m�s de una vez.
   DFS se ejecuta en Start()
   BFS se ejecuta en MarkPath()
   EnqueueNode se adapt� en TryAddNode:
   En DFS se llamaba recursivamente, lo que pod�a cusar desbordamiento de pila.
   En BFS se usa queue y se encolan los nodos para ser explorados despu�s.
*/

public class TileGrid2D : MonoBehaviour
{
    [SerializeField] private int width = 5;                                  // Ancho de la cuadr�cula
    [SerializeField] private int height = 5;                                 // Alto de la cuadr�cula
    [SerializeField] private Vector2Int beginNodePos = new Vector2Int(0, 0); // Posici�n inicial del pathfinding
    [SerializeField] private Vector2Int goalNodePos = new Vector2Int(4, 4);  // Posici�n final del pathfinding

    [Header("Tile Prefabs")] // Encabezados para mostrar y dar informaci�n en el editor
    [SerializeField] private GameObject walkableTilePrefab; // Prefab para tiles transitables
    [SerializeField] private GameObject obstacleTilePrefab; // Prefab para tiles que bloquean el paso
    [SerializeField] private GameObject startTilePrefab;    // Prefab para la posici�n inicial
    [SerializeField] private GameObject goalTilePrefab;     // Prefab para la posici�n objetivo
    [SerializeField] private GameObject pathTilePrefab;     // Prefab para mostrar la ruta encontrada

    [Header("Parent for Tiles")]
    [SerializeField] private Transform gridParent;   // Contenedor de los tiles en la jerarqu�a

    private Node[][] nodeGrid;                       // Matriz de nodos para representar la cuadr�cula
    private List<Node> pathNodes = new List<Node>(); // Lista de nodos que forman el camino encontrado

    void Start()
    {
        // hay que verificar que beginNodePos y goalNodePos sean v�lidas respecto a nuestro width y height.
        if (goalNodePos.x < 0 || goalNodePos.y < 0 || goalNodePos.x >= width || goalNodePos.y >= height)
        {
            Debug.LogError("La posici�n del nodo objetivo est� fuera de los l�mites.");
            return;
        }

        // Inicializaci�n de la malla
        InitializeGrid();  
        GenerateVisualGrid();

        Debug.Log("antes de llamar a DFS recursivo");

        // Obtener nodos de inicio y objetivo
        Node beginNode = nodeGrid[beginNodePos.y][beginNodePos.x];
        Node goalNode = nodeGrid[goalNodePos.y][goalNodePos.x];

        // Asegurar que estos nodos sean transitables
        beginNode.isWalkable = true;
        goalNode.isWalkable = true;

        // IMPORTANTE: le ponemos al parent de nodeGrid que es igual a �l mismo, porque
        // si no, su padre es null y entonces hace un paso equivocado en el pathfinding.
        beginNode.parentRef = beginNode; // Marcar el nodo de inicio como su propio padre

        /*
           Notas sobre BFS Y DFS
           BFS : B�squeda de Anchura, su estructura de datos es en cola (Queue), explora primero los vecinos m�s cercanos,
           siempre encuentra el camino m�s corto (en cantidad de nodos), su uso principal se da en B�squeda de caminos �ptimos
           DFS : B�squeda de Profundidad, Su estructura de datos es en Pila (Stack) o recursi�n, explora primero lo m�s profundo,
           no garantiza el camino m�s corto, su uso principal se da en exploraci�n y resoluci�n de laberintos.
        */

// Ejecutar el algoritmo de b�squeda en anchura (BFS)
bool BFSResult = BreadthFirstSearch(beginNode, goalNode);
        if (BFSResult)
        {
            Debug.Log("�S� hubo camino!");
            MarkPath(goalNode);
        }
        else
        {
            Debug.Log("No hubo camino");
        }
    }

    void InitializeGrid()
    {
        // hay que pedir memoria para nuestro nodeGrid. 
        nodeGrid = new Node[height][];
        for (int y = 0; y < height; y++)
        {
            nodeGrid[y] = new Node[width];       // pedimos memoria para toda la fila
            for (int x = 0; x < width; x++)
            {
                nodeGrid[y][x] = new Node(x, y);

                // Ponemos randoms de caminable o no.
                // Se evita que el nodo de inicio o el de objetivo sean obst�culos
                if (Random.value < 0.2f && new Vector2Int(x, y) != beginNodePos && new Vector2Int(x, y) != goalNodePos)
                {
                    nodeGrid[y][x].isWalkable = false;
                    Debug.Log($"Obst�culo en {x}, {y}");
                }
                else
                {
                    nodeGrid[y][x].isWalkable = true;
                    Debug.Log($"Nodo Caminable en {x}, {y}");
                }
            }
        }

        // Asegurar que los nodos de inicio y final sean transitables
        nodeGrid[beginNodePos.y][beginNodePos.x].isWalkable = true;
        nodeGrid[goalNodePos.y][goalNodePos.x].isWalkable = true;
    }

    void GenerateVisualGrid()
    {
        float spacing = 55f; // Separaci�n entre tiles

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject prefabToInstantiate = nodeGrid[y][x].isWalkable ? walkableTilePrefab : obstacleTilePrefab;

                if (new Vector2Int(x, y) == beginNodePos)
                {
                    prefabToInstantiate = startTilePrefab;
                    Debug.Log($"Nodo Inicio en {x}, {y}");
                }
                else if (new Vector2Int(x, y) == goalNodePos)
                {
                    prefabToInstantiate = goalTilePrefab;
                    Debug.Log($"Nodo Final en {x}, {y}");
                }

                Vector3 position = new Vector3(x * spacing, y * spacing, 0);
                GameObject tile = Instantiate(prefabToInstantiate, position, Quaternion.identity, gridParent);

                // Asegurar que todos los tiles tienen el mismo tama�o de 50 unidades
                tile.transform.localScale = Vector3.one * (1f * tile.GetComponent<SpriteRenderer>().bounds.size.x);
            }
        }
    }

    bool BreadthFirstSearch(Node origin, Node goal)
    {
        Queue<Node> openNodes = new Queue<Node>();
        HashSet<Node> closedNodes = new HashSet<Node>();
        openNodes.Enqueue(origin);
        origin.parentRef = origin;

        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes.Dequeue();

            if (currentNode == goal)
            {
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }

            int x = currentNode.x;
            int y = currentNode.y;

            if (y < height - 1) TryAddNode(nodeGrid[y + 1][x], currentNode, ref openNodes, ref closedNodes);
            if (x < width - 1) TryAddNode(nodeGrid[y][x + 1], currentNode, ref openNodes, ref closedNodes);
            if (y > 0) TryAddNode(nodeGrid[y - 1][x], currentNode, ref openNodes, ref closedNodes);
            if (x > 0) TryAddNode(nodeGrid[y][x - 1], currentNode, ref openNodes, ref closedNodes);
        }
        // si no se pudo encolar ni encontr� el camino, retorna falso.
        return false;
    }

    bool TryAddNode(Node enqueuedNode, Node currentNode, ref Queue<Node> openNodes, ref HashSet<Node> closedNodes)
    {
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable && !closedNodes.Contains(enqueuedNode))
        {
            enqueuedNode.parentRef = currentNode;
            openNodes.Enqueue(enqueuedNode);
            closedNodes.Add(enqueuedNode);
            return true;
        }
        return false;
    }

    void MarkPath(Node goalNode)
    {
        Node current = goalNode;
        float spacing = 55f; // Espaciado igual al de los tiles

        while (current.parentRef != current)
        {
            Vector3 position = new Vector3(current.x * spacing, current.y * spacing, 0);

            // Si es el nodo final, no lo reemplazamos
            if (current.x == goalNodePos.x && current.y == goalNodePos.y)
            {
                Debug.Log($"El nodo final en {current.x}, {current.y} se mantiene visible y no es reemplazado.");
            }
            else
            {
                // Eliminar cualquier tile existente en la posici�n antes de colocar el camino
                foreach (Transform child in gridParent)
                {
                    if (child.position == position)
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }

                // Instanciar el tile del camino
                Instantiate(pathTilePrefab, position, Quaternion.identity, gridParent);
            }

            Debug.Log($"Nodo en camino: {current.x}, {current.y} -> Posici�n real: {position}");
            current = current.parentRef;
        }
    }
}
