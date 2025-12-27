namespace SampleApp.Entities;

/// <summary>
/// Represents a company entity.
/// </summary>
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
