using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TurnsManager : MonoBehaviour
{
    [Header("Botón cambio de turno")]
    [SerializeField] private Button botonCambioTurno;
    [SerializeField] private TextMeshProUGUI textoDelBoton;

    [Header("Interfaz")]
    [SerializeField] private TextMeshProUGUI textoDeFase;
    [SerializeField] private TextMeshProUGUI textoDeTurno;

    [Header("UI de Transición de Fase")]
    [SerializeField] private CanvasGroup panelTransicion;
    [SerializeField] private TextMeshProUGUI textoTransicion;
    [SerializeField] private float duracionAnimacion = 0.8f;

    public enum Fase { Construccion, Movimiento, Ataque }
    private bool enTurno;
    public Fase faseActual;
    private bool estaEnTransicion = false;
    
    //Variable general para deshabilitar las casillas:
    public static bool EstaEnTransicion { get; private set; }

    [Header("Referencia IA")]
    [SerializeField] private AIPlayer jugadorEnemigo;

    public bool EsTurnoJugador => enTurno;

    private void BuscarAIPlayer()
{
    // Si ya tenemos una referencia, no hacemos nada
    if (jugadorEnemigo != null)
        return;

    // Buscar los AIPlayer existentes en la escena
    AIPlayer[] jugadoresIA = FindObjectsByType<AIPlayer>(
        FindObjectsSortMode.None
    );

    if (jugadoresIA.Length > 0)
    {
        jugadorEnemigo = jugadoresIA[0];

        Debug.Log(
            "TurnManager: AIPlayer encontrado automáticamente: "
            + jugadorEnemigo.gameObject.name
        );
    }
    else
    {
        Debug.LogError(
            "TurnManager: NO EXISTE ningún AIPlayer activo en la escena."
        );
    }
}

    void Start()
{
    BuscarAIPlayer();

    enTurno = true;
    faseActual = Fase.Construccion;

    // El resto de vuestro código continúa igual...

        
        enTurno = true;
        faseActual = Fase.Construccion;

        if (panelTransicion != null)
        {
            panelTransicion.alpha = 0f;
            panelTransicion.blocksRaycasts = false;
        }

        ActButton();
        ActFaseTexto();
        ActTurnoTexto();
    }

    private void ActButton()
    {
        switch (faseActual)
        {
            case Fase.Construccion: ActualizarBoton("Mover tropas", Color.blue); break;
            case Fase.Movimiento: ActualizarBoton("Atacar", Color.red); break;
            case Fase.Ataque: ActualizarBoton("Pasar turno", Color.green); break;
            default: ActualizarBoton("ERROR", Color.gray); break;
        }
    }

    private void ActualizarBoton(string texto, Color colorBase)
    {
        textoDelBoton.text = texto;
        ColorBlock cb = botonCambioTurno.colors;
        cb.normalColor = colorBase;
        cb.highlightedColor = Color.Lerp(colorBase, Color.white, 0.3f);
        cb.pressedColor = Color.Lerp(colorBase, Color.black, 0.3f);
        cb.colorMultiplier = 1f;
        cb.selectedColor = colorBase;
        botonCambioTurno.colors = cb;

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void PressButton()
    {
        if (estaEnTransicion) return;

        switch (faseActual)
        {
            case Fase.Construccion:
                faseActual = Fase.Movimiento;
                StartCoroutine(AnimarTransicionFase());
                break;
            case Fase.Movimiento:
                faseActual = Fase.Ataque;
                StartCoroutine(AnimarTransicionFase());
                break;
            case Fase.Ataque:
                enTurno = false; // Bloqueamos tus clics
                StartCoroutine(SecuenciaTurnoEnemigo()); // Lanzamos la secuencia de la IA
                break;
        }
    }

    
private IEnumerator SecuenciaTurnoEnemigo()
{
    // Buscar la IA por si no estaba asignada inicialmente
    BuscarAIPlayer();

    // =========================================
    // FASE 1: CONSTRUCCION
    // =========================================

    faseActual = Fase.Construccion;

    yield return StartCoroutine(AnimarTransicionFase());

    if (jugadorEnemigo != null)
    {
        jugadorEnemigo.PerformConstructionPhase();
    }

    yield return new WaitForSeconds(1f);

    // =========================================
    // FASE 2: MOVIMIENTO
    // =========================================

    faseActual = Fase.Movimiento;

    yield return StartCoroutine(AnimarTransicionFase());

    if (jugadorEnemigo != null)
    {
        jugadorEnemigo.PerformMovementPhase();
    }

    yield return new WaitForSeconds(1f);

    // =========================================
    // FASE 3: ATAQUE
    // =========================================

    faseActual = Fase.Ataque;

    yield return StartCoroutine(AnimarTransicionFase());

    if (jugadorEnemigo != null)
    {
        jugadorEnemigo.PerformAttackPhase();
    }

    yield return new WaitForSeconds(1f);

    // =========================================
    // DEVOLVER EL TURNO AL JUGADOR
    // =========================================

    enTurno = true;
    faseActual = Fase.Construccion;

    yield return StartCoroutine(AnimarTransicionFase());

    Debug.Log("TurnManager: Vuelve el turno del jugador.");
}


    private IEnumerator AnimarTransicionFase()
    {
        estaEnTransicion = true;
        EstaEnTransicion = true;
        panelTransicion.blocksRaycasts = true;

        if (faseActual == Fase.Construccion)
            textoTransicion.text = (enTurno ? "TU TURNO\n" : "TURNO DEL ENEMIGO\n") + "FASE DE " + faseActual.ToString().ToUpper();
        else
            textoTransicion.text = "FASE DE " + faseActual.ToString().ToUpper();
        
        float tiempo = 0f;
        float mitadDuracion = duracionAnimacion / 2f;

        while (tiempo < mitadDuracion)
        {
            tiempo += Time.deltaTime;
            panelTransicion.alpha = Mathf.Lerp(0f, 1f, tiempo / mitadDuracion);
            yield return null;
        }
        panelTransicion.alpha = 1f;

        ActButton();
        ActFaseTexto();
        ActTurnoTexto();

        tiempo = 0f;
        while (tiempo < mitadDuracion)
        {
            tiempo += Time.deltaTime;
            panelTransicion.alpha = Mathf.Lerp(1f, 0f, tiempo / mitadDuracion);
            yield return null;
        }
        panelTransicion.alpha = 0f;

        panelTransicion.blocksRaycasts = false;
        estaEnTransicion = false;
        EstaEnTransicion = false;
    }

    private void ActFaseTexto() { textoDeFase.text = "Fase de " + faseActual.ToString(); }
    private void ActTurnoTexto() { textoDeTurno.text = enTurno ? "Tu turno" : "Turno del enemigo"; }
}
