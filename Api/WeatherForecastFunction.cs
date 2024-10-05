using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Api;

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public IEnumerable<string>? UserRoles { get; set; }
}

public class SomeMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        // Use to extract requesting user.
        var req = await context.GetHttpRequestDataAsync();
        await next(context);
    }
}

public class HttpTrigger
{
    // https://azure.github.io/static-web-apps-cli/docs/cli/local-auth

    public static ClaimsPrincipal Parse(HttpRequestData req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
        {
            var data = header.FirstOrDefault();
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        //Common auth code used on the Blazor side to
        //get the Claims principal from ClientPrincipal
        principal.UserRoles =
            principal.UserRoles?.Except(new[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        if (!principal.UserRoles?.Any() ?? true)
        {
            return new ClaimsPrincipal();
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(identity);
    }

    private readonly ILogger _logger;

    public HttpTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HttpTrigger>();
    }

    [Function("protected")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var randomNumber = new Random();
        var temp = 0;

        var key = "x-ms-client-principal";

        var response = req.CreateResponse(HttpStatusCode.OK);
        _logger.LogInformation("HIT THAT SHIT");
        if (req.Headers.TryGetValues(key, out IEnumerable<string> headers))
        {
            var header = headers?.FirstOrDefault();
            _logger.LogInformation($"Got it: {header}");
            var decoded = Convert.FromBase64String(header);
            var json = Encoding.UTF8.GetString(decoded);
            var result = JsonSerializer.Deserialize<ClientPrincipal>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            response.WriteAsJsonAsync(result);
        }
        else
        {
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = temp = randomNumber.Next(-20, 55),
                Summary = GetSummary(temp)
            }).ToArray();
            response.WriteAsJsonAsync(result);
        }


        //response = req.CreateResponse(HttpStatusCode.OK);

        return response;
    }


    [Function("Public")]
    public HttpResponseData Public([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        //var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //{
        //	Date = DateTime.Now.AddDays(index),
        //	TemperatureC = temp = randomNumber.Next(-20, 55),
        //	Summary = GetSummary(temp)
        //}).ToArray();

        var key = "x-ms-client-principal";

        string principal;
        string authHeader;
        if (req.Headers.TryGetValues(key, out IEnumerable<string> headers))
        {
            principal = headers.FirstOrDefault();
        }

        if (req.Headers.TryGetValues("Authorization", out var x))
        {
            authHeader = x.FirstOrDefault();
        }

        var result = "ok from public";
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(result);

        return response;
    }

    private string GetSummary(int temp)
    {
        var summary = "Mild";

        if (temp >= 32)
        {
            summary = "Hot";
        }
        else if (temp <= 16 && temp > 0)
        {
            summary = "Cold";
        }
        else if (temp <= 0)
        {
            summary = "Freezing";
        }

        return summary;
    }
}