using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Botones_tienda : MonoBehaviour
{
    [SerializeField] private GameObject ventana;
    [SerializeField] private GameObject boton;

    //public GameManager gameManager;

    [Header("Jugador y naves")]
    [SerializeField] private HumanPlayer player;
    public Troop[] troops;


    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoNombre;

    [SerializeField] private TextMeshProUGUI textoDescripcion;

    [SerializeField] private TextMeshProUGUI textoCreditos;
    [SerializeField] private TextMeshProUGUI textoChatarra;
    [SerializeField] private TextMeshProUGUI textoTritio;
    [SerializeField] private TextMeshProUGUI textoCristal;

    [SerializeField] private TextMeshProUGUI textoAttack;
    [SerializeField] private TextMeshProUGUI textoHp;
    [SerializeField] private TextMeshProUGUI textoVelocity;

    [Header("Marcos de los botones")]
    [SerializeField] private GameObject[] marcosBotones;

    [Header("Cursores")]
    public Texture2D[] texturaCursor;

    private int tier = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < 7; i++) marcosBotones[i].SetActive(false);
        ActScreen();
    }

    public void OnClick(int n)
    {
        if (n == tier) return;
        marcosBotones[tier].SetActive(false);
        tier = n;
        ActScreen();
    }

    public void OpenWindow()
    {
        //Confirmamos que no se queda una navea seleccionada:
        player.selectedTroop = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        //Abrimos la tienda y ocultamos el botón:
        ventana.SetActive(true);
        boton.SetActive(false);
    }

    public void CloseWindow()
    {
        boton.SetActive(true);
        ventana.SetActive(false);
    }

    public void Buy()
    {
        //Primero se comprueba si se puede comprar:
        if (player.creditos - troops[tier].creditos < 0 ||
            player.chatarra - troops[tier].chatarra < 0 ||
            player.tritio - troops[tier].tritio < 0 ||
            player.cristal - troops[tier].cristales < 0) return;

        //Consumimos los materiales:    
        player.creditos -= troops[tier].creditos;
        player.chatarra -= troops[tier].chatarra;
        player.tritio -= troops[tier].tritio;
        player.cristal -= troops[tier].cristales;

        //Actualizamos la nave seleccionada del jugador:
        player.selectedTroop = troops[tier];

        //Actualizamos el cursor:
        Texture2D textura = texturaCursor[tier];
        Vector2 centroCursor = new Vector2(textura.width / 2f, textura.height / 2f);
        Cursor.SetCursor(textura, centroCursor, CursorMode.Auto);
        
        //Cerramos la tienda:
        CloseWindow();
    }

    private void ActScreen()
    {
        Troop naveActual = troops[tier];

        if (player != null)
        {
            SkinFaction skin = naveActual.GetFactionSkin(player.faction);
            textoNombre.text = skin.factionTroopName;
            textoDescripcion.text = skin.factionDescription;
        }
        else
        {
            textoNombre.text = naveActual.name;
            textoDescripcion.text = naveActual.descripcion;
        }

        textoCreditos.text = naveActual.creditos.ToString();
        textoChatarra.text = naveActual.chatarra.ToString();
        textoTritio.text = naveActual.tritio.ToString();
        textoCristal.text = naveActual.cristales.ToString();

        textoAttack.text = naveActual.attack.ToString();
        textoHp.text = naveActual.hp.ToString();
        textoVelocity.text = naveActual.velocity.ToString();

        marcosBotones[tier].SetActive(true);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
