namespace MoneyTrace.Application.Common;

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("Handling request: {@RequestName} {@Request}", requestName, request);

        TResponse response;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            response = await next();
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Handled request: {@RequestName}, Execution time: {@ExecutionTime}ms", requestName, stopwatch.ElapsedMilliseconds);
        }
        return response;
    }
}