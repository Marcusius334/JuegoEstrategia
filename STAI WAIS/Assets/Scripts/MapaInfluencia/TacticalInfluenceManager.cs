using UnityEngine;

public class TacticalInfluenceManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private HexGridManager gridManager;
    [Header("Configuracion de influencia militar")]
    [SerializeField] private int influenceRadius = 4;
    [SerializeField] private float influenceDecay = 1.5f;

    //Mapas de Influencia
    public TerrainInfluenceMap TerrainMap { get; private set; }
    public MilitaryInfluenceMap MilitaryMap { get; private set; }
    public SecurityInfluenceMap SecurityMap { get; private set; }
    public StructureInfluenceMap StructureMap { get; private set; }

    private void Awake()
    {
        TerrainMap = new TerrainInfluenceMap();
        MilitaryMap = new MilitaryInfluenceMap();
        SecurityMap = new SecurityInfluenceMap();
        StructureMap = new StructureInfluenceMap();
        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
    }
    //Actualizar todos los mapas
    public void RefreshMaps(Player perspective, Troop troop) 
    {
        if (gridManager == null) 
        {
            Debug.LogError("No existe HexGridManager.");
            return;
        }
        if (perspective == null || troop == null) 
        {
            Debug.LogError("Faltan referencias para actualizar los mapas.");
            return;
        }

        //1.Calcular influencia del terreno
        TerrainMap.Calculate(gridManager);
        //2.Calcular influencia militar;
        MilitaryMap.Calculate(perspective, influenceRadius, influenceDecay);
        //3. Calcular seguridad
        SecurityMap.Calculate(MilitaryMap);
        //Calcular recursos
        StructureMap.Calculate(influenceRadius, influenceDecay);
        Debug.Log("Mapas de influencia actualizados.");
    }

    public float GetTroopInfluence(HexGridManager gridManager, HexCoordinates coordinates, Troop troop)
    {
        //Obtener el value del GetInfluence, y si es distinto de 0 ponemos las cosas delas tropas
        float value = TerrainMap.GetInfluence(coordinates);
        value += SecurityMap.GetInfluence(coordinates);
        value += StructureMap.GetInfluence(coordinates);

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
        return value;
    }

    
}
