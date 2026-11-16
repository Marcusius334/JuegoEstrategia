using UnityEngine;

public class AlaUoPhantom : Troop
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Reset()
    {
        troopType = TroopType.UoPhantom;
        
        // Estadísticas base
        attack = 300;
        hp = 1500;
        maxhp=1500;
        velocity = 6;
        range=1;

        // Costes
        creditos = 650;
        chatarra = 1800;
        tritio = 50;
        cristales = 1;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { allBuildings=true, extraDamage=250 }
        };

        

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-U",
                factionDescription = "Una de las naves más poderosas, efectiva contra edificios y naves grandes" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Tie Phantom",
                factionDescription = "Una de las naves más poderosas, efectiva contra edificios y naves grandes" 
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
