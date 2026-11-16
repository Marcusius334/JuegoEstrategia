using UnityEngine;
using System.Collections.Generic;

public class InfluenceMap 
{
    //Diccionario que almacena la influencia de cada casilla
    private Dictionary<HexCoordinates, float> influenceValues = new Dictionary<HexCoordinates, float>();

    //asignar una incluencia a una casilla
    public void SetInfluence(HexCoordinates coordinates, float value) {influenceValues[coordinates] = value;}
    //obtener la influencia de una casilla
    public float GetInfluence(HexCoordinates coordinates) 
    {
        if (influenceValues.TryGetValue(coordinates, out float value)) { return value; }
        return 0f;
    }
    public float GetTroopInfluence(HexGridManager gridManager, HexCoordinates coordinates, Troop troop)
    {
        if (gridManager == null || troop == null) return Mathf.Infinity;
        HexCell cell = gridManager.GetCell(coordinates);
        if(cell == null || cell.Terrain == null) return Mathf.Infinity;
        Terrain terrain = cell.Terrain;

        // Comprobar terrenos imposibles de atravesar
        if (troop.impassableTerrains != null)
        {
            foreach (HexTerrainType type in troop.impassableTerrains)
            {
                if (terrain.Type == type)
                    return Mathf.Infinity;
            }
        }

        // Influencia general del terreno
        float value = GetInfluence(coordinates);

        // Bonificaciones de movimiento
        if (troop.movementBonuses != null)
        {
            foreach (BonusMovementTerrain bonus in troop.movementBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    value -= bonus.value;
                    break;
                }
            }
        }

        // Bonificaciones de defensa
        if (troop.defenseBonuses != null)
        {
            foreach (BonusDefenseTerrain bonus in troop.defenseBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    value += bonus.value;
                    break;
                }
            }
        }

        return value;

        /*//Obtener el value del GetInfluence, y si es distinto de 0 ponemos las cosas delas tropas
        float value = GetInfluence(coordinates);
        if(value != 0)
        {
            //Obtenemos la cell y el terreno a partir de las coordenadas con el gridManager
            HexCell cell = gridManager.GetCell(coordinates);
            Terrain terrain = cell.Terrain;

            //Los terrenos especiales imposibles de atravesar devuelven Mathf.Infinity
            foreach (HexTerrainType type in troop.impassableTerrains) 
            {
                if (terrain.Type == type) 
                {
                    return Mathf.Infinity;
                }
            }

            //Bonificaciones de movimiento de la tropa
            foreach (BonusMovementTerrain bonus in troop.movementBonuses) 
            {
                if (terrain.Type == bonus.terrain) 
                {
                    value -= bonus.value; //Cuanto mayor es la influencia + recomendable.Las penalizaciones de movimiento restan
                    break;
                }
            }

            //Bonificaciones defensivas de la tropa
            foreach (BonusDefenseTerrain bonus in troop.defenseBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    value += bonus.value; //Cuanto mayor es la influencia + recomendable. Los bonos de defensa suman
                    break;
                }
            }
        }
        return value;*/
    }

    //Sumar influencia a una casilla
    public void AddInfluence(HexCoordinates coordinates, float value) 
    {
        float currentValue = GetInfluence(coordinates);
        SetInfluence(coordinates, currentValue + value);
    }
    public bool HasInfluence(HexCoordinates coordinates) { return influenceValues.ContainsKey(coordinates); }
    //Eliminar todos los valores del mapa
    public void Clear() {influenceValues.Clear();}

    
}
