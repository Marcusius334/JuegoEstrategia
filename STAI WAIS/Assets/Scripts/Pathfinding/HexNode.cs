using JetBrains.Annotations;
using UnityEngine;

public class HexNode
{
    public HexCoordinates coordinates;//coordenadas de la casilla
  
    public HexNode parent;//CAsilla anterior de la ruta

    //coste acumulado desde el origen(g)
    public float costSoFar;
    //Estimacion del coste hasta el objetivo(h)
    public float heuristic;
    //prioridad total(float= global+h)
    public float priority;
    
    // PM consumidos desde el origen
    public int movementSpent;
    //PM restantes respecto al turno actual
    public int currentMovementPoints;//Movimiento que le queda a la tropa

    //CONSTRUCTOR
    public HexNode(HexCoordinates coordinates, float costSoFar, float heuristic, float priority, int movementSpent, int currentMovementPoints, HexNode parent)
    {
        this.coordinates = coordinates;
        this.costSoFar = costSoFar;
        this.heuristic = heuristic;
        this.priority = priority;
        this.movementSpent = movementSpent;
        this.currentMovementPoints = currentMovementPoints;
        this.parent = parent;
    }

    // Constructor antiguo, para mantener temporalmente
    // la compatibilidad con los algoritmos actuales.
    public HexNode(
        HexCoordinates coordinates,
        float priority,
        int currentMovementPoints,
        HexNode parent)
    {
        this.coordinates = coordinates;
        this.costSoFar = priority;
        this.heuristic = 0;
        this.priority = priority;
        this.movementSpent = 0;
        this.currentMovementPoints = currentMovementPoints;
        this.parent = parent;
    }

    public bool SameCoordinates(HexCoordinates other)
    {
        return coordinates.Q == other.Q &&
           coordinates.R == other.R;
    }
    public bool SameCoordinates(HexNode other)
    {
        if(other == null) return false;

        return coordinates.Q == other.coordinates.Q && coordinates.R == other.coordinates.R;
    }
    public void UpdateCosts(float newCost, float newHeuristic)
    {
        costSoFar = newCost;
        heuristic = newHeuristic;
        priority = costSoFar + heuristic;
    }

}
