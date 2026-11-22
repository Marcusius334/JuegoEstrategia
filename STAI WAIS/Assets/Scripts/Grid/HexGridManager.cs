using NUnit.Framework;
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

    //private HexCoordinates destinoAgujero;
    private HexCell agujeroDeGusano1;
    private HexCell agujeroDeGusano2;

    [Header("Prefabs de estructuras")]
    [SerializeField] private Structure macronavePrefab;
    [SerializeField] private Structure planetaPrefab;
    [SerializeField] private Structure asteroidesPrefab;
    [SerializeField] private Structure kyberPrefab;

    [Header("Configuración de generación")]
    [SerializeField] private int minStructuresPerType = 2; //salvo el Kyber, el resto de estreucturas se generan de 1 a 3 estructuras
    [SerializeField] private int maxStructuresPerType = 3;

    // Probabilidad relativa de aparecer en su terreno favorito
    [SerializeField] private int preferredTerrainWeight = 5; //He hecho que todas las estructuras aparezcan en todos los terrenos pero que tengan tendencia a aparecer en el "suyo"
    // Probabilidad relativa de aparecer en otros terrenos
    [SerializeField] private int otherTerrainWeight = 1;

    private void Start()
    {
        RegistrarGrid();

        bool primerAgujeroConectado = false;

        foreach (HexCell cell in cells.Values)
        {
            cell.Initialize(this, cell.Coordinates);
            cell.LoadTerrain(this);

            if (cell.Terrain == null)
            {
                Debug.LogError(
                    "No se pudo cargar el terreno de: " + cell.Coordinates
                );
            }

            else if (cell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano)
            {
                if (agujeroDeGusano1 == null) //NO hay otro agujero de gusano encontrado aún:
                {
                    //destinoAgujero = cell.Coordinates;
                    agujeroDeGusano1 = cell;
                }

                else if (agujeroDeGusano2 == null) //NO hay otro agujero de gusano encontrado aún:
                {
                    //destinoAgujero = cell.Coordinates;
                    agujeroDeGusano2 = cell;
                }

                else //Ya se ha encontrado el otro agujero de gusano:
                {
                    if (!primerAgujeroConectado)
                    {
                        //Creamos las coorrelaciones:
                        cell.SetDestinoAgujeroDeGusano(agujeroDeGusano1);
                        agujeroDeGusano1.SetDestinoAgujeroDeGusano(cell);
                    }
                    else
                    {
                        //Creamos las coorrelaciones:
                        cell.SetDestinoAgujeroDeGusano(agujeroDeGusano2);
                        agujeroDeGusano2.SetDestinoAgujeroDeGusano(cell);
                    }
                    /* //Creamos las coorrelaciones:
                    cell.SetDestinoAgujeroDeGusano(agujeroDeGusano);
                    agujeroDeGusano.SetDestinoAgujeroDeGusano(cell);
                    
                    //Reiniciamos las variables:
                    //destinoAgujero = new HexCoordinates(0, 0);
                    agujeroDeGusano = null; */
                }
            }
        }
        GenerarEstructuras();
        Debug.Log("Casillas registradas: " + cells.Count);
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
        Debug.Log("Generando tablero...");
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

    public bool CoordinateInWorld(HexCoordinates coordinates) 
    {
        cells.TryGetValue(coordinates, out HexCell cell);
        return cell is not null;
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

    public void DesSelectCell()
    {
        selectedCell.SetSelected(false);
        selectedCell = null;
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
        return CreateTerrain(HexTerrainType.TerrenoEspacio);
    }
    //Forma de obtener el tipo de terreno
    public Terrain GetTerrain(HexCoordinates coordinates)
    {
        HexCell hex = GetCell(coordinates);
        //return hex.Terrain;
        return hex != null ? hex.Terrain : null;
    }

    //////////////////////////////////////////////////////////////
    /////////////////CREACION DE ESTRUCTURAS//////////////////////
    //////////////////////////////////////////////////////////////
    private void GenerarEstructuras() 
    {
        // Evitamos duplicar estructuras si se vuelve a ejecutar
        LimpiarEstructuras();

        // El centro del tablero, en coordenadas axiales, es (0,0)
        HexCoordinates centro = new HexCoordinates(0, 0);
        HexCell celdaCentro = GetCell(centro);
        if (celdaCentro == null) Debug.Log("No se ha encontrado la casilla central");
        else { CrearEstructuraEnCelda(celdaCentro, kyberPrefab); }

        GenerarEstructurasAleatorias(macronavePrefab, HexTerrainType.TerrenoEscombros);
        GenerarEstructurasAleatorias(planetaPrefab,HexTerrainType.TerrenoOrbitaPlanetaria);
        GenerarEstructurasAleatorias(asteroidesPrefab,HexTerrainType.TerrenoAsteroides);
        Debug.Log("Generación de estructuras completada.");
    }
    private void GenerarEstructurasAleatorias(Structure prefab, HexTerrainType terrenoPreferido) 
    {
        if (prefab == null) return;
        int cantidad = Random.Range(minStructuresPerType, maxStructuresPerType + 1);//Cantidad aleatoria de 1 a 3
        //casillas disponibles 
        List<HexCell> celdasDisponibles = new List<HexCell>();
        foreach(HexCell cell in cells.Values) 
        {
            // No permitimos colocar las estructuras aleatorias en el centro,porque el centro está reservado para el Kyber.
            if (cell.Coordinates.Q == 0 && cell.Coordinates.R == 0) continue;
            // No colocamos estructuras en casillas que ya tienen otra
            if (cell.HasStructure) continue;
            //No ponemos estructuras en aujeros de gusano
            if (cell.TerrainType == HexTerrainType.TerrenoAgujeroDeGusano) continue;
            celdasDisponibles.Add(cell);
        }
        //Si hay menos casillas disponibles que estructuras solicitadas, reducimos la cantidad para evitar errores.
        cantidad = Mathf.Min(cantidad, celdasDisponibles.Count);
        for (int i = 0; i < cantidad; i++) 
        {
            HexCell celdaElegida = ElegirCeldaPonderada(celdasDisponibles, terrenoPreferido);
            if (celdaElegida == null) break;
            CrearEstructuraEnCelda(celdaElegida, prefab);
            //Evitamos repetir la casilla elegida
            celdasDisponibles.Remove(celdaElegida);
        }

    }
    private HexCell ElegirCeldaPonderada(List<HexCell> celdasDisponibles, HexTerrainType terrenoPreferido) 
    {
        if(celdasDisponibles == null||celdasDisponibles.Count == 0) return null;
        int pesoTotal = 0;
        //Calculamos el peso total de todas las casillas 
        foreach (HexCell cell in celdasDisponibles) 
        {
            if (cell.TerrainType == terrenoPreferido)
                pesoTotal += preferredTerrainWeight;
            else
                pesoTotal += otherTerrainWeight;
        }
        if (pesoTotal <= 0) return celdasDisponibles[Random.Range(0, celdasDisponibles.Count)];
        //Elegimos un número aleatorio dentro del peso total 
        int numeroAleatorio = Random.Range(0, pesoTotal);
        //Recorremos las casillas hasta encontrar la seleccionada
        foreach (HexCell cell in celdasDisponibles) 
        {
            int peso;
            if (cell.TerrainType == terrenoPreferido) peso = preferredTerrainWeight;
            else peso = otherTerrainWeight;

            numeroAleatorio -= peso;
            if (numeroAleatorio < 0) return cell;
        }
        // Caso de seguridad
        return celdasDisponibles[celdasDisponibles.Count - 1];
    }
    private void CrearEstructuraEnCelda(HexCell cell, Structure prefab) 
    {
        if(cell == null||prefab == null) return;
        if (cell.HasStructure) return;
        //Crear el prefab en la posicion del hexagono 
        Structure nuevaEstructura = Instantiate(prefab, cell.transform.position, Quaternion.identity, cell.transform);
        //Se mantiene como hijo de la cailla
        nuevaEstructura.transform.localPosition = Vector3.zero;
        //Registrar la estructura dentro de la casilla
        if (!cell.TrySetStructure(nuevaEstructura)) 
        {
            Destroy(nuevaEstructura.gameObject);
            return;
        }
        Debug.Log(
        "Estructura " + nuevaEstructura.StructureType +
        " creada en " + cell.Coordinates +
        " sobre el terreno " + cell.TerrainType
        );

        //Damos a conocer a las casillas colindantes de la estructura:
        for (int i = 0; i < 6; i++)
        {
            HexCell casilla;
            HexCoordinates coordenadas = cell.Coordinates.GetNeighbor(i);
            if (CoordinateInWorld(coordenadas)) {
                casilla = GetCell(coordenadas);
                casilla.AddNeighborStructure(nuevaEstructura.StructureType);
            }
        }
    }
    private void LimpiarEstructuras() 
    {
        foreach (HexCell cell in cells.Values) 
        {
            if(cell.HasStructure) cell.ClearStructure();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Generar tablero en escena")]
    private void GenerateGridInEdtor()
    {
        // Comprobar que el HexGridManager pertenece a la escena
        if (UnityEditor.EditorUtility.IsPersistent(this))
        {
            Debug.LogError(
                "No se puede generar el tablero desde un prefab del Project. " +
                "Selecciona el HexGridManager de la Hierarchy."
            );
            return;
        }

        // Eliminar el tablero anterior
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        cells.Clear();
        selectedCell = null;

        // Generar el nuevo tablero
        GenerateGrid();

        Debug.Log("Tablero generado correctamente en la escena.");
    }

#endif

}
