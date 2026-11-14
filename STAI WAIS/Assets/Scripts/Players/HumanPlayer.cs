using UnityEngine;

public class HumanPlayer : Player
{
    public Troop selectedTroop;
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

        if (turnsManager.faseActual == TurnsManager.Fase.Construccion)
        {
            Construir(hit);
        }
        else if (turnsManager.faseActual == TurnsManager.Fase.Movimiento)
        {
            Movimiento(hit);
        }
    }

    // =========================================
    //
    // FASE 1: CONSTRUCCION
    //
    // =========================================
    private void Construir(RaycastHit2D hit)
    {
        if (selectedTroop == null) return;
            Debug.Log("Hay tropa seleccionada: " + selectedTroop.name);
            if (hit.collider != null)
            {
                HexCell clickedCell = hit.collider.GetComponent<HexCell>();
                
                if (clickedCell != null)
                {
                    Debug.Log("Hay casilla sleccionada");

                    Troop nuevaTropa = Instantiate(selectedTroop);
                    nuevaTropa.Initialize(clickedCell, this);
                }
            }
    }

    // =========================================
    //
    // FASE 2: MOVIMIENTO
    //
    // =========================================
    private void Movimiento(RaycastHit2D hit)
    {
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

