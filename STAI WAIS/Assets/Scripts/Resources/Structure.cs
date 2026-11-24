using UnityEngine;

public class Structure : MonoBehaviour
{
    [Header("Tipo de estructura")]
    [SerializeField] private StructureTpe structureType;
    public StructureTpe StructureType => structureType;

    [Header("Producci�n")]
    [SerializeField] public int recursosPorTurno = 10;

    [Header("Inter�s estrat�gico")]
    [SerializeField] private float strategicValue = 20f;
    public float StrategicValue => strategicValue;

    //jugador que controla actualmente la estructura
    public Player ControllingPLayer { get; private set; }

    private Player baseOwner;             //Nuevo añadido de more

    public void SetBaseOwner(Player owner)
    {
        baseOwner = owner;
        ControllingPLayer = owner;
    }

    //tipo de recurso que produce esta estructura
    public ResourceType ResourceProduced 
    {
        get 
        {
            switch (structureType) 
            {
                case StructureTpe.StructMacronave:
                    return ResourceType.Chatarra;
                case StructureTpe.StructPlanetas:
                    return ResourceType.Creditos;
                case StructureTpe.StructAsteroides:
                    return ResourceType.Tritio;
                case StructureTpe.StructKyber:
                    return ResourceType.CristalKyber;
                default:
                    return ResourceType.Chatarra;
            }
        }
    }
    //Comrpobar quien controla la estructura
    public void ActualizarControl() 
    {
        HexCell estrcuturaCell = GetComponentInParent<HexCell>();
        if (estrcuturaCell == null) 
        {
            ControllingPLayer = null;
            return;
        }
        int rebeldes = 0;
        int imperio = 0;
        Player jugadorRebelde = null;
        Player jugadorImperio = null;

        HexGridManager grid = FindFirstObjectByType<HexGridManager>();
        if (grid == null) 
        {
            ControllingPLayer = null;
            return;
        }
        //Comprobamos las casillas adyacentes
        for (int i = 0; i < 6; i++) 
        {
            HexCoordinates coordinates = estrcuturaCell.Coordinates.GetNeighbor(i);
            
            HexCell vecino = grid.GetCell(coordinates);
            if (vecino == null || vecino.OccupyingTroop == null) continue;

            Troop tropa = vecino.OccupyingTroop;
            if (tropa.owner == null) continue;
            if (tropa.owner.faction == Faction.Rebeldes)
            {
                rebeldes++;
                jugadorRebelde = tropa.owner;
            }
            else if (tropa.owner.faction == Faction.Imperio) 
            {
                imperio++;
                jugadorImperio = tropa.owner;
            }
        }
        // determinar el dueño. En caso de empate mantiene el dueño base (null si es neutral)
        if (rebeldes > imperio) ControllingPLayer = jugadorRebelde;
        else if (imperio > rebeldes) ControllingPLayer = jugadorImperio;
        else ControllingPLayer = baseOwner;
    }

}
