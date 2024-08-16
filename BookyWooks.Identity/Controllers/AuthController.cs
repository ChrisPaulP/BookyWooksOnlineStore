using BookyWooks.Identity.Models;
using BookyWooks.Identity.Services;
using BookyWooks.Messaging.Messages.Events;
using BookyWooks.SharedKernel;
using IdentityServer4;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using Tracing;

namespace BookyWooks.Identity.Controllers;

[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
[Route("api/[controller]")]
// [Route("api/[controller]/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuthController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint, JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
        _jwtTokenService = jwtTokenService;
    }
    [HttpPost]
    public IActionResult Login([FromBody] LoginModel user)
    {
        var loginResult = _jwtTokenService.GenerateAuthToken(user);

        return loginResult is null ? Unauthorized() : Ok(loginResult);
    }

    [HttpPost, Route("~/Api/Auth/SignUp")]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp(SignupDTO signupDto)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity();
        activity?.AddEvent(new("User creation started."));

        var user = new ApplicationUser
        {
            UserName = signupDto.Email,
            Email = signupDto.Email,
        };

        var result = await _userManager.CreateAsync(user, signupDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description).ToList());
        }

        await _publishEndpoint.Publish<UserCreatedEvent>(new UserCreatedEvent(user.Id, user.Email, user.UserName));

        OpenTelemetryMetricConfiguration.UserCreatedEventCounter.Add(1, new KeyValuePair<string, object>("event.name", "UserCreatedEvent"));

        activity?.AddEvent(new("User created successfully."));

        return Ok(new Result<string>("User created successfully")); ;
    }

    [HttpGet, Route("~/Api/Auth/GetUser")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser()
    {
        var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null) return BadRequest();

        var user = await _userManager.FindByIdAsync(userIdClaim.Value);

        if (user == null) return BadRequest();

        return Ok(new { Id = user.Id, UserName = user.UserName, Email = user.Email });
    }

    [Authorize(Roles = "admin")]
    [HttpGet, Route("~/Api/Auth/OnlyAdmin")]
    public async Task<IActionResult> OnlyAdmin()
    {
        await LogTokenAndClaims();

        return Ok();
    }

    private async Task LogTokenAndClaims()
    {
        var identityToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);

        Debug.WriteLine($"Identity token: {identityToken}");

        foreach (var claim in User.Claims)
        {
            Debug.WriteLine($"Claim type: {claim.Type} - Claim value: {claim.Value}");
        }
    }
}
