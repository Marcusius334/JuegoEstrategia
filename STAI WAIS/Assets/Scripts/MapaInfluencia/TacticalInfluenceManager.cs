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

    private void Awake()
    {
        TerrainMap = new TerrainInfluenceMap();
        MilitaryMap = new MilitaryInfluenceMap();
        SecurityMap = new SecurityInfluenceMap();
        if(gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();
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
        TerrainMap.Calculate(gridManager, troop);
        //2.Calcular influencia militar;
        MilitaryMap.Calculate(perspective, influenceRadius, influenceDecay);
        //3. Calcular seguridad
        SecurityMap.Calculate(MilitaryMap);
        Debug.Log("Mapas de influencia actualizados.");
    }

    
}
