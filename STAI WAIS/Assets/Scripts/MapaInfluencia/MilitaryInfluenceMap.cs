using UnityEngine;

public class MilitaryInfluenceMap
{
    public InfluenceMap AlliedInfluence { get; private set; }
    public InfluenceMap EnemyInfluence { get; private set; }
    public MilitaryInfluenceMap() 
    {
        AlliedInfluence = new InfluenceMap();
        EnemyInfluence = new InfluenceMap();
    }
    public void Calculate(Player perspective, int radius, float decay) 
    {
        AlliedInfluence.Clear();
        EnemyInfluence.Clear();
        if(perspective == null) return;

        HexCell[] allCells = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);

        Troop[] allTroops = Object.FindObjectsByType<Troop>(FindObjectsSortMode.None);

        foreach (Troop source in allTroops) 
        {
            if (source == null | source.currentCell == null || source.owner == null) continue;

            bool allied = source.owner.team == perspective.team;
            InfluenceMap targetMap = allied? AlliedInfluence : EnemyInfluence;

            int strenght = Mathf.Max(0, source.attack);

            foreach (HexCell cell in allCells) 
            {
                if(cell == null) continue;

                int distance = source.currentCell.Coordinates.DistanceTo(cell.Coordinates);
                //Limitar el radio de influendia 
                if (distance > radius) continue;
                //Decaimineto linera
                float influence = Mathf.Max(0f, strenght - decay * distance);

                targetMap.AddInfluence(cell.Coordinates, influence);
            }
        }
    }
}
