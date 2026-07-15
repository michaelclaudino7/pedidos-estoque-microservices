using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Pedidos.Api;

/// <summary>
/// Ponto único de tratamento de exceções não capturadas da API.
/// Erros de validação de domínio (ArgumentException) viram 400 com mensagem clara;
/// qualquer outra coisa vira 500 genérico (sem vazar detalhes internos pro cliente).
/// </summary>
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
        var (statusCode, title, detail) = exception switch
        {
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Requisicao invalida",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu um erro inesperado ao processar a requisicao.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro nao tratado ao processar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("Requisicao invalida em {Method} {Path}: {Message}", httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
