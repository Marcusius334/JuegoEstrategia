using UnityEngine;
using System.Collections.Generic;

public class InfluenceMap 
{
    //Diccionario que almacena la influencia de cada casilla
    private Dictionary<HexCoordinates, float> influenceValues = new Dictionary<HexCoordinates, float>();

    //asignar una incluencia a una casilla
    public void SetInfluence(HexCoordinates coordinates, float value) 
    {
        influenceValues[coordinates] = value;
    }
    //obtener la influencia de una casilla
    public float GetInfluence(HexCoordinates coordinates) 
    {
        if (influenceValues.TryGetValue(coordinates, out float value)) { return value; }
        return 0f;
    }

    //Sumar influencia a una casilla
    public void AddInfluence(HexCoordinates coordinates, float value) 
    {
        float currentValue = GetInfluence(coordinates);
        SetInfluence(coordinates, currentValue + value);
    }
    //Eliminar todos los valores del mapa
    public void Clear() 
    {
        influenceValues.Clear();
    }

    public void CalculateTerrainInfluence(HexGridManager gridManager)
    {
        // Borrar los valores anteriores
        Clear();

        // Recorrer todas las casillas del tablero
        HexCell[] allCells = Object.FindObjectsByType<HexCell>(
            FindObjectsSortMode.None
        );

        foreach (HexCell cell in allCells)
        {
            if (cell == null)
                continue;

            if (cell.Terrain == null)
            {
                Debug.LogError(
                    "La casilla " + cell.Coordinates +
                    " no tiene Terrain inicializado."
                );
                continue;
            }

            // Obtener las características del terreno
            int movementCost = cell.Terrain.MovementCost;
            int defenseBonus = cell.Terrain.DefenseBonus;

            // Calcular la influencia
            float influence = -movementCost + defenseBonus;

            // Guardar el resultado
            SetInfluence(cell.Coordinates, influence);

            Debug.Log(
                "Casilla: " + cell.Coordinates +
                " | Terreno: " + cell.TerrainType +
                " | Influencia: " + influence
            );
        }
    }
}
