using UnityEngine;

public class HexCell : MonoBehaviour //Representa cada hex�gono inividual que aparece en el tablero
{
    public HexCoordinates Coordinates { get; private set; } //Guarda las coordenadas de la casilla
    public Terrain Terrain { get; private set; }

    private HexGridManager grid;//Administrador del tablero
    private SpriteRenderer spriteRenderer;

    [Header("Colores de la casilla")]//el ultimo digito es para la componente opacidad
    public Color normalColor = new Color(1f, 1f, 1f, 0.15f);
    public Color hoverColor = new Color(0f, 0f, 0f, 0.25f);
    public Color selectedColor = new Color(0f, 0f, 0f, 0.40f);
    
    private bool isSelected = false;
    private bool isHovered = false;

    public void Initialize(HexGridManager gridManager, HexCoordinates coordinates)
    {
        grid = gridManager;
        Coordinates = coordinates;
        name = $"Hex_{coordinates.Q}_{coordinates.R}"; //Nombre del objeto de Unity

        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    private void OnMouseEnter()//cuadno el raton entra en la casilla
    {
        if (TurnsManager.EstaEnTransicion) return;
        isHovered = true;
        UpdateVisual();
    }
    private void OnMouseExit()//cuando el at�n sale de la casilla
    {
        if (TurnsManager.EstaEnTransicion) return;
        isHovered = false;
        UpdateVisual();
    }
    private void OnMouseDown()//Detectar los clics del rat�n sobre las celdas
    {
        if (TurnsManager.EstaEnTransicion) return;
        if (grid != null)
        {
            grid.SelectCell(this);
        }
    }
    public void SetSelected(bool selected) 
    {
        isSelected = selected;
        UpdateVisual();
    }
    private void UpdateVisual() 
    {
        if(spriteRenderer == null || Terrain == null) return;

        if (isSelected) { spriteRenderer.color = Color.Lerp(Terrain.TerrainColor, selectedColor, 0.5f); }
        else if (isHovered) { spriteRenderer.color = Color.Lerp(Terrain.TerrainColor, hoverColor, 0.5f); }
        else { spriteRenderer.color = Terrain.TerrainColor; }
    }
    public void SetTerrain(Terrain terrain) 
    {
        Terrain = terrain;
        UpdateVisual();
    }
}
