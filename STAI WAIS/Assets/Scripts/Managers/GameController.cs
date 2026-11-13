using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Referencias")]
    public Troop troopPrefab; 
    public Player currentPlayer; 
    public TurnsManager turnsManager; // Asegúrate de que arrastraste el TurnsManager aquí en el Inspector

    private Troop activeTroop; 

    void Update()
    {
        // --- BLOQUEO ESTRICTO ---
        // Si no hay turnsManager asignado, o estamos en transición, o NO es nuestro turno -> salimos del Update
        if (turnsManager == null || TurnsManager.EstaEnTransicion || !turnsManager.EsTurnoJugador)
        {
            return; // Esto "corta" el código y evita que lea los clics del ratón
        }

        // Detectar clic izquierdo (Solo llega hasta aquí si es el turno del jugador)
        if (Input.GetMouseButtonDown(0))
        {
            if (troopPrefab == null) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);

            if (hitCollider != null)
            {
                HexCell clickedCell = hitCollider.GetComponent<HexCell>();
                
                if (clickedCell != null)
                {
                    if (activeTroop == null)
                    {
                        activeTroop = Instantiate(troopPrefab);
                        activeTroop.Initialize(clickedCell, currentPlayer);
                    }
                    else
                    {
                        activeTroop.MoveToCell(clickedCell);
                    }
                }
            }
        }
    }
}