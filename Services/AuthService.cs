using System.Security.Cryptography;
using System.Text;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Authentication;
using EduBridgeMVC.Contracts.Authentication;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Helpers;
using EduBridgeMVC.Models;
using EduBridgeMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender) : IAuthService
{
    private const int RefreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        var result = await signInManager.PasswordSignInAsync(user, password, false, true);

        if (result.Succeeded)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            var (token, expiresIn) = jwtProvider.GenerateToken(user, userRoles);

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);

            string refreshToken;
            DateTime refreshTokenExpiry;

            if (activeRefreshToken != null)
            {
                refreshToken = activeRefreshToken.Token;
                refreshTokenExpiry = activeRefreshToken.ExpiresOn;
            }
            else
            {
                refreshToken = GenerateRefreshToken();
                refreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiresOn = refreshTokenExpiry
                });

                await userManager.UpdateAsync(user);
            }

            var response = new AuthResponse(token, expiresIn, refreshToken, refreshTokenExpiry);

            return Result.Success(response);
        }

        var error = result.IsNotAllowed
            ? UserErrors.EmailNotConfirmed
            : result.IsLockedOut
                ? UserErrors.LockedUser
                : UserErrors.InvalidCredentials;

        return Result.Failure<AuthResponse>(error);
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(
        string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        if (user.IsDisabled)
            return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        if (user.LockoutEnd > DateTime.UtcNow)
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);


        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var userRoles = await userManager.GetRolesAsync(user);

        var (newToken, expiresIn) = jwtProvider.GenerateToken(user, userRoles);
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        await userManager.UpdateAsync(user);

        var response = new AuthResponse(newToken, expiresIn, newRefreshToken, refreshTokenExpiration);

        return Result.Success(response);
    }

    public async Task<Result> RevokeRefreshTokenAsync(
        string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        return Result.Success();
    }

   public async Task<Result> RegisterAsync(
    RegisterRequest request, CancellationToken cancellationToken = default)
{
    var existingUser = await userManager.Users
        .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

    if (existingUser is not null)
    {
        if (existingUser.IsDisabled)
            return Result.Failure(new Error("User.Disabled",
                "This account has been disabled. Please contact support.",
                StatusCodes.Status400BadRequest));

        return Result.Failure(UserErrors.DuplicatedEmail);
    }

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName
    };

    var result = await userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
       var roleToAssign = string.IsNullOrWhiteSpace(request.Role)
            ? DefaultRoles.Student
            : request.Role;

        await userManager.AddToRoleAsync(user, roleToAssign);

        // Upload profile image if provided
        if (request.ProfileImage != null && request.ProfileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine("wwwroot", "images", "profiles");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(request.ProfileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await request.ProfileImage.CopyToAsync(stream, cancellationToken);
            user.ProfileImageUrl = $"/images/profiles/{fileName}";
            await userManager.UpdateAsync(user);
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    var error = result.Errors.First();

    return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
}

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
{
    if (await userManager.FindByIdAsync(request.UserId) is not { } user)
        return Result.Failure(UserErrors.InvalidCode);

    if (user.EmailConfirmed)
        return Result.Failure(UserErrors.DuplicatedConfirmation);

    var code = request.Code;

    try
    {
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
    }
    catch (FormatException)
    {
        return Result.Failure(UserErrors.InvalidCode);
    }

    var result = await userManager.ConfirmEmailAsync(user, code);

    if (result.Succeeded)
        return Result.Success();

    var error = result.Errors.First();

    return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
}


    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        logger.LogInformation("Confirmation code: {code}", code);

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        if (await userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed with { StatusCode = StatusCodes.Status400BadRequest });

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        logger.LogInformation("Reset code: {code}", code);

        await SendResetPasswordEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult identityResult;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            identityResult = await userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            identityResult = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
        }

        if (identityResult.Succeeded)
            return Result.Success();

        var error = identityResult.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
{
    var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

    var actionUrl = $"{origin}/Auth/confirm-email?userId={user.Id}&code={code}";

    var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
        new Dictionary<string, string>
        {
            { "{{name}}", user.FirstName },
            { "{{action_url}}", actionUrl }
        }
    );

    await emailSender.SendEmailAsync(user.Email!, "✅EDU Bridge : Email Confirmation", emailBody);
}

    private async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/Auth/reset-password?email={user.Email}&code={code}" }
            }
        );

        await emailSender.SendEmailAsync(user.Email!, "✅EDU Bridge : Change Password", emailBody);
    }
}