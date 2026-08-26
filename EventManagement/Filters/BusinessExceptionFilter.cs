using EventManagement.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventManagement.Filters
{
    /// <summary>
    /// Кастомный фильтр исключений
    /// </summary>
    public class BusinessExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// Переопределённый метод
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is EventNotFoundedExeption nf)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = nf.Message,
                    Instance = context.HttpContext.Request.Path,

                };
                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status404NotFound,
                };
                context.ExceptionHandled = true;
            }
            if (context.Exception is ValidationException dt)
            {
                var validationProblemDetails = new ValidationProblemDetails();
                validationProblemDetails.Instance = context.HttpContext.Request.Path;
                if (dt.Errors is not null)
                {
                    validationProblemDetails.Errors = dt.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(x => x.ErrorMessage).ToArray());
                }

                context.Result = new ObjectResult(validationProblemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
                context.ExceptionHandled = true;

            }



        }
    }
}
