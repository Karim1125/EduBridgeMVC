using System.ComponentModel.DataAnnotations;

namespace EduBridgeMVC.Settings;

public class SecurityCodeSettings
{
    public static string SectionName = "SecurityCodeSettings";
    [Required] public string TaCode { get; set; } = string.Empty;
    [Required] public string DoctorCode { get; set; } = string.Empty;
}