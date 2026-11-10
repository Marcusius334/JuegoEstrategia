using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Recursos iniciales del jugador humano")]
    public int j_creditos = 100;
    public int j_chatarra = 500;
    public int j_tritio = 20;
    public int j_cristal = 0;

    [Header("Recursos iniciales del jugador IA")]
    public int ia_creditos = 100;
    public int ia_chatarra = 500;
    public int ia_tritio = 20;
    public int ia_cristal = 0;

    [Header("Interfaz de usuario")]
    [SerializeField] private TextMeshProUGUI textoCreditos;
    [SerializeField] private TextMeshProUGUI textoChatarra;
    [SerializeField] private TextMeshProUGUI textoTritio;
    [SerializeField] private TextMeshProUGUI textoCristales;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ActText()
    {
        textoCreditos.text = "Creditos: " + j_creditos.ToString();
        textoChatarra.text = "Chatarra: " + j_chatarra.ToString();
        textoTritio.text = "Tritio: " + j_tritio.ToString();
        textoCristales.text = "Cristales Kayber: " + j_cristal.ToString();
    }
}
