using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class PathfindingManager
{
    //Tipo de tropa
    protected static Troop troop;

    //GridManager para obtener el terreno
    protected static HexGridManager gridManager;
    protected static InfluenceMap influenceMap;

    //Saber qué casillas han sido visitadas
    protected static List<HexNode> closed = new List<HexNode>(); 

    //Saber el orden de las casillas visitando por su prioridad (número más pequeño mayor prioridad)
    //Al añadir algo luego hay que poner -> visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
    protected static List<HexNode> visiting = new List<HexNode>();

    // ================================
    // Poner casilla en visitando
    // ================================
    protected static void AddToVisiting(HexNode hex)
    {
        if (!closed.Contains(hex) && hex.priority < int.MaxValue)
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
        }
    }
    // ================================

    // ================================
    // Poner casilla en closed
    // ================================
    protected static void AddToClosed(HexNode hex)
    {
        if (closed.Contains(hex) && hex.priority < int.MaxValue)
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
        }
    }
    // ================================

    // ================================
    // Sacar la casilla más prioritaria de visiting
    // ================================
    protected static HexNode PopVisiting()
    {
        HexNode hex = visiting[0];
        visiting.RemoveAt(0);
        visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
        AddToClosed(hex);
        return hex;
    }
    // ================================

    // ================================
    // Obtener las coordenadas de los vecinos de un hexágono dado
    // ================================
    protected static List<HexCoordinates> GetNeighborsCoordinates(HexNode hex)
    {
        List<HexCoordinates> neighbors = new List<HexCoordinates> {};

        int[] directions = {0,1,2,3,4,5};

        foreach (int direction in directions)
        {
            neighbors.Add(hex.coordinates.GetNeighbor(direction));
        }
            
        return neighbors;
    }
    // ================================

    // ================================
    // Calcular el gasto de movimiento
    // ================================
    protected static int CalculateMovement(HexNode hex, Terrain terrain)
    {
        int currentMovement = hex.currentMovementPoints;

        currentMovement -= terrain.MovementCost;

        foreach(BonusMovementTerrain bonus in troop.movementBonuses)
        {
            if(terrain.Type == bonus.terrain)
            {
                currentMovement -= bonus.value; //Mayor número = peor
                break;
            }
        }

        return currentMovement;
    }
    // ================================

    // ================================
    // Ver si la casilla está ocupada
    // ================================
    public static bool CellIsOccupied(HexNode hex)
    {
        HexCell cell = gridManager.GetCell(hex.coordinates);
        return cell.IsOccupied;
    }
    // ================================
}
