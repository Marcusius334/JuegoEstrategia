using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

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
    private List<HexCoordinates> validPositions;//Cosa para bordear y desbordear las casillas a las que se mueve
    [SerializeField] private HexGridManager gridManager;
    [SerializeField] private TacticalInfluenceManager influenceMap;

    [Header("Interfaz de usuario")]
    [SerializeField] private TextMeshProUGUI textoCreditos;
    [SerializeField] private TextMeshProUGUI textoChatarra;
    [SerializeField] private TextMeshProUGUI textoTritio;
    [SerializeField] private TextMeshProUGUI textoCristales;

    void Start(){
        validPositions = new List<HexCoordinates>{};
    }
    
    // --- NUEVO: Necesitamos leer el turno ---
    public TurnsManager turnsManager;

    void Update()
    {
        // --- BLOQUEO ESTRICTO DE TURNO ---
        if (turnsManager == null || TurnsManager.EstaEnTransicion || !turnsManager.EsTurnoJugador) return;

        ActText();

        if (!Input.GetMouseButtonDown(0)) return;

        // --- BLOQUEO SI EL RATÓN ESTÁ SOBRE UN BOTÓN O VENTANA DE LA UI ---
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

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

    private void ActText()
    {
        textoCreditos.text = "Creditos: " + creditos.ToString();
        textoChatarra.text = "Chatarra: " + chatarra.ToString();
        textoTritio.text = "Tritio: " + tritio.ToString();
        textoCristales.text = "Cristales Kayber: " + cristal.ToString();
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

                    //Comprobamos si se intenta construir sobre una estructura:
                    if (clickedCell.HasStructure) return;
                    
                    //Comprobamos si hay estructura válida:
                    bool sePuedeConstruir = false;
                    foreach (StructureTpe estructura in selectedTroop.estructurasConstruibles) {
                        if (clickedCell.ComprobarEstructuraCercana(estructura)) sePuedeConstruir = true;
                    }

                    if (!sePuedeConstruir) return;

                    //Creamos la nave:
                    Troop nuevaTropa = Instantiate(selectedTroop);
                    if (nuevaTropa.Initialize(clickedCell, this))
                    {
                        //Reseteamos valores:
                        selectedTroop = null;
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    }
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
        //reseteo de los bordes de las posiciones valid
        if(validPositions.Count > 0)
        {
            Debug.Log("entra");
            foreach(HexCoordinates position in validPositions)
            {
                HexCell hex = gridManager.GetCell(position);
                Debug.Log("Hexexexe");
                hex.EnableBorder(false);
            }
        }
        
        if (hit.collider != null)
        {
            Debug.Log("Testing mover tropas: se ha clickado algo");

            Troop clickedTroop = hit.collider.GetComponent<Troop>();

            Debug.Log("Cosa clicada -> "+hit);

            if (clickedTroop == null) Debug.Log("Testing mover tropas: No se ha encontrado tropa");
            else Debug.Log("Testing mover tropas: Se ha encontrado tropa: ");

            HexCell clickedCell = hit.collider.GetComponent<HexCell>();

            if (clickedTroop != null && clickedTroop.owner == this)
            {
                selectedTroop = clickedTroop;
                Debug.Log("Testing mover tropas: Tropa seleccionada: " + selectedTroop.name);
                validPositions = PathfindingDijkstra.GetValidPositions(gridManager, clickedTroop, influenceMap);
                foreach(HexCoordinates position in validPositions)
                {
                    HexCell hex = gridManager.GetCell(position);
                    hex.EnableBorder(true);
                }
            }
            else if (selectedTroop != null && clickedCell != null)
            {
                Debug.Log("Testing mover tropas: Casilla seleccionada");

                //Comprobamos si se intenta construir sobre una estructura:
                if (clickedCell.HasStructure) return;
                
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

