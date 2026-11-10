using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HexGridManager : MonoBehaviour //Administra todo el tablero
{
    [Header("Configuracion del tablero")]
    public GameObject hexPrefab; //prefab del hexagono
    public int mapRadius = 6; //Radio del tablero N = 1 + 3R(R+1) donde N = numero de casillas y R el radio
    public float hexSize = 1f; //tama�o del hexagono y separacion entre los centros
    
    private Dictionary<HexCoordinates, HexCell> cells = new Dictionary<HexCoordinates, HexCell>(); //almacena las casillas del tablero
    private HexCell selectedCell;

    private void Start()
    {
        RegistrarGrid();
        foreach (HexCell cell in cells.Values) 
        {
            cell.Initialize(this,cell.Coordinates);
            cell.LoadTerrain(this);
        }
    }
    private void RegistrarGrid() 
    {
        cells.Clear();
        HexCell[] existingCells = GetComponentsInChildren<HexCell>();
        foreach (HexCell cell in existingCells) 
        {
            if(!cells.ContainsKey(cell.Coordinates))
                cells.Add(cell.Coordinates, cell);
        }
    }
    //Generar el tablero
    public void GenerateGrid() 
    {
        for (int q = -mapRadius; q <= mapRadius; q++) //recorrer coordenada Q. Reccore desde -mapRadius hasta +mapRadius -> Ej: -5,-4,-3...3,4,5
        {
            //Calcular los l�mites de R para crear tablero hexagonal en lugar de cuadrado
            int MinRadius = Mathf.Max(-mapRadius, -q - mapRadius);
            int MaxRadius = Mathf.Min(mapRadius, -q + mapRadius);

            for (int r = MinRadius; r <= MaxRadius; r++) 
            {
                HexCoordinates coordinates = new HexCoordinates(q, r);
                Vector3 position = AxialToWorld(coordinates); //convertir las coordenadas a posicion de Unity


                //Crear una copia del prefab
                GameObject hexObject = Instantiate(hexPrefab, position, Quaternion.identity, transform);

                //Busca el componente HexCell dentro del objeto que acabamos de crear.
                //As� podemos acceder a sus m�todos y variables.
                HexCell cell = hexObject.GetComponent<HexCell>();

                cell.Initialize(this, coordinates);

                Terrain terrain = GenerateTerrain();
                cell.SetTerrain(terrain);

                cells.Add(coordinates, cell); //A�adimos al diccionario
            }
        }
    }
    public Vector3 AxialToWorld(HexCoordinates hex) //convierte coordenadas axiales a coordenadas de Unity
    {
        float x = hexSize * Mathf.Sqrt(3f) * (hex.Q + hex.R / 2f);
        float y = hexSize * 1.5f * hex.R;
        return transform.position + new Vector3(x, y, 0);
    }
    public HexCell GetCell(HexCoordinates coordinates) 
    {
        cells.TryGetValue(coordinates, out HexCell cell);
        return cell;
    }
    public void SelectCell(HexCell cell) 
    {
        //Quitar la seleccion anterior
        if (selectedCell != null) { selectedCell.SetSelected(false); }
        
        //Guardar la nueva seleccion
        selectedCell = cell;

        //activar visualmente la nueva seleccion 
        selectedCell.SetSelected(true);
        Debug.Log("Casilla seleccionada: " + cell.Coordinates);
    }

    //////////////////////////////////////////////////////////////
    /////////////////CREACION DE TERRENOS/////////////////////////
    //////////////////////////////////////////////////////////////
    public Terrain CreateTerrain(HexTerrainType type) //
    {
        switch (type) //se crean los terrenos con los valores: Typo de terreno, Coste de movimiento, Bono de Defensa, Color del terreno 
        {
            case HexTerrainType.TerrenoEspacio:
                return new Terrain(type, 1, 0, new Color(0.79f, 0.92f, 0.92f)); //Azul oscuro
            case HexTerrainType.TerrenoOrbitaPlanetaria:
                return new Terrain(type, 2, 0,new Color(0.2f, 0.45f, 0.85f)); //Azul claro
            case HexTerrainType.TerrenoAsteroides:
                return new Terrain(type, 3, 1,new Color(0.70f, 0.58f, 0.43f)); //Marrón
            case HexTerrainType.TerrenoEscombros:
                return new Terrain(type, 5, 3,new Color(0.45f, 0.45f, 0.45f)); //Gris
            case HexTerrainType.TerrenoNebulosa:
                return new Terrain(type, 3, -1,new Color(0.50f, 0.38f, 0.85f)); //Morado
            case HexTerrainType.TerrenoAgujeroDeGusano:
                return new Terrain(type, 0, 0,new Color(0.0f, 0.0f, 0.0f)); //NEGRO
            default:
                return null;
        }
    }
    private Terrain GenerateTerrain() //Generador aleatorio de terrenos. Por ahora los terrenos se generan aleatoriamente. Con (más o menos), la misma probabilidad de que se generen. Cuando el mapa esté diseñado lo cambiaré.
    {
        float random = Random.Range(0f, 100f);
        if (random < 16f)
            return CreateTerrain(HexTerrainType.TerrenoEspacio);
        if (random < 32f)
            return CreateTerrain(HexTerrainType.TerrenoOrbitaPlanetaria);
        if (random < 48f)
            return CreateTerrain(HexTerrainType.TerrenoAsteroides);
        if (random < 64f)
            return CreateTerrain(HexTerrainType.TerrenoEscombros);
        if (random < 80)
            return CreateTerrain(HexTerrainType.TerrenoNebulosa);
        return CreateTerrain(HexTerrainType.TerrenoAgujeroDeGusano);



    }

#if UNITY_EDITOR
    [ContextMenu("Generar tablero en escena")]
    private void GenerateGridInEdtor() 
    {
        //Eliminar tablero anterior
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        cells.Clear();
        selectedCell = null;

        //Generar el nuevo Tablero
        GenerateGrid();
    }

#endif
}
