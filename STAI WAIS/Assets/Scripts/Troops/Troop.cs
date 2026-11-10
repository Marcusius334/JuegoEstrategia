using UnityEngine;

public class Troop : MonoBehaviour
{
    public HexCell currentCell;
    public Player owner; // Para saber a qué jugador pertenece (Humano o IA)

    public void Initialize(HexCell startingCell, Player troopOwner)
    {
        currentCell = startingCell;
        owner = troopOwner;
        transform.position = currentCell.transform.position;
    }

    public void MoveToCell(HexCell targetCell)
    {
        // Validar si es una casilla vecina transitable
        if (currentCell.Coordinates.DistanceTo(targetCell.Coordinates) == 1 && 
            targetCell.TerrainType != HexTerrainType.TerrenoAgujeroDeGusano)
        {
            currentCell = targetCell;
            transform.position = targetCell.transform.position; 
            Debug.Log($"Tropa movida a {targetCell.Coordinates}");
        }
        else
        {
            Debug.Log("Movimiento inválido: la casilla está demasiado lejos o no es accesible.");
        }
    }
}
