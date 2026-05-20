using System.Security.Claims;
using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Abstractions.Consts;
using EduBridgeMVC.Contracts.Team;
using EduBridgeMVC.Errors;
using EduBridgeMVC.Models;
using EduBridgeMVC.Persistence;
using EduBridgeMVC.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduBridgeMVC.Services;

public class TeamService(
    ApplicationDbContext context,
    INotificationService notificationService,
    IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager,
    IMapper mapper) : ITeamService
{
    private string CurrentUserId => httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<Result<TeamResponse>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .Include(t => t.Leader)
            .Include(t => t.Ta).ThenInclude(ta => ta!.User)
            .Include(t => t.Doctor).ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return team is null
            ? Result.Failure<TeamResponse>(TeamErrors.TeamNotFound)
            : Result.Success(mapper.Map<TeamResponse>(team));
    }

    public async Task<Result<IEnumerable<TeamResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var teams = await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .Include(t => t.Leader)
            .Include(t => t.Ta).ThenInclude(ta => ta!.User)
            .Include(t => t.Doctor).ThenInclude(d => d!.User)
            .ToListAsync(cancellationToken);

        return Result.Success(mapper.Map<IEnumerable<TeamResponse>>(teams));
    }

    public async Task<Result<IEnumerable<TeamMemberResponse>>> GetMembersAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .AsNoTracking()
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (team is null)
            return Result.Failure<IEnumerable<TeamMemberResponse>>(TeamErrors.TeamNotFound);

        var members = team.Members
            .OrderByDescending(m => m.Role == MemberRole.Leader)
            .ThenBy(m => m.User.FirstName)
            .ThenBy(m => m.User.LastName);

        return Result.Success(mapper.Map<IEnumerable<TeamMemberResponse>>(members));
    }

    public async Task<Result<TeamResponse>> CreateAsync(
        CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            LeaderId = CurrentUserId,
            MaxMembers = TeamSettings.MaxMembers,
            Status = TeamStatus.Open
        };


        await context.Teams.AddAsync(team, cancellationToken);

        await context.TeamMembers.AddAsync(new TeamMember
        {
            TeamId = team.Id,
            UserId = CurrentUserId,
            Role = MemberRole.Leader,
            JoinedAt = DateTime.UtcNow
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(team.Id, cancellationToken);
    }

    public async Task<Result<TeamResponse>> UpdateAsync(
        Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null)
            return Result.Failure<TeamResponse>(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure<TeamResponse>(TeamErrors.NotTeamLeader);

        if (request.Name is not null) team.Name = request.Name;
        if (request.Description is not null) team.Description = request.Description;

        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.FindAsync<Team>([id], cancellationToken);

        if (team is null || team.IsDeleted)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        team.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddMemberAsync(
        Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        // verify user exists and has Student role
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var isStudent = await userManager.IsInRoleAsync(user, DefaultRoles.Student);

        if (!isStudent)
            return Result.Failure(TeamErrors.UserNotStudentRole);

        if (team.Members.Any(m => m.UserId == userId))
            return Result.Failure(TeamErrors.AlreadyMember);

        if (team.Members.Count >= team.MaxMembers)
            return Result.Failure(TeamErrors.TeamFull);

        await context.TeamMembers.AddAsync(new TeamMember
        {
            TeamId = id,
            UserId = userId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        }, cancellationToken);

        team.Status = team.Members.Count + 1 >= team.MaxMembers
            ? TeamStatus.Full
            : TeamStatus.Partial;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            userId,
            NotificationType.TeamMemberJoined,
            $"You have been added to team {team.Name}",
            team.Id,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(
        Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        var member = team.Members.FirstOrDefault(m => m.UserId == userId);

        if (member is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        context.TeamMembers.Remove(member);

        if (team.Members.Count - 1 < team.MaxMembers)
            team.Status = TeamStatus.Partial;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LeaveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId == CurrentUserId)
            return Result.Failure(TeamErrors.LeaderCannotLeave);

        var member = team.Members.FirstOrDefault(m => m.UserId == CurrentUserId);

        if (member is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        context.TeamMembers.Remove(member);

        if (team.Members.Count - 1 < team.MaxMembers)
            team.Status = TeamStatus.Partial;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangeStatusAsync(
        Guid id, ChangeTeamStatusRequest request, CancellationToken cancellationToken = default)
    {
        var team = await context.FindAsync<Team>([id], cancellationToken);

        if (team is null || team.IsDeleted)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        team.Status = request.Status;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignLeaderAsync(
        Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null || team.IsDeleted)
            return Result.Failure(TeamErrors.TeamNotFound);

        if (team.LeaderId != CurrentUserId)
            return Result.Failure(TeamErrors.NotTeamLeader);

        if (team.LeaderId == userId)
            return Result.Failure(TeamErrors.AlreadyLeader);

        var newLeaderMember = team.Members.FirstOrDefault(m => m.UserId == userId);

        if (newLeaderMember is null)
            return Result.Failure(TeamErrors.MemberNotFound);

        // demote current leader to member
        var currentLeaderMember = team.Members.FirstOrDefault(m => m.UserId == CurrentUserId);
        if (currentLeaderMember is not null)
            currentLeaderMember.Role = MemberRole.Member;

        // promote new leader
        newLeaderMember.Role = MemberRole.Leader;
        team.LeaderId = userId;

        await context.SaveChangesAsync(cancellationToken);

        await notificationService.SendAsync(
            userId,
            NotificationType.TeamMemberJoined,
            $"You have been assigned as the leader of team {team.Name}",
            team.Id,
            cancellationToken);

        return Result.Success();
    }
}
