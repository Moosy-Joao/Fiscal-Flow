using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using FiscalFlow.Api.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace FiscalFlow.Api.Security;

public static class SecurityFeature
{
    public const string AuthorizationPolicyName =
        "fiscalflow-api";

    public const string RateLimitPolicyName =
        "fiscalflow-rate-limit";

    public static ApiSecurityOptions AddSecurityFeature(
        this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection(ApiSecurityOptions.SectionName)
            .Get<ApiSecurityOptions>()
            ?? new ApiSecurityOptions();

        Validate(options);

        builder.Services.AddSingleton(options);

        var signingKey = CreateSigningKey(options);

        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.SaveToken = false;
                jwt.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = options.Issuer,
                        ValidateAudience = true,
                        ValidAudience = options.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.FromSeconds(
                            options.ClockSkewSeconds),
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };

                jwt.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        return WriteProblemAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "Autenticação necessária",
                            "Forneça um token JWT Bearer válido.",
                            context.HttpContext.RequestAborted);
                    },
                    OnForbidden = context =>
                        WriteProblemAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "Acesso negado",
                            "A identidade autenticada não possui as claims exigidas.",
                            context.HttpContext.RequestAborted)
                };
            });

        builder.Services.AddAuthorization(authorization =>
        {
            authorization.AddPolicy(
                AuthorizationPolicyName,
                policy =>
                {
                    policy.AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("sub");
                    policy.RequireClaim(
                        TenantMiddleware.TenantClaimType);
                });
        });

        builder.Services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            rateLimiter.OnRejected = async (
                context,
                cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    context.HttpContext.Response.Headers[
                        "Retry-After"] = Math.Ceiling(
                            retryAfter.TotalSeconds)
                        .ToString(
                            CultureInfo.InvariantCulture);
                }

                await WriteProblemAsync(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Limite de requisições excedido",
                    "Aguarde antes de realizar novas requisições.",
                    cancellationToken);
            };

            rateLimiter.AddPolicy(
                RateLimitPolicyName,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ResolvePartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit =
                                options.RateLimitPermitLimit,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(
                                options.RateLimitWindowSeconds)
                        }));
        });

        return options;
    }

    private static SymmetricSecurityKey CreateSigningKey(
        ApiSecurityOptions options)
    {
        if (options.Enabled)
        {
            return new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.SigningKey!));
        }

        return new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(32));
    }

    private static string ResolvePartitionKey(
        HttpContext context)
    {
        return context.User.FindFirstValue("sub")
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType =
            "application/problem+json";

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["correlationId"] =
                        context.TraceIdentifier
                }
            },
            cancellationToken);
    }

    private static void Validate(
        ApiSecurityOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer)
            || string.IsNullOrWhiteSpace(options.Audience)
            || options.ClockSkewSeconds < 0
            || options.ClockSkewSeconds > 300
            || options.RateLimitPermitLimit <= 0
            || options.RateLimitWindowSeconds <= 0)
        {
            throw new InvalidOperationException(
                "A seção Security não foi configurada corretamente.");
        }

        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey)
            || Encoding.UTF8.GetByteCount(
                options.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Security:SigningKey deve possuir ao menos 32 bytes quando a segurança estiver habilitada.");
        }
    }
}
