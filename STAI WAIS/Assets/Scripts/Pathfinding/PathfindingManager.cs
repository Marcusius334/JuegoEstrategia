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

    //Saber qué casillas han sido visitadas
    protected static List<HexNode> closed = new List<HexNode>(); 

    //Saber el orden de las casillas visitando por su prioridad (número más pequeño mayor prioridad)
    //Al añadir algo luego hay que poner -> visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
    protected static List<HexNode> visiting = new List<HexNode>();

    // ================================
    // Sacar la casilla más prioritaria de visiting
    // ================================
    protected static HexNode PopVisiting()
    {
        HexNode hex = visiting[0];
        visiting.RemoveAt(0);
        visiting.Sort((a, b) => a.priority.CompareTo(b.priority));
        closed.Add(hex);
        return hex;
    }
    // ================================

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
    // Obtener las coordenadas de los vecinos de un hexágono dado
    // ================================
    protected static HexCoordinates[] GetNeighborsCoordinates(HexNode hex)
    {
        HexCoordinates[] neighbors = new HexCoordinates[6];

        int[,] directions = {
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
        }

        return neighbors;
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
