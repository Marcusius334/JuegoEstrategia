using UnityEngine;

public class AlaYoBombarderio : Troop
{
    private void Reset()
    {
        troopType = TroopType.YoBombardero;
        
        // Estadísticas base
        attack = 250;
        hp = 800;
        maxhp=800;
        velocity = 4;
        range=1;

        // Costes
        creditos = 150;
        chatarra = 700;
        tritio = 0;
        cristales = 0;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { targetType = TroopType.YoBombardero, extraDamage = 0 }
        };

        defenseBonuses= new BonusDefenseTerrain[]
        {
            new BonusDefenseTerrain{ terrain = HexTerrainType.TerrenoEscombros, value=100}
        };

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-Y",
                factionDescription = "Efectivo para destruir naves y estructuras de gran calibre" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Bombardero tie",
                factionDescription = "Efectivo para destruir naves y estructuras de gran calibre" 
            }
        };
    }

    public override int CalculateDamageAgainst(Troop target)
    {
        int damage = base.CalculateDamageAgainst(target);

        if (target.maxhp > 1000)
        {
            damage += 200; // Daño adicional
        }

        return damage;
    }
}
