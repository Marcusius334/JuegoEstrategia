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
