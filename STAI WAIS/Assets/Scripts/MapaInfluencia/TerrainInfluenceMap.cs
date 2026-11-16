using UnityEngine;

public class TerrainInfluenceMap : InfluenceMap
{
    public void Calculate(HexGridManager gridManager, Troop troop) 
    {
        Clear();
        if(gridManager == null|| troop == null) return;
        HexCell[] allCells = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);


        foreach (HexCell cell in allCells) 
        {
            if(cell == null || cell.Terrain == null) continue;

            Terrain terrain = cell.Terrain;
            //Los terrenos especiales imporibles de atravesar reciben una penalizacion muy elevada
            bool impassable = false; 
            foreach (HexTerrainType type in troop.impassableTerrains) 
            {
                if (terrain.Type == type) 
                {
                    impassable = true;
                }
            }
            if (impassable) 
            {
                SetInfluence(cell.Coordinates, Mathf.Infinity);
                continue;
            }
            //Caracteristicas generales del terreno
            float influence = terrain.MovementCost - terrain.DefenseBonus;

            //Bonificaciones de movimiento de la tropa
            foreach (BonusMovementTerrain bonus in troop.movementBonuses) 
            {
                if (terrain.Type == bonus.terrain) 
                {
                    influence -= bonus.value; //Cuanto mayor es la influencia + recomendable.Las penalizaciones de movimiento restan
                    break;
                }
            }
            //Bonificaciones defensivas de la tropa
            foreach (BonusDefenseTerrain bonus in troop.defenseBonuses)
            {
                if (terrain.Type == bonus.terrain)
                {
                    influence += bonus.value; //Cuanto mayor es la influencia + recomendable. Los bonos de defensa suman
                    break;
                }
            }
            SetInfluence(cell.Coordinates, influence);
        }
    }
}
