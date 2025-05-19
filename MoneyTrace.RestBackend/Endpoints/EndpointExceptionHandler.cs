using System.Net;
using Microsoft.AspNetCore.Diagnostics;

public static class EndpointExceptionHandler
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.Run(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                if (contextFeature.Error is UnauthorizedAccessException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message
                    });
                }
                else
                {
                    //Propertly convert exceptions to HttpStatusCode
                    Console.WriteLine(contextFeature.Error);
                    if (contextFeature.Error is BadHttpRequestException badRequest)
                        context.Response.StatusCode = badRequest.StatusCode;
                    if (contextFeature.Error is ArgumentException || contextFeature.Error is ArgumentNullException || contextFeature.Error is ArgumentOutOfRangeException)
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message
                    });
                }
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Internal Server Error. Please try again later."
                });
            }
        });
    }
}