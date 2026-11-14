using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class PathfindingDijkstra : PathfindingManager
{
    // ================================
    // realizar Dijkstra
    // ================================
    public static HexCoordinates GetBestPosition(HexGridManager newGridManager, Troop newTroop)
    {
        //Iniciar el gridManager y tropa
        gridManager = newGridManager;
        troop = newTroop;

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        Terrain terrain = gridManager.GetTerrain(coordinates);
        int priority = CalculatePriority(terrain);
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

        if (CellIsOccupied(end))
        {
            end = start;
        }

        return end.coordinates;
    }
    // ================================

    // ================================
    // Obtener posiciones validas con Dik¡jkstra
    // ================================
    public static List<HexCoordinates> GetValidPositions(HexCoordinates coordinates, HexGridManager newGridManager, Troop newTroop)
    {
        //Iniciar el gridManager y tropa
        gridManager = newGridManager;
        troop = newTroop;

        //Iniciar el primer nodo
        Terrain terrain = gridManager.GetTerrain(coordinates);
        int priority = CalculatePriority(terrain);
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
    public static int CalculatePriority(Terrain terrain)
    {
        int priority = 0;

        //Ver si puede moverse a ese terreno
        foreach(HexTerrainType impTerrain in troop.impassableTerrains)
        {
            if(terrain.Type == impTerrain)
            {
                priority = int.MaxValue;
                return priority;
            }
        }

        //Calcular terreno
        priority += terrain.MovementCost; //Mayor número = peor
        priority -= terrain.DefenseBonus; //Mayor número = mejor

        //Calcular especificaciones de tropa
        foreach(BonusMovementTerrain bonus in troop.movementBonuses)
        {
            if(terrain.Type == bonus.terrain)
            {
                priority += bonus.value; //Mayor número = peor
                break;
            }
        }
        foreach(BonusDefenseTerrain bonus in troop.defenseBonuses)
        {
            if(terrain.Type == bonus.terrain)
            {
                priority += bonus.value; //Mayor número = mejor
                break;
            }
        }

        //Calcular bonos mapas influencia
        //Cuando estén más acabados será sumar y restar priority en la casilla correspondiente

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
                HexCoordinates[] neighborsCoordinates = GetNeighborsCoordinates(hex);
                foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                {
                    Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                    int newPriority = CalculatePriority(newTerrain);
                    int currentMovement = CalculateMovement(hex, newTerrain);
                    HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                    AddToVisiting(newHex);
                }
            }
        }
    }
    // ================================
}
