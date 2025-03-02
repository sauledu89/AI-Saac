using UnityEngine;

// Node, es gestionado por el script TileGrid2D
// Por lo tanto, no necesita ser asignado a un GameObject 

public class Node
{
    public int x, y;                   // Coordenadas del nodo en la cuadrícula
    public bool isWalkable;            // Se puede caminar sobre este nodo?
    public bool partOfRoute = false;   // Es parte del camino encontrado?
    public Node parentRef;             // Referencia al nodo padre para reconstrucción del camino

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWalkable = true;             // Por defecto, todos los nodos son caminables
        parentRef = null;              // Se inicializa sin padre
    }
}


/*
   Igual que el nodo del proyecto 3D

   Node : Proyecto 3D
 
public class Node
{
    public int x, y;
    public bool isWalkable;
    public bool partOfRoute = false;
    public Node parentRef;

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWalkable = true;
        parentRef = null;
    }
}

*/ 