using UnityEngine;

public class HexNode
{
    public HexCoordinates coordinates;
    public float priority;
    public int currentMovementPoints;
    public HexNode parent;

    public HexNode(HexCoordinates coordinates, float priority, int currentMovementPoints, HexNode parent)
    {
        this.coordinates = coordinates;
        this.priority = priority;
        this.currentMovementPoints = currentMovementPoints;
        this.parent = parent;
    }

    public bool SameCoordinates(object obj)
    {
        if (obj is not HexCoordinates other)
            return false;

        return coordinates.Q == other.Q && coordinates.R == other.R;
    }

}
