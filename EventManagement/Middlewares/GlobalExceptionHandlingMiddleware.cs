using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EventManagement.Middlewares
{
    /// <summary>
    /// Класс, содержащий метод-расширение для подключения глобального обработчика исключений
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtension
    {
        /// <summary>
        /// Подключение глобального обработчика исключений
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
    /// <summary>
    /// Промежуточный слой ПО для обработки исключений
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="next">Делегат следующего элемента в цепочке конвейера</param>
        /// <param name="logger">Логгер</param>
        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, ProblemDetailsFactory problemDetailsFactory)
        {
            _next = next;
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;

        }
        /// <summary>
        /// Метод входа 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                ///Вызов следующего элемента в цепочке конвейера
                await _next(context);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unhandeled exception, Method={Method}, Path={Path}",
                    context.Request.Method, context.Request.Path);
                if (context.Response.HasStarted)
                {
                    return;
                }
                var statusCode = StatusCodeMapping(ex);
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(_problemDetailsFactory.CreateProblemDetails(context, statusCode, "Unhandeled exception", detail: ex.Message));
                

            }
        }
        private static int StatusCodeMapping(Exception ex)
                 => ex switch
                 {
                     _ => StatusCodes.Status500InternalServerError
                 };
    }
}
