using UnityEngine;

public class HexNode
{
    public HexCoordinates coordinates;
    public int priority;
    public int currentMovementPoints;
    public HexNode parent;

    public HexNode(HexCoordinates coordinates, int priority, int currentMovementPoints, HexNode parent)
    {
        this.coordinates = coordinates;
        this.priority = priority;
        this.currentMovementPoints = currentMovementPoints;
        this.parent = parent;
    }
}
