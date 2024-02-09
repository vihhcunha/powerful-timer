using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PowerfulTimer.Api.Models;

namespace PowerfulTimer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected ActionResult HandleResult(IErrorOr result, int successStatusCode = 200)
    {
        if (result.IsError)
        {
            return BadRequestWithErrorResult(result.Errors);
        }

        return StatusCode(successStatusCode, ResponseModelBuilder.CreateSuccessResponse());
    }

    protected ActionResult HandleResult<T>(ErrorOr<T> result, int successStatusCode = 200)
    {
        if (result.IsError)
        {
            return BadRequestWithErrorResult(result.Errors);
        }

        return StatusCode(successStatusCode, ResponseModelBuilder.CreateSuccessResponse(result.Value));
    }

    protected ActionResult HandleResult(ModelStateDictionary modelState)
    {
        var errors = new List<Error>();
        var modelStateErrors = modelState.Values.SelectMany(e => e.Errors);
        foreach (var modelStateError in modelStateErrors)
        {
            errors.Add(Error.Validation(description: modelStateError.ErrorMessage));
        }
        var error = ErrorOrExtensions.ToErrorOr(errors);

        return HandleResult(result: error);
    }

    private ActionResult BadRequestWithErrorResult(List<Error> errors)
    {
        return BadRequest(ResponseModelBuilder.CreateErrorResponse(CreateErrorResponseObject(errors)));
    }

    private List<ErrorResponseModel> CreateErrorResponseObject(List<Error> resultErrors)
    {
        var errors = new List<ErrorResponseModel>();
        foreach (var error in resultErrors)
        {
            errors.Add(new ErrorResponseModel
            {
                Message = error.Description
            });
        }
        return errors;
    }
}
