using UnityEngine;
using System.Collections.Generic;


public class PathfindingManager
{
    //Tipo de tropa
    protected static Troop troop;

    //GridManager para obtener el terreno
    protected static HexGridManager gridManager;

    //Saber qué casillas han sido visitadas
    protected static List<HexNode> closed = new List<HexNode>(); 

    //Saber el orden de las casillas visitando por su prioridad (número más pequeño mayor prioridad)
    //Al añadir algo luego hay que poner -> visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
    protected static List<HexNode> visiting = new List<HexNode>();

    // ================================
    // Reiniciar la búsqueda
    // ================================
    protected static void ResetSearch() 
    {
        closed.Clear();
        visiting.Clear();
    }

    //Preparar los datos necesarios para buscar una ruta
    protected static bool PrepareSearch(HexGridManager newGridManager, Troop newTroop) 
    {
        ResetSearch();
        gridManager = newGridManager;
        troop = newTroop;

        if (gridManager == null || troop == null ||
            troop.currentCell == null || troop.owner == null)
        {
            Debug.LogError("Faltan referencias para realizar el pathfinding.");
            return false;
        }

        if (troop.influenceManager == null)
        {
            Debug.LogError("La tropa no tiene asignado TacticalInfluenceManager.");
            return false;
        }

        // Actualizar los mapas desde la perspectiva de esta tropa
        troop.influenceManager.RefreshMaps(troop.owner);

        return true;

    }
    // ================================
    // Comprobar si dos nodos representan la misma casilla
    // ================================
    protected static bool SameCell(HexNode a, HexNode b) 
    {
        if(a== null|| b == null) return false;
        return a.coordinates.DistanceTo(b.coordinates) == 0;
    }
    // ================================
    // Poner casilla en visitando
    // ================================
    protected static void AddToVisiting(HexNode hex)
    {
        if (hex == null || float.IsInfinity(hex.priority) || float.IsNaN(hex.priority))
            return;

        // Comprobar si algún nodo de closed es mejor o igual.
        bool dominatedByClosed = closed.Exists(x =>
            SameCell(x, hex) &&
            x.priority <= hex.priority &&
            x.currentMovementPoints >= hex.currentMovementPoints);

        if (dominatedByClosed) return;

        // Eliminar nodos de closed que sean peores.
        closed.RemoveAll(x =>
            SameCell(x, hex) &&
            hex.priority <= x.priority &&
            hex.currentMovementPoints >= x.currentMovementPoints);

        // Comprobar si ya existe un nodo que domina al nuevo.
        bool dominatedByVisiting = visiting.Exists(x =>
            SameCell(x, hex) &&
            x.priority <= hex.priority &&
            x.currentMovementPoints >= hex.currentMovementPoints);

        if (dominatedByVisiting) return;

        // Eliminar nodos de visiting que sean peores.
        visiting.RemoveAll(x =>
            SameCell(x, hex) &&
            hex.priority <= x.priority &&
            hex.currentMovementPoints >= x.currentMovementPoints);

        visiting.Add(hex);
        visiting.Sort((a, b) => a.priority.CompareTo(b.priority));

        /*if (!closed.Contains(hex) && hex.priority < int.MaxValue)
        {
            int index = visiting.FindIndex(x => x.Equals(hex));

            if(index != -1)
            {
                if(hex.priority < visiting[index].priority)
                {
                    visiting.RemoveAt(index);
                    visiting.Add(hex);
                    visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
                }
            }
            else
            {
                visiting.Add(hex);
                visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
            }
            
        }else if (!closed.Contains(hex))
        {
            closed.Add(hex);
        }*/
    }
    // ================================

    // ================================
    // Poner casilla en closed
    // ================================
    protected static void AddToClosed(HexNode hex)
    {
        if(hex == null) return;

        bool dominated = closed.Exists(x =>
            SameCell(x, hex) &&
            x.priority <= hex.priority &&
            x.currentMovementPoints >= hex.currentMovementPoints);

        if (dominated) return;

        closed.RemoveAll(x =>
            SameCell(x, hex) &&
            hex.priority <= x.priority &&
            hex.currentMovementPoints >= x.currentMovementPoints);

        closed.Add(hex);
        closed.Sort((a, b) => a.priority.CompareTo(b.priority));
        /*if (closed.Contains(hex) && hex.priority < int.MaxValue)
        {
            int index = closed.FindIndex(x => x.Equals(hex));
            if(hex.priority < closed[index].priority)
            {
                closed.RemoveAt(index);
                closed.Add(hex);
                closed.Sort((a, b) => a.priority.CompareTo(b.priority));
            }
            
        }else if (!closed.Contains(hex))
        {
            closed.Add(hex);
        }*/
    }
    // ================================

