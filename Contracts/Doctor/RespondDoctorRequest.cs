namespace EduBridgeMVC.Contracts.Doctor;

public record RespondDoctorRequestDto(
    bool IsApproved,
    string? ResponseMessage
);