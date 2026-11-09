using UnityEngine;
[System.Serializable]

public class Terrain
{
    public HexTerrainType Type;
    public int MovementCost;
    public int DefenseBonus;
    public Color TerrainColor; //Más adelante habría que cambiarlo por el sprite correspondiente a cada terreno

    //Constructor
    public Terrain(HexTerrainType type, int movementCost, int defenseBonus, Color terrainColor) 
    {
        Type = type;
        MovementCost = movementCost;
        DefenseBonus = defenseBonus;
        TerrainColor = terrainColor;
    }
}
