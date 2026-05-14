namespace EduBridgeMVC.Models;

public class DoctorRequest : BaseRequest
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid? RespondedByDoctorId { get; set; }
    public Doctor? RespondedByDoctor { get; set; }
}