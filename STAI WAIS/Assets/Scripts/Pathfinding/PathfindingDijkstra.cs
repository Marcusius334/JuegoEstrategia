using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class PathfindingDijkstra : PathfindingManager
{
    // ================================
    // Realizar Dijkstra
    // ================================
    public static HexCoordinates GetBestPosition(HexGridManager newGridManager, Troop newTroop, TacticalInfluenceManager newInfluenceMap)
    {
        //Iniciar el gridManager, tropa y el influence map
        gridManager = newGridManager;
        troop = newTroop;
        influenceMap = newInfluenceMap;

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        Terrain terrain = gridManager.GetTerrain(coordinates);
        float priority = CalculatePriority(coordinates);
        HexNode start = new HexNode(coordinates, priority, troop.velocity, null);
        AddToVisiting(start);

        CalculateDijkstra();

        //Sortear la lista closed para obtener la mejor casilla que no esté ocupada
        HexNode end = null;
        while(closed.Count > 0){
            closed.Sort((a, b) => a.priority.CompareTo(b.priority));
            end = closed[0];
            if (!CellIsOccupied(end))
            {
                break;
            }
            closed.RemoveAt(0);
        }

        if (CellIsOccupied(end) || end.priority == Mathf.Infinity)
        {
            end = start;
        }

        return end.coordinates;
    }
    // ================================

    // ================================
    // Obtener posiciones validas con Dijkstra
    // ================================
    public static List<HexCoordinates> GetValidPositions(HexGridManager newGridManager, Troop newTroop, TacticalInfluenceManager newInfluenceMap)
    {
        //Iniciar el gridManager, tropa y el influence map
        gridManager = newGridManager;
        troop = newTroop;
        influenceMap = newInfluenceMap;

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        Terrain terrain = gridManager.GetTerrain(coordinates);
        float priority = CalculatePriority(coordinates);
        HexNode start = new HexNode(coordinates, priority, troop.velocity, null);
        AddToVisiting(start);

        CalculateDijkstra();

        List<HexCoordinates> validpositions = new List<HexCoordinates> {};
        foreach(HexNode hex in closed)
        {
            if (!CellIsOccupied(hex))
            {
                validpositions.Add(hex.coordinates);
            }
        }
        return validpositions;
    }
    // ================================

    // ================================
    // Obtener la prioidad de una casilla
    // ================================
    public static float CalculatePriority(HexCoordinates coordinates)
    {
        float priority = 0;

        priority -= influenceMap.GetTroopInfluence(gridManager, coordinates, troop);

        if(priority == -Mathf.Infinity)
        {
            priority = Mathf.Infinity;
        }

        return priority;
    }
    // ================================

    // ================================
    // Calcular Dijkstra
    // ================================
    public static void CalculateDijkstra()
    {
        //Mientras hayan cosas en visiting
        while(visiting.Count > 0){
            //Sacar el nodo más prioritario de visiting y comprobar si le queda movimiento
            HexNode hex = PopVisiting();

            if(hex.currentMovementPoints > 0){
                //Obtener sus vecinos y meter los vecinos en visiting
                List<HexCoordinates> neighborsCoordinates = GetNeighborsCoordinates(hex);
                foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                {
                    if(gridManager.CoordinateInWorld(newCoordinate)){
                        Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                        float newPriority = CalculatePriority(newCoordinate);
                        int currentMovement = CalculateMovement(hex, newTerrain);
                        HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                        AddToVisiting(newHex);
                    }
                }
            }
        }
    }
    // ================================
}
