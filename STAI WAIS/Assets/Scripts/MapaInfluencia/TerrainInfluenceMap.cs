using UnityEngine;

public class TerrainInfluenceMap : InfluenceMap
{
    public void Calculate(HexGridManager gridManager) 
    {
        Clear();
        if(gridManager == null) return;
        HexCell[] allCells = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);


        foreach (HexCell cell in allCells) 
        {
            if(cell == null || cell.Terrain == null) continue;

            Terrain terrain = cell.Terrain;
            
            //Caracteristicas generales del terreno
            float influence = terrain.MovementCost - terrain.DefenseBonus;

            SetInfluence(cell.Coordinates, influence);
        }
    }
}
