using UnityEngine;

public class AlaXoCaza : Troop
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Reset()
    {
        troopType = TroopType.XoCaza;
        
        // Estadísticas base
        attack = 150;
        hp = 600;
        maxhp=600;
        velocity = 4;
        range=1;

        // Costes
        creditos = 0;
        chatarra = 200;
        tritio = 0;
        cristales = 0;

        // Ventajas contra otras naves
        damageBonuses = new BonusDamageTroop[]
        {
            new BonusDamageTroop { targetType = TroopType.MartilloODefensor, extraDamage = -50 }
        };

        

        // Configuración por facción (luego solo arrastras el Sprite en el Inspector)
        factionSkins = new SkinFaction[]
        {
            new SkinFaction
            {
                faction = Faction.Rebeldes,
                factionTroopName = "Ala-X",
                factionDescription = "La nave más basica de los rebeldes" 
            },
            new SkinFaction
            {
                faction = Faction.Imperio,
                factionTroopName = "Caza Tie",
                factionDescription = "El tie más basico del imperio" 
            }
        };
    }
}
