<<<<<<< Updated upstream
=======

using System.Collections.Generic;
>>>>>>> Stashed changes
using UnityEngine;

public class Troop : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
<<<<<<< Updated upstream
        
    }

    // Update is called once per frame
    void Update()
    {
        
=======
        influenceMap = new InfluenceMap();
    }
    public void Initialize(HexCell startingCell, Player troopOwner)
    {
        if (startingCell == null)
        {
            Debug.LogError("No se puede crear la tropa: la casilla inicial es nula.");
            Destroy(gameObject);
            return;
        }

        //HAY QUE COMPROBAR SI LA TROPA SE PUEDE PONER EN ESE TERRENO DE LA CASILLA ELEJIDA
        //TAMBIÉN HAY QUE COMPROBAR QUE LA TROPA SE PUEDA PONER EN UNA TROPA ADJACENTE A LA ESTACIÓN DE COMPRA
        //BÁSICAMENTE PARA QUE NO SE PUEDA PONER EN CUALQUIER PARTE DEL MAPA POR LA CARA

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
        influenceMap = new InfluenceMap();

        transform.position = currentCell.transform.position;

        Debug.Log("Tropa creada en " + currentCell.Coordinates);
    }

    public void MoveToCell(HexCell targetCell, HexGridManager hexGridManager)
    {
        if (currentCell == null || targetCell == null)
        {
            Debug.LogWarning("Movimiento imposible: no existe una casilla válida.");
            return;
        }

        //Comprobar que la casilla sea válida
        List<HexCoordinates> validPositions = PathfindingDijkstra.GetValidPositions(targetCell.Coordinates, hexGridManager, this);
        if (!validPositions.Contains(targetCell.Coordinates))
        {
            Debug.Log("Movimiento inválido: la casilla está demasiado lejos.");
            return;
        }

        // Guardar la casilla anterior
        HexCell previousCell = currentCell;

        //No casilla anterior
        if(targetCell == previousCell)
        {
            return;
        }

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

    public void UpdateInfluenceMap(HexGridManager gridManager)
    {
        if (influenceMap == null)
            influenceMap = new InfluenceMap();

        influenceMap.CalculateTerrainInfluence(gridManager);
>>>>>>> Stashed changes
    }
}
