using UnityEngine;

public class HumanPlayer : Player
{
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
    public Troop selectedTroop;
    private Troop selectedAttackTroop;
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
                    selectedTroop.MoveToCell(clickedCell, gridManager);
                    selectedTroop = null; 
                }
            }
    }

    // =========================================
    // FASE 3: ATAQUE
    // =========================================

    private void Ataque(RaycastHit2D hit)
    {
        if (hit.collider == null) return;

        // Comprobar si hemos pulsado una tropa
        Troop clickedTroop = hit.collider.GetComponentInParent<Troop>();

        if (clickedTroop == null) return;

        // 1. Si no tenemos ninguna tropa seleccionada,
        // seleccionamos una tropa propia.

        if (selectedAttackTroop == null)
        {
            if (clickedTroop.owner == this)
            {
                selectedAttackTroop = clickedTroop;
                Debug.Log("Tropa atacante seleccionada: " + selectedAttackTroop.name);
            }

            return;
        }

        // 2. Si pulsamos otra tropa propia,
        // cambiamos la selección.

        if (clickedTroop.owner == this)
        {
            selectedAttackTroop = clickedTroop;
            Debug.Log("Nueva tropa atacante seleccionada: " + selectedAttackTroop.name);
            return;
        }

        // 3. Comprobar que tenemos una tropa atacante válida.

        if (selectedAttackTroop.currentCell == null || clickedTroop.currentCell == null)
        {
            Debug.LogWarning("No se puede atacar: alguna tropa no tiene casilla.");
            return;
        }

        // 4. Calcular distancia entre las dos casillas.

        int distancia = selectedAttackTroop.currentCell.Coordinates.DistanceTo(clickedTroop.currentCell.Coordinates);

        // 5. Comprobar rango de ataque.

        if (distancia > selectedAttackTroop.range)
        {
            Debug.Log(
                "Objetivo fuera de alcance. Distancia: " +
                distancia + " | Rango: " + selectedAttackTroop.range
            );

            return;
        }

        // 6. Ejecutar ataque.

        Debug.Log(selectedAttackTroop.name + " ataca a " +clickedTroop.name);

        selectedAttackTroop.Attack(clickedTroop);
    }

    

}

