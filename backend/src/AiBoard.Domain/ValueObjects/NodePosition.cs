namespace AiBoard.Domain.ValueObjects;

public sealed class NodePosition
{
    public decimal X { get; private set; }
    public decimal Y { get; private set; }

    private NodePosition()
    {
    }

    public NodePosition(decimal x, decimal y)
    {
        X = x;
        Y = y;
    }
}
