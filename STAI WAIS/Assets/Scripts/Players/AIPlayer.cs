
using UnityEngine;
using System.Collections.Generic;

public class AIPlayer : Player
{
    [Header("Configuracion")]
    public Troop troopPrefab;

    [SerializeField] private HexGridManager gridManager;

    [Header("Tropas Activas")]
    public List<Troop> myTroops = new List<Troop>();

    public void PerformRandomMoves()
    {
        Debug.Log("IA: Comienza mi turno.");

        // Comprobar referencias necesarias
        if (troopPrefab == null)
        {
            Debug.LogError("IA: No hay un prefab de tropa asignado.");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("IA: No hay un HexGridManager asignado.");
            return;
        }

        // =========================================
        // 1. GENERAR UNA NUEVA TROPA
        // =========================================

        HexCell[] allCells = FindObjectsByType<HexCell>(
            FindObjectsSortMode.None
        );

        List<HexCell> availableCells = new List<HexCell>();

        foreach (HexCell cell in allCells)
        {
            if (cell != null &&
                !cell.IsOccupied &&
                cell.TerrainType != HexTerrainType.TerrenoAgujeroDeGusano)
            {
                availableCells.Add(cell);
            }
        }

        if (availableCells.Count > 0)
        {
            // Elegir una casilla libre aleatoria
            HexCell spawnCell = availableCells[
                Random.Range(0, availableCells.Count)
            ];

            Troop newTroop = Instantiate(troopPrefab);

            newTroop.Initialize(spawnCell, this);

            if (newTroop != null && newTroop.currentCell == spawnCell)
            {
                Debug.Log("IA: Nueva tropa creada en " + spawnCell.Coordinates);
            }
        }
        else
        {
            Debug.LogWarning("IA: No hay casillas disponibles para crear tropas.");
        }

        // =========================================
        // 2. LOCALIZAR TODAS LAS TROPAS ENEMIGAS
        // =========================================

        myTroops.Clear();

        Troop[] allTroops = FindObjectsByType<Troop>(
            FindObjectsSortMode.None
        );

        foreach (Troop troop in allTroops)
        {
            if (troop != null && troop.owner == this)
            {
                myTroops.Add(troop);
            }
        }

        Debug.Log("IA: Tengo " + myTroops.Count + " tropas.");

        // =========================================
        // 3. MOVER TODAS LAS TROPAS
        // =========================================

        foreach (Troop troop in myTroops)
        {
            if (troop == null || troop.currentCell == null)
                continue;

            List<HexCell> validNeighbors = new List<HexCell>();

            // Comprobar las seis direcciones posibles
            for (int i = 0; i < 6; i++)
            {
                HexCoordinates neighborCoords =
                    troop.currentCell.Coordinates.GetNeighbor(i);

                HexCell neighborCell = gridManager.GetCell(neighborCoords);

                // Comprobar que la casilla exista
                if (neighborCell == null)
                    continue;

                // Comprobar terreno y ocupacion
                if (neighborCell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano)
                    continue;

                if (neighborCell.IsOccupied)
                    continue;

                validNeighbors.Add(neighborCell);
            }

            // Elegir una casilla vecina aleatoria
            if (validNeighbors.Count > 0)
            {
                HexCell destination = validNeighbors[
                    Random.Range(0, validNeighbors.Count)
                ];

                troop.MoveToCell(destination);
            }
            else
            {
                Debug.Log("IA: Una tropa no tiene movimientos disponibles.");
            }
        }

        Debug.Log("IA: He terminado mi turno.");
    }
}
