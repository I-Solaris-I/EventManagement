using EventManagement.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EventManagement.Filters
{
    /// <summary>
    /// Кастомный фильтр исключений
    /// </summary>
    public class BusinessExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<BusinessExceptionFilter> _logger;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="problemDetailsFactory">Фабрика для создания problemDetails</param>
        public BusinessExceptionFilter(ILogger<BusinessExceptionFilter> logger, ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }
        /// <summary>
        /// Переопределённый метод
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            var httpContext = context.HttpContext;
            if (context.Exception is EventNotFoundedExeption nf)
            {
                _logger.LogError(nf, "Event not founded exception, EventId={EventId},Method={Method}, Path={Path}",
                    nf.EventId,
                    httpContext.Request.Method,
                    httpContext.Request.Path);
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext, StatusCodes.Status404NotFound, "Not Found");
                context.HttpContext.Response.ContentType = "application/problem+json";

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status404NotFound,
                };
                context.ExceptionHandled = true;
            }
            if (context.Exception is ArgumentNullException ane)
            {
                _logger.LogError(ane, "Argument null exception, ParamName={ParamName}, Method={Method}, Path={Path}",
                    ane.ParamName,
                    httpContext.Request.Method,
                    httpContext.Request.Path);
                var problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext, StatusCodes.Status400BadRequest, "Argument is Null");
                context.HttpContext.Response.ContentType = "application/problem+json";

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status404NotFound,
                };
                context.ExceptionHandled = true;
            }
            if (context.Exception is ValidationException dt)
            {
                _logger.LogWarning(dt, "Validation exception, Method={Method}, Path={Path}",
                   httpContext.Request.Method,
                   httpContext.Request.Path
                   );
                var msd = new ModelStateDictionary();
                if (dt.Errors is not null)
                {
                    foreach (var elem in dt.Errors)
                    {
                        msd.AddModelError(elem.PropertyName, elem.ErrorMessage);

                    }
                }

                var validationProblemDetails = _problemDetailsFactory.CreateValidationProblemDetails(httpContext, msd, StatusCodes.Status400BadRequest);
                context.HttpContext.Response.ContentType = "application/problem+json";
                context.Result = new ObjectResult(validationProblemDetails)
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
                context.ExceptionHandled = true;

            }
        }
    }
}
