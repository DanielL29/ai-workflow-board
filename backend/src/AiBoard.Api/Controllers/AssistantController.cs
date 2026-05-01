using AiBoard.Api.Contracts.Boards;
using AiBoard.Application.Abstractions.AI;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AiBoard.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/assistant")]
[ApiVersion("1.0")]
public sealed class AssistantController(IAssistantOrchestrator assistantOrchestrator, MediatR.ISender sender) : ControllerBase
{
    [HttpPost("reply")]
    public async Task<IActionResult> Reply([FromBody] AssistantPromptRequest request, CancellationToken cancellationToken)
    {
        var response = await assistantOrchestrator.ReplyAsync(request.BoardId, request.Message, cancellationToken);
        return Ok(new { response });
    }

    [HttpPost("images")]
    public async Task<IActionResult> GenerateImage([FromBody] AssistantImageRequest request, CancellationToken cancellationToken)
    {
        if (request.BoardId is null)
        {
            return BadRequest("BoardId is required for image generation.");
        }

        // Create an image node
        var node = await sender.Send(
            new AiBoard.Application.Boards.Commands.AddBoardNodeCommand(
                request.BoardId.Value,
                AiBoard.Domain.Enums.NodeType.Image,
                "Image",
                request.Prompt,
                0m,
                0m,
                request.Provider),
            cancellationToken);

        try
        {
            var result = await sender.Send(
                new AiBoard.Application.Boards.Commands.GenerateBoardNodeCommand(request.BoardId.Value, node.Id, request.Provider ?? string.Empty, request.Prompt),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, title: "Image generation failed", statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
