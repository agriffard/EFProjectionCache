namespace SampleApp.Dtos;

/// <summary>
/// DTO for user data with company name.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}
