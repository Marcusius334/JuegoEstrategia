using UnityEngine;

public class AlaAoInterceptor : Troop
{
    private void Reset()
    {
        troopType = TroopType.AoInterceptor;
        
        // Estadísticas base
        attack = 200;
        hp = 450;
        maxhp=450;
        velocity = 8;
        range=2;

        // Costes
        creditos = 0;
        chatarra = 300;
        tritio = 0;
        cristales = 0;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { targetType = TroopType.YoBombardero, extraDamage = 100 },
            new BonusDamageTroop { targetType = TroopType.MartilloODefensor, extraDamage = -50 }
        };

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-A",
                factionDescription = "Ala eficiente para destruir ties avanzados" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Interceptor TIE",
                factionDescription = "Tie util para neutralizar Alas-B" 
            }
        };
    }
}

    

