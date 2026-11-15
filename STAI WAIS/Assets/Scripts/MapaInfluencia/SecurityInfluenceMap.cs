using UnityEngine;

public class SecurityInfluenceMap : InfluenceMap //Combina los mapas de influencia de los enemigos y de los aliados
{
    public void Calculate(MilitaryInfluenceMap militaryMap) 
    {
        Clear();

        if (militaryMap == null) return;
        HexCell[] allCells = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);

        foreach (HexCell cell in allCells) 
        {
            if (cell == null) continue;
            float allied = militaryMap.AlliedInfluence.GetInfluence(cell.Coordinates);
            float enemy = militaryMap.EnemyInfluence.GetInfluence(cell.Coordinates);
            float security = allied - enemy;
            SetInfluence(cell.Coordinates, security);

        }
    }
}
