using System.Collections.Generic;
using UnityEngine;

public class PathfindingAStar : PathfindingManager
{
    // ================================
    // Realizar A*
    // ================================
    public static HexCoordinates GetBestPosition(HexGridManager newGridManager, Troop newTroop, HexCoordinates endCoordinates, TacticalInfluenceManager newInfluenceMap)
    {
        //Iniciar el gridManager, tropa y el influence map
        gridManager = newGridManager;
        troop = newTroop;
        influenceMap = newInfluenceMap;

        // Limpiamos las listas estáticas para no mezclar búsquedas anteriores
        visiting.Clear();
        closed.Clear();

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        Terrain terrain = gridManager.GetTerrain(coordinates);
        float priority = CalculatePriority(coordinates, coordinates, endCoordinates);
        HexNode start = new HexNode(coordinates, priority, troop.velocity, null);
        AddToVisiting(start);

        CalculateAStar(coordinates, endCoordinates);

        //Sortear la lista closed para obtener la mejor casilla que no esté ocupada
        //Pillar la mejor casilla que tenga movimiento válido
        // Unificamos ambos filtros para elegir la casilla más prioritaria que esté libre y alcanzable este turno
        HexNode end = null;
        closed.Sort((a, b) => a.priority.CompareTo(b.priority));

        foreach (HexNode node in closed)
        {
            if (node == null) continue;
            if (node.SameCoordinates(coordinates)) continue;

            if (!CellIsOccupied(node) && node.currentMovementPoints >= 0)
            {
                end = node;
                break;
            }
        }

        if (end == null || CellIsOccupied(end) || end.priority == Mathf.Infinity)
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

        // Comprobamos que influenceMap exista antes de consultar la influencia
        if (influenceMap != null)
        {
            priority -= influenceMap.GetTroopInfluence(gridManager, coordinates, troop);
        }

        if(priority == -Mathf.Infinity)
        {
            priority = Mathf.Infinity;
        }

        //Calcular distancia - DIRÍA QUE NO SE PUEDE HACER MAPA DE INFLUENCIAS DE DISTANCIAS A LO A*
        // Usamos coordinates (la casilla evaluada) hasta end para que la heurística A* guíe hacia el objetivo
        priority += AxialDistance(coordinates, end) * 10f; //Mayor distancia = peor

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

            Terrain currentTerrain = gridManager.GetTerrain(hex.coordinates);

            if (currentTerrain != null && currentTerrain.Type == HexTerrainType.TerrenoAgujeroDeGusano)
            {
                HexCell agujeroDeGusano1 = gridManager.GetCell(hex.coordinates);
                // Evitamos error si el agujero de gusano aún no tiene destino conectado
                if (agujeroDeGusano1 == null || agujeroDeGusano1.destino == null) continue;
                HexCell agujeroDeGusano2 = agujeroDeGusano1.destino;


                HexNode hexGusano = new HexNode(agujeroDeGusano2.Coordinates, Mathf.Infinity, 0, hex);
                List<HexCoordinates> neighborsCoordinates = GetNeighborsCoordinates(hexGusano);
                foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                {
                    if(gridManager.CoordinateInWorld(newCoordinate)){
                        Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                        float newPriority = CalculatePriority(newCoordinate, start, end);
                        int currentMovement = 0;
                        HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hexGusano);
                        AddToVisiting(newHex);
                    }
                }
            }
            else
            //Si estamos en end paramos
            if(!hex.SameCoordinates(end)){
                //Obtener sus vecinos y meter los vecinos en visiting
                List<HexCoordinates> neighborsCoordinates = GetNeighborsCoordinates(hex);
                foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                {
                    if(gridManager.CoordinateInWorld(newCoordinate)){
                        Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                        float newPriority = CalculatePriority(newCoordinate, start, end);
                        int currentMovement = CalculateMovement(hex, newTerrain);
                        HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                        AddToVisiting(newHex);
                    }
                }
            }
            else
            {
                //Al llegar al final obtenemos el camino idóneo y bufamos esas casillas y las de alrededor
                while (hex != null && !hex.SameCoordinates(start))
                {
                    HexNode newHex = hex.parent;
                    // Evitamos NullReferenceException al llegar al nodo raíz (parent == null)
                    if (newHex == null) break;
                    newHex.priority -= 2;

                    //Obtener sus vecinos, bufarlos y meterlos en closed
                    List<HexCoordinates> neighborsCoordinates = GetNeighborsCoordinates(newHex);
                    foreach(HexCoordinates newCoordinate in neighborsCoordinates)
                    {
                        if(gridManager.CoordinateInWorld(newCoordinate)){
                            Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                            float newPriority = CalculatePriority(newCoordinate, start, end);
                            newPriority -= 1;//Bonus extra por estar cerca o en el camino ideal (es uno porque si tiene muchos vecinos puede que aumente demasiado si fuera mas de 1)
                            int currentMovement = CalculateMovement(hex, newTerrain);
                            HexNode neighborHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                            AddToClosed(neighborHex);
                        }
                    }

                    hex = newHex;
                }
                break;
            }
        }
    }
    // ================================

    // ================================
    // Calcula la distancia entre dos hexagonos usando coordenadas axiales
    // ================================
    protected static int AxialDistance(HexCoordinates start, HexCoordinates end)
    {
        return start.DistanceTo(end);
    }
    // ================================

}