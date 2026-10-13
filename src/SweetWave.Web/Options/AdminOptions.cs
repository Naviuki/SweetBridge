using System.ComponentModel.DataAnnotations;

namespace SweetWave.Web.Options;

/// <summary>
/// Credentials of the administrator account used by the admin section.
/// </summary>
public sealed class AdminOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string SectionName = "Admin";

    /// <summary>
    /// Administrator login.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Administrator password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}