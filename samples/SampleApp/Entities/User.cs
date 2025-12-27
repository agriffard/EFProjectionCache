namespace SampleApp.Entities;

/// <summary>
/// Represents a user entity.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
