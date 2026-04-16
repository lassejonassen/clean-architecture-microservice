using CleanArchitecture.SharedKernel;
using CleanArchitecture.SharedKernel.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebAPI.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

    protected IActionResult HandleFailure(Error error)
    {
        // Define extensions (metadata)
        var extensions = new Dictionary<string, object?>
        {
            { "errorCode", error.Code } // Assuming your Error object has a unique code
        };

        return error.Type switch
        {
            ErrorType.Failure => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Failure",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                detail: error.Description,
                instance: HttpContext.Request.Path,
                extensions: extensions),

            ErrorType.Validation => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                detail: error.Description,
                instance: HttpContext.Request.Path,
                extensions: extensions),

            ErrorType.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                detail: error.Description,
                instance: HttpContext.Request.Path,
                extensions: extensions),

            ErrorType.Conflict => Problem(
               statusCode: StatusCodes.Status409Conflict,
               title: "Conflict",
               type: "https://tools.ietf.org/html/rfc7231#section-6.5.8",
               detail: error.Description,
               instance: HttpContext.Request.Path,
               extensions: extensions),

            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Server Error",
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                detail: "An unexpected error occurred.",
                instance: HttpContext.Request.Path,
                extensions: extensions)
        };
    }
}
