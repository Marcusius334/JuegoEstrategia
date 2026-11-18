using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class HexCell : MonoBehaviour //Representa cada hex�gono inividual que aparece en el tablero
{
    [SerializeField] private int coordinatesQ;
    [SerializeField]private int coordinatesR;
    [SerializeField] private HexTerrainType terrainType;
    public HexCoordinates Coordinates => new HexCoordinates(coordinatesQ, coordinatesR); //Guarda las coordenadas de la casilla
    public HexTerrainType TerrainType => terrainType;//guarda que terreno tiene la propia casilla
    public Terrain Terrain { get; private set; }

    // Tropa que ocupa actualmente esta casilla
    private Troop occupyingTroop;
    public Troop OccupyingTroop => occupyingTroop;

    private SpriteRenderer outline;

    // Comprobar si la casilla está ocupada
    public bool IsOccupied => occupyingTroop != null;
    

    //ESTRUCTURAS
    private Structure occupyingStructure;
    public Structure OccupyingStructure => occupyingStructure;
    public bool HasStructure => occupyingStructure != null;

    void Awake()
    {
        outline = transform.GetChild(0).GetComponent<SpriteRenderer>();
        outline.enabled = false;
    }
    // Intentar colocar una tropa en esta casilla
    public bool TrySetOccupant(Troop troop)
    {
        if (troop == null) return false;

        // Si otra tropa ya ocupa la casilla, no permitimos colocarla
        if (occupyingTroop != null && occupyingTroop != troop)
            return false;

        occupyingTroop = troop;
        return true;
    }

    // Liberar la casilla cuando una tropa se marcha
    public void ClearOccupant(Troop troop)
    {
        if (occupyingTroop == troop)
        occupyingTroop = null;
    }

    private HexGridManager grid;//Administrador del tablero
    private SpriteRenderer spriteRenderer;

    [Header("Colores de la casilla")]//el ultimo digito es para la componente opacidad
    public Color normalColor = new Color(1f, 1f, 1f, 0.15f);
    public Color hoverColor = new Color(0f, 0f, 0f, 0.25f);
    public Color selectedColor = new Color(0f, 0f, 0f, 0.40f);
    
    private bool isSelected = false;
    private bool isHovered = false;
    private Transform tr;

    public HexCell destino; //Solo para agujeros de gusano. 

    public void Initialize(HexGridManager gridManager, HexCoordinates coordinates)
    {
        grid = gridManager;
        coordinatesQ = coordinates.Q;
        coordinatesR = coordinates.R;

        name = $"Hex_{coordinates.Q}_{coordinates.R}"; //Nombre del objeto de Unity

        spriteRenderer = GetComponent<SpriteRenderer>();

        tr = GetComponent<Transform>();

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
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Hay algún botón que está encima de la casilla
            return;
        }

        if (TurnsManager.EstaEnTransicion) return;
        if (grid != null)
        {
            if (isSelected)
            {
                grid.DesSelectCell();
                //UpdateVisual();
                CameraController.targetFocusPosition = Vector3.zero;
            }
            else
            {
                grid.SelectCell(this);
                Debug.Log("Se ha seleccionado");
                //UpdateVisual();
                CameraController.targetFocusPosition = tr.position;
            }
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
        else if (isHovered) { spriteRenderer.color = Color.Lerp(spriteRenderer.color, hoverColor, 0.5f); }
        else { spriteRenderer.color = Terrain.TerrainColor; }
    }

    public void EnableBorder(bool enable)
    {
        outline.enabled = enable;
    }
    public void SetTerrain(Terrain terrain) 
    {
        Terrain = terrain;
        terrainType = terrain.Type;

        // Obtener el SpriteRenderer si todavía no está asignado
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        // Aplicar directamente el color del terreno
        spriteRenderer.color = terrain.TerrainColor;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(spriteRenderer);
#endif
    }
    public void LoadTerrain(HexGridManager grid) 
    {
        Terrain = grid.CreateTerrain(terrainType);
        UpdateVisual();
    }

    public void SetDestinoAgujeroDeGusano(HexCell c)
    {
        if (terrainType != HexTerrainType.TerrenoAgujeroDeGusano) return;

        destino = c;
    }


    ////////////////////////////////////////////////
    /// ESTRUCTURAS
    ///////////////////////////////////////////////
    public bool TrySetStructure(Structure structure) 
    {
        if(structure == null) return false;
        //Noo permitimos tener 2 estructuras en la misma casilla
        if(occupyingStructure != null) return false;

        occupyingStructure = structure;
        return true;
    }
    public void ClearStructure() 
    {
        if (occupyingStructure != null) 
        {
            Destroy(occupyingStructure.gameObject);
            occupyingStructure = null;
        }
    }
}
