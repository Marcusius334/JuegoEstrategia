using UnityEngine;
using UnityEngine.EventSystems;

public class HumanPlayer : Player
{
    public Troop selectedTroop;
    [SerializeField] private HexGridManager gridManager;

    void Start(){
        ActualizarRecursos();
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

        if (turnsManager.faseActual == TurnsManager.Fase.Construccion)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (selectedTroop == null) return;
            Debug.Log("Hay tropa seleccionada: " + selectedTroop.name);
            if (hit.collider != null)
            {
                HexCell clickedCell = hit.collider.GetComponent<HexCell>();
                
                if (clickedCell != null)
                {
                    Debug.Log("Hay casilla sleccionada");

                    //Creamos la nave:
                    Troop nuevaTropa = Instantiate(selectedTroop);
                    nuevaTropa.Initialize(clickedCell, this);

                    //Reseteamos valores:
                    selectedTroop = null;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
        }
        else if (turnsManager.faseActual == TurnsManager.Fase.Movimiento)
        {
            if (hit.collider != null)
            {
                Debug.Log("Testing mover tropas: se ha clickado algo");

                Troop clickedTroop = hit.collider.GetComponent<Troop>();

                if (clickedTroop == null) Debug.Log("Testing mover tropas: No se ha encontrado tropa");
                else Debug.Log("Testing mover tropas: Se ha encontrado tropa: ");

                HexCell clickedCell = hit.collider.GetComponent<HexCell>();

                if (clickedTroop != null && clickedTroop.owner == this)
                {
                    selectedTroop = clickedTroop;
                    Debug.Log("Testing mover tropas: Tropa seleccionada: " + selectedTroop.name);
                }
                else if (selectedTroop != null && clickedCell != null)
                {
                    Debug.Log("Testing mover tropas: Casilla seleccionada");
                    selectedTroop.MoveToCell(clickedCell);
                    selectedTroop = null; 
                }
            }
        }
    }

    private void ActualizarRecursos()
    {
        chatarra = gameManager.j_chatarra;
        creditos = gameManager.j_creditos;
        tritio = gameManager.j_tritio;
        cristal = gameManager.j_cristal;
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

