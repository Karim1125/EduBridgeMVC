using EduBridgeMVC.Abstractions;

namespace EduBridgeMVC.Errors;

public static class DoctorErrors
{
    public static readonly Error DoctorNotFound = new(
        "Doctor.NotFound", "Doctor not found", StatusCodes.Status404NotFound);

    public static readonly Error UserNotFound = new(
        "Doctor.UserNotFound", "User not found", StatusCodes.Status404NotFound);

    public static readonly Error UserAlreadyDoctor = new(
        "Doctor.AlreadyExists", "This user is already registered as a doctor", StatusCodes.Status409Conflict);

    public static readonly Error DoctorNotAvailable = new(
        "Doctor.NotAvailable", "This doctor has no available slots", StatusCodes.Status409Conflict);

    public static readonly Error UserNotDoctorRole = new(
        "Doctor.NotDoctorRole", "User does not have the Doctor role", StatusCodes.Status403Forbidden);
}