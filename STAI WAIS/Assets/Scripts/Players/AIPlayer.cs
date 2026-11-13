

using UnityEngine;
using System.Collections.Generic;

public class AIPlayer : Player
{
    [Header("Configuracion")]
    public Troop troopPrefab;
    [SerializeField] private HexGridManager gridManager;

    [Header("Tropas Activas")]
    public List<Troop> myTroops = new List<Troop>();

    void Start(){
        chatarra = gameManager.ia_chatarra;
        creditos = gameManager.ia_creditos;
        tritio = gameManager.ia_tritio;
        cristal = gameManager.ia_cristal;
    }

    // =========================================
    // FASE 1: CONSTRUCCION
    // =========================================

    public void PerformConstructionPhase()
    {
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
    private HexCell ChoosBestCell(Troop troop, List<HexCell> neighbors) //supongo que esta funcion habrá que cambiarla o moverla cuando tengamos el A*
    {
        HexCell bestCell = null;
        //Empezamos con la influencia más baja posible
        float highestInfluence = float.MinValue;
        foreach (HexCell cell in neighbors) 
        {
            float influence = troop.influenceMap.GetInfluence(cell.Coordinates);
            if (influence > highestInfluence) 
            {
                highestInfluence = influence;
                bestCell = cell;
            }
        }
        return bestCell;
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
                HexCell destination = ChoosBestCell(troop, validNeighbors);
                if (destination != null) { troop.MoveToCell(destination); }
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


    
}
