using UnityEngine;

public class MartiillooDefensor : Troop
{
    private void Reset()
    {
        troopType = TroopType.MartilloODefensor;
        
        // Estadísticas base
        attack = 100;
        hp = 1600;
        maxhp=1600;
        velocity = 2;
        range=1;

        // Costes
        creditos = 500;
        chatarra = 1800;
        tritio = 60;
        cristales = 0;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { targetType = TroopType.YoBombardero, extraDamage = 10 }
        };

        defenseBonuses= new BonusDefenseTerrain[]
        {
            new BonusDefenseTerrain{ terrain = HexTerrainType.TerrenoEscombros, value=-100}
            
        };

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Cabeza de Martillo",
                factionDescription = "Una nave capaz de resistir muchos ataques pero hacer poco daño" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Tie Defensor",
                factionDescription = "Efectivo para aguantar numerosos ataques pero no provocara mucho daño" 
            }
        };
    }
}
