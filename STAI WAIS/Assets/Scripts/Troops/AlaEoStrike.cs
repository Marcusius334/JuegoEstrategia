using UnityEngine;

public class AlaEoStrike : Troop
{
    private void Reset()
    {
        troopType = TroopType.EoStrike;
        
        // Estadísticas base
        attack = 400;
        hp = 1200;
        maxhp=1200;
        velocity = 10;
        range=4;

        // Costes
        creditos = 800;
        chatarra = 2000;
        tritio = 80;
        cristales = 2;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { allShips=true, extraDamage=200 }
        };

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-E",
                factionDescription = "Ala muy efectivo contra todo tipo de naves enemigas" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Tie Strike",
                factionDescription = "Un tie megaOfensivo contra todo tipo de naves" 
            }
        };
    }
    public override int CalculateDamageAgainst(Troop target)
    {
        int damage = base.CalculateDamageAgainst(target);

        if (target.maxhp > 1000)
        {
            damage += 50; // Daño adicional
        }

        return damage;
    }
}
