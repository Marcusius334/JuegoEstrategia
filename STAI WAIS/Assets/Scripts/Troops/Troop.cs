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
    public TacticalInfluenceManager influenceManager; //Este es el GESTOR que contiene los 3 mapas de influencia. Se accede a los mapas Globales a traves del Gestor

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


    [Header("Apariencia por Bando")]
    public SpriteRenderer spriteRenderer;
    public SkinFaction[] factionSkins;

    [Header("Estructuras donde se puede construir")]
    public StructureTpe[] estructurasConstruibles;
    [Header("GridManager")]
    private HexGridManager gridManager;

    void Awake()
    {
        influenceManager = FindAnyObjectByType<TacticalInfluenceManager>();
        gridManager = FindAnyObjectByType<HexGridManager>();
    } 

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

    public SkinFaction GetFactionSkin(Faction playerFaction)
    {
        if (factionSkins != null)
        {
            foreach (var skin in factionSkins)
            {
                if (skin.faction == playerFaction)
                {
                    return new SkinFaction
                    {
                        faction = playerFaction,
                        factionTroopName = !string.IsNullOrEmpty(skin.factionTroopName) ? skin.factionTroopName : name,
                        factionDescription = !string.IsNullOrEmpty(skin.factionDescription) ? skin.factionDescription : descripcion,
                        sprite = skin.sprite
                    };
                }
            }
        }

        // Si no tiene skin configurada para esa facción, devuelve los datos base
        return new SkinFaction
        {
            faction = playerFaction,
            factionTroopName = name,
            factionDescription = descripcion,
            sprite = spriteRenderer != null ? spriteRenderer.sprite : null
        };
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

    private void OnDestroy()
    {
        // Liberar la casilla si la tropa es destruida
        if (currentCell != null)
            currentCell.ClearOccupant(this);
    }
    public bool Initialize(HexCell startingCell, Player troopOwner)
    {
        if (startingCell == null)
        {
            Debug.LogError("No se puede crear la tropa: la casilla inicial es nula.");
            Destroy(gameObject);
            return false;
        }

        //HAY QUE COMPROBAR SI LA TROPA SE PUEDE PONER EN ESE TERRENO DE LA CASILLA ELEJIDA
        if (!TerrenoValido(startingCell))
        {
            Debug.Log("No se puede crear la tropa seleccionada en ese tipo de terreno");
            currentCell = null;
            Destroy(gameObject);
            return false;
        }

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
            return false;
        }

        currentCell = startingCell;
        owner = troopOwner;

        transform.position = currentCell.transform.position;

        Debug.Log("Tropa creada en " + currentCell.Coordinates);

        //Consumimos los materiales:    
        owner.creditos -= creditos;
        owner.chatarra -= chatarra;
        owner.tritio -= tritio;
        owner.cristal -= cristales;

        return true;
    }

    public void MoveToCell(HexCell targetCell, HexGridManager hexGridManager)
    {

        if (currentCell == null || targetCell == null)
        {
            Debug.LogWarning("Movimiento imposible: no existe una casilla válida.");
            return;
        }

        if (!TerrenoValido(targetCell))
        {
            Debug.Log("No se puede mover la tropa a ese terreno");
            return;
        }

        //Comprobar que la casilla sea válida
        List<HexCoordinates> validPositions = PathfindingDijkstra.GetValidPositions(hexGridManager, this, influenceManager);//targetCell.Coordinates,
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
        if (targetCell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) //Ha entrado en un agujero de gusano:
        {
            if (targetCell.destino != null) //Está conectado com otro agujero de gusano:
            {
                currentCell = targetCell.destino;
                transform.position = targetCell.destino.transform.position;
            }
        }
        else
        {
            currentCell = targetCell;
            transform.position = targetCell.transform.position;
        }

        Debug.Log("Tropa movida a " + currentCell.Coordinates);
    }

    private bool TerrenoValido(HexCell casilla)
    {
        foreach (HexTerrainType terrenoNoValido in impassableTerrains)
        {
            if (terrenoNoValido == casilla.TerrainType)
            {
                return false;
            }
        }

        return true;
    }
}

