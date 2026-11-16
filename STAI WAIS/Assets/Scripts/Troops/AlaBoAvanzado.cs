using UnityEngine;

public class AlaBoAvanzado : Troop
{
    private void Reset()
    {
        troopType = TroopType.BoAvanzado;
        
        // Estadísticas base
        attack = 250;
        hp = 1200;
        maxhp=1200;
        velocity = 6;
        range=2;

        // Costes
        creditos = 0;
        chatarra = 1200;
        tritio = 70;
        cristales = 0;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { allShips = true, extraDamage = 150 }
        };

        defenseBonuses= new BonusDefenseTerrain[]
        {
            new BonusDefenseTerrain{ terrain = HexTerrainType.TerrenoAsteroides, value=-100},
            new BonusDefenseTerrain{ terrain = HexTerrainType.TerrenoNebulosa, value=100}
        };

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-B",
                factionDescription = "Una Ala diseñada para ser efectiva contra las naves"
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Tie avanzado",
                factionDescription = "Un tie hecho para arrasar contra las naves enemigas" 
            }
        };
    }
}
