using System.Collections.Generic;

using UnityEngine;

// ================================
//Cosas de los terrenos especiales
// ================================

//distincion de tropa
public enum TroopType
{
    AoInterceptor,
    BoAvanzado,
    YoBombardero,
    XoCaza,
    MartilloODefensor,
    UoPhantom,
    EoStrike
}

//distincion de categoria 
public enum UnitCategory
{
    Nave,
    Edificio
}

//faccion
public enum Faction{
    Rebeldes,
    Imperio
}

[System.Serializable]
public struct BonusMovementTerrain
{
    public HexTerrainType terrain;
    public int value;
}
[System.Serializable]
public struct BonusDefenseTerrain
{
    public HexTerrainType terrain;
    public int value;
}
[System.Serializable]
public struct BonusDamageTroop
{
    public bool allShips;
    public bool allBuildings;
    public TroopType targetType;
    public int extraDamage;
}
[System.Serializable]
public struct SkinFaction
{
    public Faction faction;
    public string factionTroopName; 
    [TextArea] public string factionDescription; 
    public Sprite sprite;
}
// ================================

public class Troop : MonoBehaviour
{
    public HexCell currentCell;
    public Player owner;

    [Header("Mapas de influencia")]
    public TerrainInfluenceMap terrainInfluenceMap;
    public MilitaryInfluenceMap militaryInfluenceMap;
    public SecurityInfluenceMap securityInfluenceMap;

    [Header("Tipo y Ventajas")]
    public UnitCategory category = UnitCategory.Nave;
    public TroopType troopType;
    public BonusDamageTroop[] damageBonuses;

    [Header("Estadísticas")]
    public int attack;
    public int maxhp;
    public int hp;
    public int velocity;
    public int extradmg;
    public int range;

    [Header("Terrenos especiales")]
    public BonusMovementTerrain[] movementBonuses;
    public BonusDefenseTerrain[] defenseBonuses;
    public HexTerrainType[] impassableTerrains;

    [Header("Costes de construcción")]
    public int creditos;
    public int chatarra;
    public int tritio;
    public int cristales;

    [Header("Detalles de las tropas")]
    public new string name;
    public string descripcion;

    //Mapa de influencia propio de esta tropa 
    public InfluenceMap influenceMap;

    private void Awake()
    {
        
        influenceMap = new InfluenceMap();
    }

    [Header("Apariencia por Bando")]
    public SpriteRenderer spriteRenderer;
    public SkinFaction[] factionSkins;

    public virtual void ApplyFactionSkin(Faction playerFaction)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (factionSkins == null) return;

