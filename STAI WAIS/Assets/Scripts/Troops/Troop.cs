
using UnityEngine;

public class Troop : MonoBehaviour
{
    public HexCell currentCell;
    public Player owner;

    public void Initialize(HexCell startingCell, Player troopOwner)
    {
        if (startingCell == null)
        {
            Debug.LogError("No se puede crear la tropa: la casilla inicial es nula.");
            Destroy(gameObject);
            return;
        }

        // Liberar una posible casilla anterior
        if (currentCell != null)
            currentCell.ClearOccupant(this);

        // Comprobar si la nueva casilla está libre
        if (!startingCell.TrySetOccupant(this))
        {
            Debug.LogWarning("No se puede crear una tropa: la casilla está ocupada.");
            currentCell = null;
            Destroy(gameObject);
            return;
        }

        currentCell = startingCell;
        owner = troopOwner;

        transform.position = currentCell.transform.position;

        Debug.Log("Tropa creada en " + currentCell.Coordinates);
    }

    public void MoveToCell(HexCell targetCell)
    {
        if (currentCell == null || targetCell == null)
        {
            Debug.LogWarning("Movimiento imposible: no existe una casilla válida.");
            return;
        }

        // Comprobar que sea una casilla vecina
        if (currentCell.Coordinates.DistanceTo(targetCell.Coordinates) != 1)
        {
            Debug.Log("Movimiento inválido: la casilla está demasiado lejos.");
            return;
        }

        // Comprobar el terreno
        if (targetCell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano)
        {
            Debug.Log("Movimiento inválido: no se puede atravesar un agujero de gusano.");
            return;
        }

        // Comprobar si está ocupada
        if (targetCell.IsOccupied)
        {
            Debug.Log("Movimiento inválido: la casilla está ocupada.");
            return;
        }

        // Guardar la casilla anterior
        HexCell previousCell = currentCell;

        // Ocupar la nueva casilla
        if (!targetCell.TrySetOccupant(this))
            return;

        // Liberar la anterior
        previousCell.ClearOccupant(this);

        // Actualizar posición
        currentCell = targetCell;
        transform.position = targetCell.transform.position;

        Debug.Log("Tropa movida a " + targetCell.Coordinates);
    }

    private void OnDestroy()
    {
        // Liberar la casilla si la tropa es destruida
        if (currentCell != null)
            currentCell.ClearOccupant(this);
    }
}

