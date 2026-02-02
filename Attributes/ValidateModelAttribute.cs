using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BankLite.Api.Attributes;

/// <summary>
/// Validates model state before controller action executes
/// Prevents invalid input from reaching business logic
/// </summary>
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new
                {
                    field = x.Key,
                    message = e.ErrorMessage
                }))
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                error = "Validation failed",
                errors
            });
        }
    }
}
