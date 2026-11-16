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
        float priority = CalculatePriority(coordinates, coordinates, endCoordinates);
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

        //Pillar la mejor casilla que tenga movimiento válido
        while(closed.Count > 0){
            closed.Sort((a, b) => a.priority.CompareTo(b.priority));
            end = closed[0];
            if (end.currentMovementPoints >= 0)
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
    public static float CalculatePriority(HexCoordinates coordinates, HexCoordinates start, HexCoordinates end)
    {
        float priority = 0;

        InfluenceMap influenceMap = new InfluenceMap();
        priority -= influenceMap.GetInfluence(coordinates);

        if(priority == -Mathf.Infinity)
        {
            priority = Mathf.Infinity;
        }

        //Calcular distancia - DIRÍA QUE NO SE PUEDE HACER MAPA DE INFLUENCIAS DE DISTANCIAS A LO A*
        priority += AxialDistance(start, end); //Mayor distancia = peor

        return priority;
    }
    // ================================

    // ================================
    // Calcular A*
    // ================================
    public static void CalculateAStar(HexCoordinates start, HexCoordinates end)
    {
        //Mientras hayan cosas en visiting
        while(visiting.Count > 0){
            //Sacar el nodo más prioritario de visiting (no comprobamos el movimiento, eso se mira más tarde)
            HexNode hex = PopVisiting();

            //Si estamos en end paramos
            if(!hex.SameCoordinates(end)){
                //Obtener sus vecinos y meter los vecinos en visiting
                HexCoordinates[] neighborsCoordinates = GetNeighborsCoordinates(hex);
                foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                {
                    Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                    float newPriority = CalculatePriority(newCoordinate, start, end);
                    int currentMovement = CalculateMovement(hex, newTerrain);
                    HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                    AddToVisiting(newHex);
                }
            }
            else
            {
                //Al llegar al final obtenemos el camino idóneo y bufamos esas casillas y las de alrededor
                while (!hex.SameCoordinates(start))
                {
                    HexNode newHex = hex.parent;
                    newHex.priority -= 2;

                    //Obtener sus vecinos, bufarlos y meterlos en closed
                    HexCoordinates[] neighborsCoordinates = GetNeighborsCoordinates(newHex);
                    foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                    {
                        Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                        float newPriority = CalculatePriority(newCoordinate, start, end);
                        newPriority -= 1;//Bonus extra por estar cerca o en el camino ideal (es uno porque si tiene muchos vecinos puede que aumente demasiado si fuera más de 1)
                        int currentMovement = CalculateMovement(hex, newTerrain);
                        HexNode neighborHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                        AddToClosed(neighborHex);
                    }

                    hex = newHex;
                }
                break;
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