    // ================================
    // Sacar la casilla más prioritaria de visiting
    // ================================
    protected static HexNode PopVisiting()
    {
        if(visiting.Count == 0) return null;

        HexNode hex = visiting[0];
        visiting.RemoveAt(0);
        //visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
        //AddToClosed(hex);
        return hex;
    }
    // ================================

    // ================================
    // Obtener las coordenadas de los vecinos de un hexágono dado
    // ================================
    protected static HexCoordinates[] GetNeighborsCoordinates(HexNode hex)
    {
        HexCoordinates[] neighbors = new HexCoordinates[6];

        for (int i = 0; i < 6; i++) 
        {
            neighbors[i] = hex.coordinates.GetNeighbor(i);
        }
        /*int[,] directions = {
            { 1, 0 },
            { 1, -1 },
            { 0, 1 },
            { -1, 0 },
            { -1, 1 },
            { 0, -1 }
        };

        foreach (int direction in directions)
        {
            hex.coordinates.GetNeighbor(direction);
        }*/

        return neighbors;
    }
    // ================================

    // ================================
    // Calcular el gasto de movimiento
    // ================================
    //Calcular el coste físico de entrar en un terreno
    protected static int CalculateTerrainMovementCost(Terrain terrain) 
    {
        if (terrain == null || troop == null)
            return -1;

        if (troop.impassableTerrains != null)
        {
            foreach (HexTerrainType type in troop.impassableTerrains)
            {
                if (terrain.Type == type)
                    return -1;
            }
        }

        int movementCost = terrain.MovementCost;

        if (troop.movementBonuses != null)
        {
            foreach (BonusMovementTerrain bonus in troop.movementBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    movementCost += bonus.value;
                    break;
                }
            }
        }

        return Mathf.Max(1, movementCost);
    }
    protected static int CalculateMovement(HexNode hex, Terrain terrain)
    {
        if (hex == null) return -1;

        int movementCost = CalculateTerrainMovementCost(terrain);

        if (movementCost < 0) return -1;

        return hex.currentMovementPoints - movementCost;
        /*if ((hex == null|| terrain == null|| troop == null))
            return -1;

        //Comprobar terrenos intransitables 
        if (troop.impassableTerrains != null) 
        {
            foreach (HexTerrainType type in troop.impassableTerrains) 
            {
                if (terrain.Type == type)
                    return -1;
            }
        }

        int movementCost = terrain.MovementCost;

        //Aplicar penalizaciones o bonificaciones de movimiento 
        if (troop.movementBonuses != null) 
        {
            foreach (BonusMovementTerrain bonus in troop.movementBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    movementCost += bonus.value; 
                    break;
                }
            }
        }
        // Una casilla transitable nunca puede costar menos de 1 PM
        movementCost = Mathf.Max(1, movementCost);

        return hex.currentMovementPoints - movementCost;*/
    }
    // ================================

    // ================================
    // Ver si la casilla está ocupada
    // ================================
    public static bool CellIsOccupied(HexNode hex)
    {
        if (hex == null || gridManager == null) return true;

 
        HexCell cell = gridManager.GetCell(hex.coordinates);
        if (cell == null) return true;

        return cell.IsOccupied;
    }
    // ================================

    // Calcular el coste estratégico de entrar en una casilla
    protected static float CalculateStrategicCost(HexCoordinates coordinates)
    {
        if (troop == null || troop.influenceManager == null)
            return Mathf.Infinity;

        float terrainInfluence = troop.influenceManager.TerrainMap.GetTroopInfluence(gridManager, coordinates, troop);

        if (float.IsInfinity(terrainInfluence))
            return Mathf.Infinity;

        // La influencia positiva del terreno representa una ventaja.
        float terrainCost = -terrainInfluence;

        // Security = aliados - enemigos.
        // Por tanto, el peligro es la superioridad enemiga.
        float security = troop.influenceManager.SecurityMap.GetInfluence(coordinates);

        float danger = Mathf.Max(0f, -security);

        // Dijkstra y A* necesitan costes no negativos.
        return Mathf.Max(0.01f, terrainCost + danger);
    }
}
