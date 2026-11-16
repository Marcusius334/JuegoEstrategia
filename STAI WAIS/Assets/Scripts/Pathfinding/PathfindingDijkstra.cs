using System.Collections.Generic;
using UnityEngine;

public class PathfindingDijkstra : PathfindingManager
{
    // ================================
    // Realizar Dijkstra
    // ================================
    public static HexCoordinates GetBestPosition(HexGridManager newGridManager, Troop newTroop)
    {
        if (!PrepareSearch(newGridManager, newTroop)) return default;
      

        //Iniciar el primer nodo
        HexCoordinates coordinates = troop.currentCell.Coordinates;
        
        HexNode start = new HexNode(coordinates, 0f, troop.velocity, null);
        AddToVisiting(start);

        CalculateDijkstra();

        // Buscar la casilla alcanzable con menor coste estratégico.
        foreach (HexNode hex in closed)
        {
            if (!CellIsOccupied(hex))
                return hex.coordinates;
        }
        // Si no hay ninguna posición disponible, quedarse donde está.
        return coordinates;
    }
    // ================================

    // ================================
    // Obtener posiciones validas con Dijkstra
    // ================================
    public static List<HexCoordinates> GetValidPositions(HexGridManager newGridManager, Troop newTroop)
    {
        List<HexCoordinates> validPositions = new List<HexCoordinates>();

        if (!PrepareSearch(newGridManager, newTroop))
            return validPositions;

        HexCoordinates coordinates = troop.currentCell.Coordinates;

        HexNode start = new HexNode(coordinates,0f,troop.velocity,null);

        AddToVisiting(start);
        CalculateDijkstra();

        foreach (HexNode hex in closed)
        {
            if (!CellIsOccupied(hex) &&
                !validPositions.Contains(hex.coordinates))
            {
                validPositions.Add(hex.coordinates);
            }
        }

        return validPositions;
    }
    // ================================

    // ================================
    // Obtener la prioidad de una casilla
    // ================================
    public static float CalculatePriority(HexCoordinates coordinates)
    {
        return CalculateStrategicCost(coordinates);
        /*//Comprobar que tenemos las referencias necesarias 
        if (troop == null || gridManager == null) return Mathf.Infinity;
        if (troop.influenceManager == null) 
        {
            Debug.LogError("La tropa no tiene asignado un TacticalInfluenceManager.");
            return Mathf.Infinity;
        }
        //Obtener la influencia del terreno adaptada a esta tropa 
        float influence = troop.influenceManager.TerrainMap.GetTroopInfluence(gridManager, coordinates, troop); //obtiene la influencia del Terreno + los buffos/debuffos de la tropa al pasar por dicho terreno
        
        //AHORA MISMO ESTOY CONFUNDIDA, NO SÉ SI ES ASÍ O AL REVÉS float priority = +influence
        // Cuanto mayor sea la influencia, menor será la prioridad numérica.
        float priority = -influence;

        //Evitar que un terreno intransitable tenga prioridad negativa infinita
        if(priority == -Mathf.Infinity) {priority = Mathf.Infinity;}

        return priority;*/
    }
    // ================================

    // ================================
    // Calcular Dijkstra
    // ================================
    public static void CalculateDijkstra()
    {
        while (visiting.Count > 0)
        {
            HexNode hex = PopVisiting();

            if (hex == null)
                continue;

            // Añadir el nodo a las casillas ya exploradas.
            AddToClosed(hex);

            // Si otro camino ya había encontrado una opción mejor,
            // no necesitamos seguir explorando este nodo.
            if (!closed.Contains(hex))
                continue;

            // Si no quedan puntos de movimiento, no podemos avanzar.
            if (hex.currentMovementPoints <= 0)
                continue;

            HexCoordinates[] neighbors = GetNeighborsCoordinates(hex);

            foreach (HexCoordinates newCoordinate in neighbors)
            {
                Terrain newTerrain = gridManager.GetTerrain(newCoordinate);

                // Comprobar que el terreno sea transitable.
                int currentMovement = CalculateMovement(hex, newTerrain);

                // No entrar si no quedan suficientes puntos de movimiento.
                if (currentMovement < 0)
                    continue;

                HexCell cell = gridManager.GetCell(newCoordinate);

                // No atravesar casillas inexistentes u ocupadas.
                if (cell == null || cell.IsOccupied)
                    continue;

                // Calcular el coste estratégico acumulado.
                float stepCost = CalculateStrategicCost(newCoordinate);

                if (float.IsInfinity(stepCost))
                    continue;

                float newPriority = hex.costSoFar + stepCost;

                HexNode newHex = new HexNode(
                    newCoordinate,
                    newPriority,
                    currentMovement,
                    hex
                );

                AddToVisiting(newHex);
            }
        }
    }
    /*//Mientras hayan cosas en visiting
    while(visiting.Count > 0){
        //Sacar el nodo más prioritario de visiting y comprobar si le queda movimiento
        HexNode hex = PopVisiting();

        if(hex.currentMovementPoints > 0){
            //Obtener sus vecinos y meter los vecinos en visiting
            HexCoordinates[] neighborsCoordinates = GetNeighborsCoordinates(hex);
            foreach(HexCoordinates newCoordinate in neighborsCoordinates)
            {
                Terrain newTerrain = gridManager.GetTerrain(newCoordinate);
                float newPriority = CalculatePriority(newCoordinate);
                int currentMovement = CalculateMovement(hex, newTerrain);
                HexNode newHex = new HexNode(newCoordinate, newPriority, currentMovement, hex);
                AddToVisiting(newHex);
            }
        }
    }*/
}
    // ================================
