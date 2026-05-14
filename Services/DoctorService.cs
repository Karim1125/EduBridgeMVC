using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class DoctorService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper) : IDoctorService
{
    private string CurrentUserId => httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<Result<IEnumerable<DoctorResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var doctors = await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<DoctorResponse>>(doctors));
    }

    public async Task<Result<DoctorResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doctor is null)
            return Result.Failure<DoctorResponse>(DoctorErrors.DoctorNotFound);

        return Result.Success(mapper.Map<DoctorResponse>(doctor));
    }

    public async Task<Result<IEnumerable<DoctorResponse>>> GetAvailableDoctorsAsync(
        CancellationToken cancellationToken = default)
    {
        var doctors = await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.AvailableTeams > 0)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<DoctorResponse>>(doctors));
    }

    public async Task<Result<IEnumerable<TeamResponse>>> GetSupervisedTeamsAsync(
        CancellationToken cancellationToken = default)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == CurrentUserId, cancellationToken);

        if (doctor is null)
            return Result.Failure<IEnumerable<TeamResponse>>(DoctorErrors.DoctorNotFound);

        var teams = await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .Include(t => t.Leader)
            .Where(t => t.DoctorId == doctor.Id)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<TeamResponse>>(teams));
    }

    public async Task<Result> CreateAsync(
        string currentUserId, CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(currentUserId);

        if (user is null)
            return Result.Failure(DoctorErrors.UserNotFound);

        var isDoctor = await userManager.IsInRoleAsync(user, DefaultRoles.Doctor);

        if (!isDoctor)
            return Result.Failure(DoctorErrors.UserNotDoctorRole);

        var alreadyHasProfile = await context.Doctors
            .AnyAsync(d => d.UserId == currentUserId, cancellationToken);

        if (alreadyHasProfile)
            return Result.Failure(DoctorErrors.UserAlreadyDoctor);

        var doctor = request.Adapt<Doctor>();
        doctor.UserId = currentUserId;
        doctor.AvailableTeams = request.MaxTeams;

        await context.Doctors.AddAsync(doctor, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(
        Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var doctor = await context.FindAsync<Doctor>([id], cancellationToken);

        if (doctor is null || doctor.IsDeleted)
            return Result.Failure(DoctorErrors.DoctorNotFound);

        request.Adapt(doctor);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var doctor = await context.FindAsync<Doctor>([id], cancellationToken);

        if (doctor is null || doctor.IsDeleted)
            return Result.Failure(DoctorErrors.DoctorNotFound);

        doctor.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<bool> HasProfileAsync(string userId)
    {
        return await context.Doctors
            .AnyAsync(d => d.UserId == userId && !d.IsDeleted);
    }
}