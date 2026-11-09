using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGridManager))] //Quiero personalizar el Inspector de todos los objetos que tengan el componente HexGridManager.
public class HexMapPainter: Editor //hereda de editor
{
    private HexTerrainType selectedTerrain = HexTerrainType.TerrenoEspacio; //Guarda el tipo de terreno que tenemos seleccionado en el pincel
    private bool paintMode = false;

    public override void OnInspectorGUI()  //método epecial de Unity.
    {
        DrawDefaultInspector(); //Muestra el inspector normal de HexGridManager
        HexGridManager grid = (HexGridManager)target; //obtenemos el objeto que contenga el Script HexGridManager


        //Personalización para que se vea bien
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("PINCEL DE TERRENOS", EditorStyles.boldLabel);
        selectedTerrain = (HexTerrainType)EditorGUILayout.EnumPopup("Terreno seleccionado", selectedTerrain); //desplegable de terrenos y lo almacena en selectedTerrain

        paintMode = EditorGUILayout.Toggle("Activar modo pintura", paintMode);//para activar o desactivar el modo pintura
        if (GUILayout.Button("Pintar todo de espacio")) { PaintAll(grid, HexTerrainType.TerrenoEspacio); } //boton para pintar todo. Ejecuta la funcion PaintAll
        SceneView.RepaintAll();//Actualiza la escena
    }
    private void OnSceneGUI() //permite detectar y gestionar interacciones dentro de la ventana Scene
    {
        if (!paintMode) //Comprobar el modo pintura
            return;

        Event e = Event.current; //detecta eventos del raton

        if (e.type == EventType.MouseDown && e.button == 0) //comrpueba que ehmos pulsado el botón izquierdo
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);//sirve para comprobar que objeto está debajo del ratón

            RaycastHit2D hit = Physics2D.GetRayIntersection(ray); 

            if (hit.collider != null)
            {
                HexCell cell = hit.collider.GetComponent<HexCell>(); //obtiene la celda que hemos tocado

                if (cell != null) //si la celda no es null, la pinta
                {
                    PaintCell(cell);
                    e.Use();
                }
            }
        }
    }

    private void PaintCell(HexCell cell)
    {
        HexGridManager grid = (HexGridManager)target; //obtiene el administrador del tablero (el que tiene el Script HexGridManager)

        Undo.RecordObject(cell, "Pintar terreno"); //guarda el estado anterior de la casilla para poder utilizar Ctrl + Z

        cell.SetTerrain(grid.CreateTerrain(selectedTerrain)); //Crea y asigna el terreno a la casilla

        EditorUtility.SetDirty(cell); //indica a Unity que el objeto ha cambiado y que debe guardar esos cambios
    }

    private void PaintAll(HexGridManager grid, HexTerrainType terrain) //pinta todas las casillas
    {
        HexCell[] cells = grid.GetComponentsInChildren<HexCell>(); //obtiene todas las casillas del Grid

        foreach (HexCell cell in cells)
        {
            Undo.RecordObject(cell, "Pintar todos los terrenos");

            cell.SetTerrain(grid.CreateTerrain(terrain));

            EditorUtility.SetDirty(cell);
        }
    }
}
