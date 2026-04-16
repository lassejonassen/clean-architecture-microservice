using CleanArchitecture.Application.Templates.Commands;
using CleanArchitecture.Application.Templates.DTOs;
using CleanArchitecture.Application.Templates.Queries;
using CleanArchitecture.SharedKernel.Messaging;
using CleanArchitecture.WebAPI.Contracts.Templates;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebAPI.Controllers;

[Route("/api/templates")]
public class TemplatesController : BaseController
{
    [ProducesResponseType(typeof(TemplateListResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllTemplatesQuery();

        var result = await Mediator.Send(query, cancellationToken);

        if (!result.Any())
        {
            return NoContent();
        }

        var dtos = result.Select(x => new TemplateResponseDTO
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
        });

        var _result = new TemplateListResponseDTO
        {
            Data = dtos,
        };

        return Ok(_result);
    }

    [ProducesResponseType(typeof(TemplateResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTemplateByIdQuery(id);

        var result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        var _result = new TemplateResponseDTO
        {
            Id = result.Value.Id,
            Name = result.Value.Name,
            Description = result.Value.Description,
        };

        return Ok(_result);
    }

    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TemplateCreateRequestDTO request, CancellationToken cancellationToken)
    {
        var createDto = new CreateTemplateDTO
        {
            Name = request.Name,
            Description = request.Description,
        };

        var query = new CreateTemplateCommand(createDto);

        var result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value }, result.Value);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TemplateUpdateRequestDTO request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("Id in route does not match Id in body.");
        }

        var updateDto = new UpdateTemplateDTO
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
        };


        var query = new UpdateTemplateCommand(updateDto);

        var result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }


    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("The id cannot be empty");
        }


        var query = new DeleteTemplateCommand(id);

        var result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }
}
