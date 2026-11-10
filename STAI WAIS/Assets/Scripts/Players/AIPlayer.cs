using UnityEngine;
using System.Collections.Generic;

public class AIPlayer : Player
{
    [Header("Configuracion")]
    public Troop troopPrefab; // ¡NUEVO! Arrastra aquí el prefab de la nave enemiga en el inspector
    [SerializeField] private HexGridManager gridManager;
    
    [Header("Tropas Activas")]
    public List<Troop> myTroops = new List<Troop>();

    public void PerformRandomMoves()
    {
        // 1. CREAR UNA NAVE NUEVA
        // Buscamos todas las casillas del tablero para elegir una al azar
        HexCell[] todasLasCasillas = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);
        
        if (troopPrefab != null && todasLasCasillas.Length > 0)
        {
            HexCell casillaAleatoria = todasLasCasillas[Random.Range(0, todasLasCasillas.Length)];
            
            // Instanciamos la nave y la asignamos a este jugador (la IA)
            Troop nuevaNave = Instantiate(troopPrefab);
            nuevaNave.Initialize(casillaAleatoria, this);
            myTroops.Add(nuevaNave);
            
            Debug.Log("IA: Nave creada en " + casillaAleatoria.Coordinates.ToString());
        }

        // 2. MOVER TODAS LAS NAVES DE LA IA
        foreach (Troop troop in myTroops)
        {
            if (troop.currentCell == null) continue;

            int randomDir = Random.Range(0, 6);
            HexCoordinates neighborCoords = troop.currentCell.Coordinates.GetNeighbor(randomDir);
            
            if (gridManager != null)
            {
                HexCell targetCell = gridManager.GetCell(neighborCoords);

                // Validamos que no se salga del mapa ni pise un agujero de gusano
                if (targetCell != null && targetCell.TerrainType != HexTerrainType.TerrenoAgujeroDeGusano)
                {
                    troop.MoveToCell(targetCell);
                }
            }
        }
    }
}
