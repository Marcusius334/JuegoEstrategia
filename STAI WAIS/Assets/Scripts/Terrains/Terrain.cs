using UnityEngine;
[System.Serializable]

public class Terrain: MonoBehaviour
{
    public HexTerrainType Type;
    public int MovementCost;
    public int DefenseBonus;
    public Color TerrainColor; //M�s adelante habr�a que cambiarlo por el sprite correspondiente a cada terreno

    //Constructor
    public Terrain(HexTerrainType type, int movementCost, int defenseBonus, Color terrainColor) 
    {
        Type = type;
        MovementCost = movementCost;
        DefenseBonus = defenseBonus;
        TerrainColor = terrainColor; //m�s adelante el color deber� ser un sprite
    }
}