        foreach (var skin in factionSkins)
        {
            if (skin.faction == playerFaction)
            {
                // Cambiar sprite
                if (spriteRenderer != null && skin.sprite != null)
                    spriteRenderer.sprite = skin.sprite;

                // Cambiar nombre si se ha definido uno para esta facción
                if (!string.IsNullOrEmpty(skin.factionTroopName))
                    name = skin.factionTroopName;

                // Cambiar descripción si se ha definido una para esta facción
                if (!string.IsNullOrEmpty(skin.factionDescription))
                    descripcion = skin.factionDescription;

                break;
            }
        }
    }

    // ==========================================
    // SISTEMA DE COMBATE Y DAÑO POR TIPO
    // ==========================================

    public virtual int CalculateDamageAgainst(Troop target)
    {
        int totalDamage = attack;

        if (damageBonuses != null)
        {
            foreach (var bonus in damageBonuses)
            {
                //naves
                if (bonus.allShips && target.category == UnitCategory.Nave)
                {
                    totalDamage += bonus.extraDamage;
                }//edific
                else if (bonus.allBuildings && target.category == UnitCategory.Edificio)
                {
                    totalDamage += bonus.extraDamage;
                }//nave concreta
                else if (!bonus.allShips && !bonus.allBuildings && bonus.targetType == target.troopType)
                {
                    totalDamage += bonus.extraDamage;
                }
            }
        }

        int terrainDefense = target.GetCurrentTerrainDefense();
        totalDamage = Mathf.Max(1, totalDamage - terrainDefense);

        return totalDamage;
    }

    public virtual void Attack(Troop target)
    {
        if (target == null || target.owner == this.owner) return;

        int finalDamage = CalculateDamageAgainst(target);
        Debug.Log($"{name} ataca a {target.name} causando {finalDamage} de daño.");
        
        target.TakeDamage(finalDamage);
    }

    public virtual void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        
        Destroy(gameObject);
    }

    public int GetCurrentTerrainDefense()
    {
        if (currentCell == null || defenseBonuses == null) return 0;

        foreach (var def in defenseBonuses)
        {
            if (def.terrain == currentCell.TerrainType)
                return def.value;
        }
        return 0;
    }

    public void MoveToCell(HexCell targetCell)
    {
        //TODO
        if (currentCell == null || targetCell == null)
        {
            Debug.LogWarning("Movimiento imposible: no existe una casilla válida.");
            return;
        }

        //TODO dijsktra
        // Comprobar que sea una casilla vecina
        if (currentCell.Coordinates.DistanceTo(targetCell.Coordinates) != 1)
        {
            Debug.Log("Movimiento inválido: la casilla está demasiado lejos.");
            return;
        }

        //TODO si es válido el terreno
        // Comprobar el terreno 
        if (targetCell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano)
        {
            Debug.Log("Movimiento inválido: no se puede atravesar un agujero de gusano.");
            return;
        }

        // Comprobar si está ocupada
        if (targetCell.IsOccupied)
        {
            Debug.Log("Movimiento inválido: la casilla está ocupada.");
            return;
        }

        // Guardar la casilla anterior
        HexCell previousCell = currentCell;

        //TODO no casilla anterior

        // Ocupar la nueva casilla
        if (!targetCell.TrySetOccupant(this))
            return;

        // Liberar la anterior
        previousCell.ClearOccupant(this);

        // Actualizar posición
        currentCell = targetCell;
        transform.position = targetCell.transform.position;

        Debug.Log("Tropa movida a " + targetCell.Coordinates);
    }

    private void OnDestroy()
    {
        // Liberar la casilla si la tropa es destruida
        if (currentCell != null)
            currentCell.ClearOccupant(this);
        influenceMap = new InfluenceMap();
    }
    public void Initialize(HexCell startingCell, Player troopOwner)
    {
        if (startingCell == null)
        {
            Debug.LogError("No se puede crear la tropa: la casilla inicial es nula.");
            Destroy(gameObject);
            return;
        }

        //HAY QUE COMPROBAR SI LA TROPA SE PUEDE PONER EN ESE TERRENO DE LA CASILLA ELEJIDA
        //TAMBIÉN HAY QUE COMPROBAR QUE LA TROPA SE PUEDA PONER EN UNA TROPA ADJACENTE A LA ESTACIÓN DE COMPRA
        //BÁSICAMENTE PARA QUE NO SE PUEDA PONER EN CUALQUIER PARTE DEL MAPA POR LA CARA

        // Liberar una posible casilla anterior
        if (currentCell != null)
            currentCell.ClearOccupant(this);

        // Comprobar si la nueva casilla está libre
        if (!startingCell.TrySetOccupant(this))
        {
            Debug.LogWarning("No se puede crear una tropa: la casilla está ocupada.");
            currentCell = null;
            Destroy(gameObject);
            return;
        }

        currentCell = startingCell;
        owner = troopOwner;
        influenceMap = new InfluenceMap();

        if (owner != null)
        {
            ApplyFactionSkin(owner.faction);
        }

        transform.position = currentCell.transform.position;

        Debug.Log("Tropa creada en " + currentCell.Coordinates);
    }

    public void MoveToCell(HexCell targetCell, HexGridManager hexGridManager)
    {
        if (currentCell == null || targetCell == null)
        {
            Debug.LogWarning("Movimiento imposible: no existe una casilla válida.");
            return;
        }

        //Comprobar que la casilla sea válida
        List<HexCoordinates> validPositions = PathfindingDijkstra.GetValidPositions(hexGridManager, this);//targetCell.Coordinates,
        if (!validPositions.Contains(targetCell.Coordinates))
        {
            Debug.Log("Movimiento inválido: la casilla está demasiado lejos.");
            return;
        }

        // Guardar la casilla anterior
        HexCell previousCell = currentCell;

        //No casilla anterior
        if(targetCell == previousCell)
        {
            return;
        }

        // Ocupar la nueva casilla
        if (!targetCell.TrySetOccupant(this))
            return;

        // Liberar la anterior
        previousCell.ClearOccupant(this);

        // Actualizar posición
        currentCell = targetCell;
        transform.position = targetCell.transform.position;

        Debug.Log("Tropa movida a " + targetCell.Coordinates);
    }

    public void UpdateInfluenceMap(HexGridManager gridManager)
    {
        if (influenceMap == null)
            influenceMap = new InfluenceMap();

        //influenceMap.CalculateTerrainInfluence(gridManager);


    }
}

