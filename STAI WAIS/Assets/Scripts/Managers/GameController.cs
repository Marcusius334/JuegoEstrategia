using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Referencias")]
    public Troop troopPrefab; // Arrastra tu prefab del inspector aquí
    public Player currentPlayer; // Arrastra el jugador actual (si tienes un objeto Player)

    private Troop activeTroop; // La tropa que estamos controlando en pantalla

    void Update()
    {
        // Detectar clic izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);

            if (hitCollider != null)
            {
                HexCell clickedCell = hitCollider.GetComponent<HexCell>();
                
                if (clickedCell != null)
                {
                    // Si aún no hemos creado la tropa, la instanciamos en la casilla clicada
                    if (activeTroop == null)
                    {
                        activeTroop = Instantiate(troopPrefab);
                        activeTroop.Initialize(clickedCell, currentPlayer);
                    }
                    // Si ya existe, le ordenamos moverse
                    else
                    {
                        activeTroop.MoveToCell(clickedCell);
                    }
                }
            }
        }
    }
}