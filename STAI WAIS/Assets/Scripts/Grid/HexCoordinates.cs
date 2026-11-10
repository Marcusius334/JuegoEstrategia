using UnityEngine;

public struct HexCoordinates //Representa la posicion de cada hexagono dentro del tablero
{
    //Coordenadas Axiales
    public int Q { get; private set; }//coordenada horizontal
    public int R { get; private set; }//coordenada vertical

    //Contructor 
    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    //Las 6 direcciones posibles
    public static readonly HexCoordinates[] Directions =
        { new HexCoordinates(1, 0), //Derecha
        new HexCoordinates(1, -1), //Derecha + arriba
        new HexCoordinates(0, 1), //Derecha + abajo
        new HexCoordinates(-1, 0), //Izquierda 
        new HexCoordinates(-1, 1), //Izquierda + abajo
        new HexCoordinates(0, -1),}; //Izquierda + arriba

    public HexCoordinates GetNeighbor(int direction) //Obtiene las coordenadas de un vecino en una direcci�n concreta
    {
        return new HexCoordinates(Q + Directions[direction].Q, R + Directions[direction].R); //Obtiene las coordenadas x e y + la direccion en x e y de su vecino
    }
    public int DistanceTo(HexCoordinates other) //Calcula la distancia entre dos hex�gonos con la formula: 
        // d = ( |Q1-Q2| + |R1-R2| + |(Q1-Q2) +(R1-R2)| ) / 2
    {
        int Qdistance = Q - other.Q;
        int Rdistance = R - other.R;

        return (Mathf.Abs(Qdistance) + Mathf.Abs(Rdistance) + Mathf.Abs(Qdistance + Rdistance)) / 2;
    }
    public override string ToString()//Convierte las coordenadas en texto legible
    {
        return $"({Q}, {R})";
    }
}
