using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using EduBridgeMVC.Contracts.Authentication;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduBridgeMVC.Controllers;

[Route("[controller]")]
public class AuthController(
    IAuthService authService,
    ITaService taService,
    ILogger<AuthController> logger
) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ITaService _taService = taService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpGet("login")]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.GetTokenAsync(
            request.Email,
            request.Password,
            cancellationToken
        );

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        HttpContext.Session.SetString("token", result.Value.Token);
        HttpContext.Session.SetString("refreshToken", result.Value.RefreshToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Value.Token);

        var role = jwt.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value;

        if (!string.IsNullOrEmpty(role))
            HttpContext.Session.SetString("role", role);

        TempData["Success"] = "Login successful";

        if (role == "TA")
        {
            var userId = jwt
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                ?.Value;
            var tasResult = await _taService.GetAllTAsAsync();

            if (!tasResult.IsSuccess)
                return RedirectToAction("Login", "Auth");

            var hasProfile = tasResult.Value.Any(x => x.UserId.ToString() == userId);

            if (!hasProfile)
                return RedirectToAction("Create", "Ta");

            return RedirectToAction("Index", "Ta");
        }

        if (role == "Admin")
            return RedirectToAction("Index", "User");

        if (role == "Doctor")
            return RedirectToAction("Index", "Doctor");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet("register")]
    public IActionResult Register() => View();

    [HttpPost("register")]
    [DisableRateLimiting]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Check your email to confirm your account";
        return RedirectToAction("Login");
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code, string? role)
    {
        var request = new ConfirmEmailRequest(userId, code, role);

        var result = await _authService.ConfirmEmailAsync(request);

        if (!result.IsSuccess)
            return View("Error");

        TempData["Success"] = "Email confirmed successfully";

        return RedirectToAction("Login");
    }

    [HttpGet("forget-password")]
    public IActionResult ForgetPassword() => View();

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.SendResetPasswordCodeAsync(request.Email);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Reset link sent to your email";
        return RedirectToAction("Login");
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string email, string code)
    {
        return View(new ResetPasswordRequest(email, code, "", ""));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Password reset successfully";
        return RedirectToAction("Login");
    }

    [HttpGet("resend-confirmation")]
    public IActionResult ResendConfirmation() => View();

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationEmailRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.ResendConfirmationEmailAsync(request);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        TempData["Success"] = "Confirmation email sent successfully";
        return RedirectToAction("Login");
    }
}
