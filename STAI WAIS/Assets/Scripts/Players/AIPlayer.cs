using UnityEngine;
using System.Collections.Generic;

public class AIPlayer : Player
{
    public List<Troop> myTroops = new List<Troop>();
    [SerializeField] private HexGridManager gridManager;

    // Se llama a esta función desde el TurnsManager cuando empiece el turno de la IA
    public void PerformRandomMoves()
    {
        foreach (Troop troop in myTroops)
        {
            if (troop.currentCell == null) continue;

            // Elegir una dirección aleatoria (de 0 a 5)
            int randomDir = Random.Range(0, 6);
            HexCoordinates neighborCoords = troop.currentCell.Coordinates.GetNeighbor(randomDir);
            
            HexCell targetCell = gridManager.GetCell(neighborCoords);

            // Si la casilla existe en el tablero, moverse
            if (targetCell != null)
            {
                troop.MoveToCell(targetCell);
            }
        }
    }
}
