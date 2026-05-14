namespace EduBridgeMVC.Contracts.Doctor;

public record SendDoctorRequestRequest(
    Guid TeamId,
    Guid DoctorId,
    string? Message
);