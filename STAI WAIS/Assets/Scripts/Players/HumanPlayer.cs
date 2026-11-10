using UnityEngine;

public class HumanPlayer : Player
{
    private Troop selectedTroop;
    [SerializeField] private HexGridManager gridManager;

    void Start(){
        chatarra = gameManager.j_chatarra;
        creditos = gameManager.j_creditos;
        tritio = gameManager.j_tritio;
        cristal = gameManager.j_cristal;
    }
    
    // --- NUEVO: Necesitamos leer el turno ---
    public TurnsManager turnsManager;

    void Update()
    {
        // --- BLOQUEO ESTRICTO DE TURNO ---
        if (turnsManager == null || TurnsManager.EstaEnTransicion || !turnsManager.EsTurnoJugador) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Troop clickedTroop = hit.collider.GetComponent<Troop>();
            HexCell clickedCell = hit.collider.GetComponent<HexCell>();

            if (clickedTroop != null && clickedTroop.owner == this)
            {
                selectedTroop = clickedTroop;
                Debug.Log("Tropa seleccionada.");
            }
            else if (selectedTroop != null && clickedCell != null)
            {
                selectedTroop.MoveToCell(clickedCell);
                selectedTroop = null; 
            }
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

