using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TurnsManager : MonoBehaviour
{
    //Gestión de turnos:
    [Header("Botón cambio de turno")]
    [SerializeField] private Button botonCambioTurno;
    [SerializeField] private TextMeshProUGUI textoDelBoton;

    [Header("Interfaz")]
    [SerializeField] private TextMeshProUGUI textoDeFase;
    [SerializeField] private TextMeshProUGUI textoDeTurno;

    [Header("UI de Transición de Fase")]
    [SerializeField] private CanvasGroup panelTransicion;
    [SerializeField] private TextMeshProUGUI textoTransicion;
    [SerializeField] private float duracionAnimacion = 0.8f; // Tiempo total del efecto

    public enum Fase{
        Construccion,
        Movimiento,
        Ataque
    }

    private bool enTurno;
    private Fase faseActual;
    private bool estaEnTransicion = false; // Bloqueo de seguridad
    public static bool EstaEnTransicion { get; private set; }

    [Header("Referencia IA")]
    [SerializeField] private AIPlayer jugadorEnemigo;

    // Propiedad publica para que el GameController sepa de quien es el turno
    public bool EsTurnoJugador => enTurno;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActButton()
    {
        switch (faseActual)
        {
            case Fase.Construccion:
                ActualizarBoton("Mover tropas", Color.blue);
                break;

            case Fase.Movimiento:
                ActualizarBoton("Atacar", Color.red);
                break;

            case Fase.Ataque:
                ActualizarBoton("Pasar turno", Color.green);
                break;        

            default:
                ActualizarBoton("ERROR", Color.gray);
                break;    
        }
    }

    private void ActualizarBoton(string texto, Color colorBase)
    {
        textoDelBoton.text = texto;
        //ActFaseTexto();

        ColorBlock cb = botonCambioTurno.colors;

        // 1. Color en reposo
        cb.normalColor = colorBase;

        // 2. Color al pasar el ratón por encima (puedes usar el mismo o matizarlo)
        cb.highlightedColor = Color.Lerp(colorBase, Color.white, 0.3f);

        // 3. Color al hacer clic
        cb.pressedColor = Color.Lerp(colorBase, Color.black, 0.3f);

        // Asegurarnos de que el multiplicador de color del bloque esté en 1
        cb.colorMultiplier = 1f;

        // 4. Color cuando el botón se queda seleccionado/enfocado (¡ESTE ES EL QUE SE QUEDABA EN BLANCO!)
        cb.selectedColor = colorBase;

        // Reasignamos la estructura al botón
        botonCambioTurno.colors = cb;

        //Quitar el foco de selección del EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void PressButton()
    {
        if (estaEnTransicion) return;
        switch (faseActual)
        {
            case Fase.Construccion:
                faseActual = Fase.Movimiento;
                break;
            case Fase.Movimiento:
                faseActual = Fase.Ataque;
                break;
            case Fase.Ataque:
                faseActual = Fase.Construccion;
                enTurno = !enTurno; // Aqui cambia de turno
                
                // --- LOGICA DE LA IA TEMPORAL PA QUE SE MUEVA ---
                if (!enTurno && jugadorEnemigo != null)
                {
                    // Ejecuta los movimientos aleatorios de la IA
                    jugadorEnemigo.PerformRandomMoves();
                    
                    //Para que el turno vuelva al jugador automáticamente 
                    // después de que la IA juegue, descomentamos la siguiente línea:
                     StartCoroutine(DevolverTurnoAlJugador());
                }
                break;        
            default:
                textoDelBoton.text = "ERROR";
                break;    
        }

        // Iniciar la minianimación
        StartCoroutine(AnimarTransicionFase());
    }

    private IEnumerator AnimarTransicionFase()
    {
        estaEnTransicion = true;
        EstaEnTransicion = true;

        // 1. Bloquear interacción con toda la pantalla
        panelTransicion.blocksRaycasts = true;

        // 2. Establecer el texto de la nueva fase en pantalla
        if (faseActual == Fase.Construccion)
        {
            if (enTurno) textoTransicion.text = "TU TURNO\nFASE DE " + faseActual.ToString().Replace('_', ' ').ToUpper();
            else textoTransicion.text = "TURNO DEL ENEMIGO\nFASE DE " + faseActual.ToString().Replace('_', ' ').ToUpper();
        }
        else textoTransicion.text = "FASE DE " + faseActual.ToString().Replace('_', ' ').ToUpper();
        
        // 3. Fundido de entrada (Aparece el panel y texto)
        float tiempo = 0f;
        float mitadDuracion = duracionAnimacion / 2f;

        while (tiempo < mitadDuracion)
        {
            tiempo += Time.deltaTime;
            panelTransicion.alpha = Mathf.Lerp(0f, 1f, tiempo / mitadDuracion);
            yield return null; // Espera al siguiente frame
        }
        panelTransicion.alpha = 1f;

        // Actualizamos el botón justo a mitad del efecto
        ActButton();
        ActFaseTexto();
        ActTurnoTexto();

        // 4. Fundido de salida (Desaparece el panel y texto)
        tiempo = 0f;
        while (tiempo < mitadDuracion)
        {
            tiempo += Time.deltaTime;
            panelTransicion.alpha = Mathf.Lerp(1f, 0f, tiempo / mitadDuracion);
            yield return null;
        }
        panelTransicion.alpha = 0f;

        // 5. Desbloquear pantalla para volver al juego normal
        panelTransicion.blocksRaycasts = false;
        estaEnTransicion = false;
        EstaEnTransicion = false;
    }

    private void ActFaseTexto()
    {
        textoDeFase.text = "Fase de " + faseActual.ToString();
    }

    private void ActTurnoTexto()
    {
        if (enTurno) textoDeTurno.text = "Tu turno";
        else textoDeTurno.text = "Turno del enemigo";
    }

    private IEnumerator DevolverTurnoAlJugador()
    {
        // Espera un poco para que se vea qué ha hecho la IA
        yield return new WaitForSeconds(1.5f); 
        PressButton(); // Simula que la IA pulsa el botón para devolverte el turno
    }
}
