using UnityEngine;

public class AIPlayer : Player
{
    // Update is called once per frame
    void Update()
    {
<<<<<<< Updated upstream
        
    }
=======
        Debug.Log("IA: FASE DE CONSTRUCCION");

        if (troopPrefab == null || gridManager == null)
        {
            Debug.LogError("IA: Faltan referencias en el Inspector.");
            return;
        }

        HexCell[] allCells = FindObjectsByType<HexCell>(
            FindObjectsSortMode.None
        );

        List<HexCell> availableCells = new List<HexCell>();

        // Buscar casillas libres y transitables
        //HAY QUE COMPROBAR SI LA TROPA SE PUEDE PONER EN ESE TERRENO DE LA CASILLA ELEJIDA
        //TAMBIÉN HAY QUE COMPROBAR QUE LA TROPA SE PUEDA PONER EN UNA TROPA ADJACENTE A LA ESTACIÓN DE COMPRA
        //BÁSICAMENTE PARA QUE NO SE PUEDA PONER EN CUALQUIER PARTE DEL MAPA POR LA CARA
        foreach (HexCell cell in allCells)
        {
            if (cell != null &&
                !cell.IsOccupied &&
                cell.TerrainType != HexTerrainType.TerrenoAgujeroDeGusano)
            {
                availableCells.Add(cell);
            }
        }

        if (availableCells.Count == 0)
        {
            Debug.LogWarning("IA: No hay casillas disponibles para crear tropas.");
            return;
        }

        // Elegir una casilla aleatoria
        HexCell spawnCell = availableCells[
            Random.Range(0, availableCells.Count)
        ];

        // Crear la tropa
        Troop newTroop = Instantiate(troopPrefab);

        newTroop.Initialize(spawnCell, this);

        if (newTroop.currentCell == spawnCell)
        {
            myTroops.Add(newTroop);
            Debug.Log("IA: Tropa creada en " + spawnCell.Coordinates);
        }
    }

    // =========================================
    // FASE 2: MOVIMIENTO
    // =========================================
    private HexCell ChoosBestCell(Troop troop)
    {
        return gridManager.GetCell(PathfindingDijkstra.GetBestPosition(gridManager, troop));
    }
    public void PerformMovementPhase()
    {
        Debug.Log("IA: FASE DE MOVIMIENTO");

        if (gridManager == null)
        {
            Debug.LogError("IA: No hay GridManager asignado.");
            return;
        }

        // Actualizar la lista de tropas propias
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

        // Mover todas las tropas
        foreach (Troop troop in myTroops)
        {
            if (troop == null || troop.currentCell == null)
                continue;

            troop.UpdateInfluenceMap(gridManager);

            List<HexCell> validNeighbors = new List<HexCell>();

            // Comprobar las seis direcciones
            for (int i = 0; i < 6; i++)
            {
                HexCoordinates neighborCoords =
                    troop.currentCell.Coordinates.GetNeighbor(i);

                HexCell neighborCell = gridManager.GetCell(neighborCoords);

                if (neighborCell == null)
                    continue;

                if (neighborCell.TerrainType ==
                    HexTerrainType.TerrenoAgujeroDeGusano)
                    continue;

                if (neighborCell.IsOccupied)
                    continue;

                validNeighbors.Add(neighborCell);
            }

            // Elegir un movimiento aleatorio
            /*if (validNeighbors.Count > 0)
            {
                HexCell destination = validNeighbors[
                    Random.Range(0, validNeighbors.Count)
                ];

                troop.MoveToCell(destination);
            }*/
            //Elegir un movimiento en funcion del coste del terreno
            if (validNeighbors.Count > 0) 
            {
                HexCell destination = ChoosBestCell(troop);
                if (destination != null) { troop.MoveToCell(destination, gridManager); }
            }
            else
            {
                Debug.Log("IA: Una tropa no puede moverse.");
            }
        }

        Debug.Log("IA: Movimiento terminado.");
    }

    // =========================================
    // FASE 3: ATAQUE
    // =========================================

    public void PerformAttackPhase()
    {
        Debug.Log("IA: FASE DE ATAQUE (Todavia no implementada).");

        // Aqui añadiremos la logica de ataque
        // cuando implementemos el sistema de combate.
    }


    
>>>>>>> Stashed changes
}
