using AiBoard.Api.Contracts.Boards;
using AiBoard.Application.Boards.Commands;
using AiBoard.Application.Boards.Queries;
using AiBoard.Application.Jobs.Commands;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiBoard.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/boards")]
[ApiVersion("1.0")]
public sealed class BoardsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var board = await sender.Send(new CreateBoardCommand(request.Name, request.Description), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { version = "1.0", boardId = board.Id }, board);
    }

    [HttpGet("{boardId:guid}")]
    public async Task<IActionResult> GetById(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await sender.Send(new GetBoardByIdQuery(boardId), cancellationToken);
        return board is null ? NotFound() : Ok(board);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var boards = await sender.Send(new GetBoardsQuery(), cancellationToken);
        return Ok(boards);
    }

    [HttpPost("{boardId:guid}/nodes")]
    public async Task<IActionResult> AddNode(Guid boardId, [FromBody] CreateBoardNodeRequest request, CancellationToken cancellationToken)
    {
        var node = await sender.Send(
            new AddBoardNodeCommand(boardId, request.Type, request.Title, request.Content, request.X, request.Y, request.Model),
            cancellationToken);

        return Ok(node);
    }

    [HttpPut("{boardId:guid}/nodes/{nodeId:guid}")]
    public async Task<IActionResult> UpdateNode(
        Guid boardId,
        Guid nodeId,
        [FromBody] UpdateBoardNodeRequest request,
        CancellationToken cancellationToken)
    {
        var node = await sender.Send(
            new UpdateBoardNodeCommand(boardId, nodeId, request.Title, request.Content, request.X, request.Y, request.Model),
            cancellationToken);

        return Ok(node);
    }

    [HttpPost("{boardId:guid}/edges")]
    public async Task<IActionResult> AddEdge(Guid boardId, [FromBody] CreateBoardEdgeRequest request, CancellationToken cancellationToken)
    {
        var edge = await sender.Send(
            new AddBoardEdgeCommand(boardId, request.SourceNodeId, request.TargetNodeId, request.Label),
            cancellationToken);

        return Ok(edge);
    }

    [HttpPost("{boardId:guid}/generations")]
    public async Task<IActionResult> QueueGeneration(Guid boardId, [FromBody] QueueGenerationRequest request, CancellationToken cancellationToken)
    {
        var jobId = await sender.Send(
            new QueueNodeGenerationCommand(boardId, request.NodeId, request.Provider, request.Prompt),
            cancellationToken);

        return Accepted(new { jobId });
    }

    [HttpPost("{boardId:guid}/nodes/{nodeId:guid}/generate")]
    public async Task<IActionResult> GenerateNode(
        Guid boardId,
        Guid nodeId,
        [FromBody] QueueGenerationRequest request,
        CancellationToken cancellationToken)
    {
        if (nodeId != request.NodeId)
        {
            return BadRequest("Route nodeId does not match request body nodeId.");
        }
        // Prefer queueing generation to be processed asynchronously by the Worker.
        var jobId = await sender.Send(
            new AiBoard.Application.Jobs.Commands.QueueNodeGenerationCommand(boardId, nodeId, request.Provider, request.Prompt),
            cancellationToken);

        return Accepted(new { jobId });
    }

    [HttpDelete("{boardId:guid}/nodes/{nodeId:guid}")]
    public async Task<IActionResult> DeleteNode(Guid boardId, Guid nodeId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteBoardNodeCommand(boardId, nodeId), cancellationToken);
        return NoContent();
    }
}
