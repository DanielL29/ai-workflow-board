using AiBoard.Api.Contracts.Boards;
using AiBoard.Application.Abstractions.AI;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AiBoard.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/boards/{boardId:guid}/memory")]
[ApiVersion("1.0")]
public sealed class BoardMemoryController(IBoardMemoryService boardMemoryService) : ControllerBase
{
    [HttpPost("documents")]
    public async Task<IActionResult> UpsertDocument(
        Guid boardId,
        [FromBody] UpsertBoardMemoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await boardMemoryService.UpsertDocumentAsync(
            boardId,
            request.SourceType,
            request.Title,
            request.Content,
            request.SourceNodeId,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        Guid boardId,
        [FromBody] SearchBoardMemoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await boardMemoryService.SearchAsync(boardId, request.Query, request.Limit, cancellationToken);
        return Ok(result);
    }
}
