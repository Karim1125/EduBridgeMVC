namespace EduBridgeMVC.Contracts.Doctor;

public record DoctorRequestResponse(
    Guid Id,
    Guid TeamId,
    Guid DoctorId,
    string TeamName,
    string DoctorName,
    string Status,
    string? Message,
    string? ResponseMessage
);