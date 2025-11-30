using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevTrails___BankProject.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Ocorreu uma exceção: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            (int statusCode, string title) = exception switch
            {
                KeyNotFoundException
                    => (StatusCodes.Status404NotFound, "Recurso não encontrado"),


                UnauthorizedAccessException
                    => (StatusCodes.Status403Forbidden, "Acesso Negado"),

                ArgumentException
                    => (StatusCodes.Status400BadRequest, "Dados Inválidos"),

                InvalidOperationException
                    => (StatusCodes.Status409Conflict, "Regra de Negócio Violada"),

                DbUpdateException
                    => (StatusCodes.Status409Conflict, "Conflito de Integridade no Banco"),

                _ => (StatusCodes.Status500InternalServerError, "Erro Interno do Servidor")
            };

            problemDetails.Status = statusCode;
            problemDetails.Title = title;

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                problemDetails.Detail = "Ocorreu um erro crítico. Contate o suporte.";
            }
            else
            {
                if (exception is DbUpdateException)
                    problemDetails.Detail = "Não foi possível salvar os dados. Verifique duplicidade de registros.";
                else
                    problemDetails.Detail = exception.Message;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}