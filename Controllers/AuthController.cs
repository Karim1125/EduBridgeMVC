using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using EduBridgeMVC.Contracts.Authentication;
using EduBridgeMVC.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Attributes;

namespace EduBridgeMVC.Controllers;

[Route("[controller]")]
public class AuthController(
    IAuthService authService,
    ITaService taService,
    IValidator<RegisterRequest> registerValidator,
    ILogger<AuthController> logger) : Controller
public class AuthController(
    IAuthService authService,
    ITaService taService,
    ILogger<AuthController> logger
) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ITaService _taService = taService;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpGet("login")]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
            return RedirectToAction("Index", "Home");

        var rememberedEmail = Request.Cookies["rememberedEmail"] ?? string.Empty;
        return View(new LoginRequest(rememberedEmail, string.Empty, !string.IsNullOrEmpty(rememberedEmail)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(request);
        }

        HttpContext.Session.SetString("token", result.Value.Token);
        HttpContext.Session.SetString("refreshToken", result.Value.RefreshToken);

        if (request.RememberMe)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            };

            Response.Cookies.Append("rememberedToken", result.Value.Token, cookieOptions);
            Response.Cookies.Append("rememberedRefreshToken", result.Value.RefreshToken, cookieOptions);
            Response.Cookies.Append("rememberedEmail", request.Email, cookieOptions);
        }
        else
        {
            Response.Cookies.Delete("rememberedToken");
            Response.Cookies.Delete("rememberedRefreshToken");
            Response.Cookies.Delete("rememberedEmail");
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Value.Token);

        var role = jwt.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value;

        if (!string.IsNullOrEmpty(role))
            HttpContext.Session.SetString("role", role);

        TempData["Success"] = "Login successful";

        if (role == "TA")
        {
            var userId = jwt.Claims.FirstOrDefault(c =>
             c.Type == ClaimTypes.NameIdentifier ||
             c.Type == "sub"
         )?.Value;
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
        Response.Cookies.Delete("rememberedToken");
        Response.Cookies.Delete("rememberedRefreshToken");
        Response.Cookies.Delete("rememberedEmail");
        return RedirectToAction("Login");
    }

    [HttpGet("register")]
    public IActionResult Register() => View();

    [HttpPost("register")]
    [DisableRateLimiting]
    public async Task<IActionResult> Register([AutoValidateNever] RegisterRequest request, CancellationToken cancellationToken)
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return View(GetRegisterErrorModel(request));
        }

        if (!ModelState.IsValid)
            return View(GetRegisterErrorModel(request));

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(GetRegisterErrorModel(request));
        }

        TempData["Success"] = "Check your email to confirm your account";
        return RedirectToAction("Login");
    }

    private RegisterRequest GetRegisterErrorModel(RegisterRequest request)
    {
        ModelState.Remove(nameof(RegisterRequest.Password));
        ModelState.Remove(nameof(RegisterRequest.ConfirmPassword));
        ModelState.Remove(nameof(RegisterRequest.ProfileImage));

        var persistedImage = request.ProfileImage is { Length: > 0 }
            ? GetProfileImageDataUrl(request.ProfileImage)
            : request.PersistedProfileImageDataUrl;

        return request with
        {
            Password = string.Empty,
            ConfirmPassword = string.Empty,
            ProfileImage = null,
            PersistedProfileImageDataUrl = persistedImage
        };
    }

    private static string? GetProfileImageDataUrl(IFormFile image)
    {
        if (image.Length == 0 || !image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return null;

        using var stream = image.OpenReadStream();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return $"data:{image.ContentType};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
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
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
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