using System.Collections.Generic;
using UnityEngine;

public class PathfindingAStar : PathfindingManager
{
    // ================================
    // Realizar A*
    // ================================
    public static HexCoordinates GetBestPosition(HexGridManager newGridManager, Troop newTroop, HexCoordinates endCoordinates)
    {
        //Iniciar el gridManager y tropa
        gridManager = newGridManager;
        troop = newTroop;

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        Terrain terrain = gridManager.GetTerrain(coordinates);
        int priority = CalculatePriority(terrain, coordinates, endCoordinates);
        HexNode start = new HexNode(coordinates, priority, troop.velocity, null);
        AddToVisiting(start);

        CalculateAStar(coordinates, endCoordinates);

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

        //Pillar la mejor casilla de closed que se pueda alcanzar con Dijkstra
        List<HexCoordinates> valid = PathfindingDijkstra.GetValidPositions(gridManager, troop);
        while(closed.Count > 0){
            closed.Sort((a, b) => a.priority.CompareTo(b.priority));
            end = closed[0];
            if (valid.Contains(end.coordinates))
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
    // Obtener la prioidad de una casilla
    // ================================
    public static int CalculatePriority(Terrain terrain, HexCoordinates start, HexCoordinates end)
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

        //SE PODRÍA PONER UN MAPA DE INFLUENCIAS DE VALOR ESTRATÉTICO ENEMIGO EN FUNCIÓN DE LAS TROPAS DEL ENEMIGO
        //A LO QUE HICIMOS EN CLASE DE LAS TROPAS BLANCAS Y NEGRAS

        //Calcular terreno - PUEDE SUSTITUIRSE POR MAPA DE INFLUENCIAS DE TERRENO
        priority += terrain.MovementCost; //Mayor número = peor
        priority -= terrain.DefenseBonus; //Mayor número = mejor

        //Calcular especificaciones de tropa - TENDRÍA QUE HACERSE UN MAPA DE INFLUENCIAS POR TROPA (NO PARECE FACTIBLE)
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

        //Calcular distancia - DIRÍA QUE NO SE PUEDE HACER MAPA DE INFLUENCIAS DE DISTANCIAS A LO A*
        priority += AxialDistance(start, end); //Mayor distancia = peor

        return priority;
    }
    // ================================

    // ================================
    // Calcular Dijkstra
    // ================================
    public static void CalculateAStar(HexCoordinates start, HexCoordinates end)
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
                    int newPriority = CalculatePriority(newTerrain, start, end);
                    int currentMovement = CalculateMovement(hex, newTerrain);
                    HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                    AddToVisiting(newHex);
                }
            }
        }
    }
    // ================================

    // ================================
    // Calcula la distancia entre dos hexágonos usando coordenadas axiales
    // ================================
    protected static int AxialDistance(HexCoordinates start, HexCoordinates end)
    {
        return start.DistanceTo(end);
    }
    // ================================

}
