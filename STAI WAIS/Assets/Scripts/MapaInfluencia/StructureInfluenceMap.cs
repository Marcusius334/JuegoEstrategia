using UnityEngine;

public class StructureInfluenceMap: InfluenceMap
{
    public void Calculate(int radius, float decay)
    {
        Clear();

        Structure[] structures =Object.FindObjectsByType<Structure>(FindObjectsSortMode.None); //detecta todas las estruturas del mapa

        HexCell[] allCells = Object.FindObjectsByType<HexCell>(FindObjectsSortMode.None);

        foreach (Structure structure in structures)
        {
            if (structure == null) continue;

            HexCell structureCell = structure.GetComponentInParent<HexCell>();

            if (structureCell == null) continue;

            float strength = structure.StrategicValue; //por ejemplo el CritalKyrbul(? tendrá más valor estrategico que la chatarra

            //propagacion de la influencia
            foreach (HexCell cell in allCells)
            {
                if (cell == null) continue;

                int distance =structureCell.Coordinates.DistanceTo(cell.Coordinates);

                if (distance > radius) continue;

                float influence = Mathf.Max(0f,strength - decay * distance);

                AddInfluence(cell.Coordinates, influence);
            }
        }
    }
}
