using UnityEngine;

public class HumanPlayer : Player
{
private Troop selectedTroop;
    [SerializeField] private HexGridManager gridManager;

    void Update()
    {
        // Solo actuar si es el turno del jugador y estamos en la fase de Movimiento
        // (Hacer pública la variable faseActual en TurnsManager o crear un método Get)
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Troop clickedTroop = hit.collider.GetComponent<Troop>();
            HexCell clickedCell = hit.collider.GetComponent<HexCell>();

            // 1. Si hace clic en una de sus tropas, la selecciona
            if (clickedTroop != null && clickedTroop.owner == this)
            {
                selectedTroop = clickedTroop;
                Debug.Log("Tropa seleccionada.");
            }
            // 2. Si tiene una tropa seleccionada y hace clic en una casilla, intenta moverla
            else if (selectedTroop != null && clickedCell != null)
            {
                selectedTroop.MoveToCell(clickedCell);
                selectedTroop = null; // Deseleccionar después de mover
            }
        }
    }
    //Funcion 1 (probablemente sea un OnMouseDown o similar)
        //Tiene que poder tocar a las tropas
        //Comprobar si es suyaa
        //Y entonces se marca como seleccionada la tropa
        //Se pulsa que otra casilla a la que quiere mover la tropa
        //Se activa la funcion de la tropa de moverse a esa casilla
    //=====================================

    //Funcion 2 (probablemente sea un OnMouseDown o similar)
        //Tiene que poder tocar las estaciones
        //Comprobar si es suya
        //Y entonces se entra al menu de comprar tropas
        //Se pulsa las tropas a comprar
        //Cada vez que se compre una tropa se activa la funcion comprar tropas de la estacion
    //=====================================
}
