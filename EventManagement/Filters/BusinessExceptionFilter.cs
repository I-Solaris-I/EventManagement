using EventManagement.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Data.SqlTypes;

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

        private void HandleProblemDetail(ExceptionContext context, int code, string? tile = null, string? details = null)
        {
            var problemDetails = _problemDetailsFactory.CreateProblemDetails(context.HttpContext, code, tile, detail: details);
            context.HttpContext.Response.ContentType = "application/problem+json";

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = code,
            };
            context.ExceptionHandled = true;
        }
        private void HandleValidationProblemDetail(ExceptionContext context, int code, IEnumerable<ValidationFailure> Errors)
        {
            var msd = new ModelStateDictionary();
            if (Errors is not null)
            {
                foreach (var elem in Errors)
                {
                    msd.AddModelError(elem.PropertyName, elem.ErrorMessage);

                }
            }

            var validationProblemDetails = _problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, msd, StatusCodes.Status400BadRequest);
            context.HttpContext.Response.ContentType = "application/problem+json";
            context.Result = new ObjectResult(validationProblemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
            context.ExceptionHandled = true;

        }
        /// <summary>
        /// Переопределённый метод
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            var httpContext = context.HttpContext;


            switch (context.Exception)
            {

                case EventNotFoundedException enf:
                    _logger.LogWarning(enf, "Event not founded exception, EventId={EventId}, Method={Method}, Path={Path}",
                        enf.EventId,
                        httpContext.Request.Method,
                        httpContext.Request.Path);
                    HandleProblemDetail(context, StatusCodes.Status404NotFound, "Not Found", enf.Message);
                    break;
                case BookingNotFoundedException bnf:
                    _logger.LogWarning(bnf, "Booking not founded exception, BookingId={BookingId}, Method={Method}, Path={Path}",
                         bnf.BookingId,
                         httpContext.Request.Method,
                         httpContext.Request.Path);
                    HandleProblemDetail(context, StatusCodes.Status404NotFound, "Not Found", bnf.Message);
                    break;
                case BookingBusinessException bbe:
                    {
                        _logger.LogWarning(bbe, "Booking Business Exception, Method={Method}, Path={Path}",
                          httpContext.Request.Method,
                          httpContext.Request.Path);
                        HandleProblemDetail(context, StatusCodes.Status400BadRequest, "Booking operation is not allowed", bbe.Message);
                        break;
                    }
                case ArgumentNullException ane:
                    {
                        _logger.LogWarning(ane, "Argument null exception, ParamName={ParamName}, Method={Method}, Path={Path}",
                            ane.ParamName,
                            httpContext.Request.Method,
                            httpContext.Request.Path);
                        HandleProblemDetail(context, StatusCodes.Status400BadRequest, "Argument is null", ane.Message);
                        break;


                    }
                case ValidationException ve:
                    _logger.LogWarning(ve, "Validation exception, Method={Method}, Path={Path}",
                         httpContext.Request.Method,
                         httpContext.Request.Path);
                    HandleValidationProblemDetail(context, StatusCodes.Status400BadRequest, ve.Errors);
                    break;




            }
        }
    }
}
