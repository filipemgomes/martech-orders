using Martech.Orders.Api.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Martech.Orders.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(JwtTokenService tokenService) : ControllerBase
{
    private const string FixedEmail = "dev@martech.com";
    private const string FixedPassword = "Senha@123";

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (request.Email != FixedEmail || request.Password != FixedPassword)
            return Unauthorized();

        var (token, expiresAtUtc) = tokenService.GenerateToken(request.Email);
        return Ok(new LoginResponse(token, expiresAtUtc));
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);
