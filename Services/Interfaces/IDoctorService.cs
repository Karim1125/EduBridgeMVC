using EduBridgeMVC.Abstractions;
using EduBridgeMVC.Contracts.Doctor;
using EduBridgeMVC.Contracts.Team;

namespace EduBridgeMVC.Services.Interfaces;

public interface IDoctorService
{
    // Queries
    Task<Result<IEnumerable<DoctorResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DoctorResponse>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<DoctorResponse>>> GetAvailableDoctorsAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<TeamResponse>>> GetSupervisedTeamsAsync(CancellationToken cancellationToken = default);

    // Commands
    Task<Result> CreateAsync(string currentUserId, CreateDoctorRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasProfileAsync(string userId);
}
