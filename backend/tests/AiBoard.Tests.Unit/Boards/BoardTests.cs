using AiBoard.Domain.Entities;
using AiBoard.Domain.Enums;
using AiBoard.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AiBoard.Tests.Unit.Boards;

public sealed class BoardTests
{
    [Fact]
    public void AddNode_ShouldTrackNodeInsideBoard()
    {
        var board = new Board("MVP board");
        var node = new BoardNode(board.Id, NodeType.Prompt, "Prompt", "Create a hero image", new NodePosition(100, 200));

        board.AddNode(node);

        board.Nodes.Should().ContainSingle().Which.Id.Should().Be(node.Id);
    }
}
