using EduBridgeMVC.Abstractions.Consts;

namespace EduBridgeMVC.Contracts.Team;

public record TeamMemberResponse(
    string UserId,
    string FullName,
    string? ProfileImageUrl,
    MemberRole Role,
    DateTime JoinedAt
);