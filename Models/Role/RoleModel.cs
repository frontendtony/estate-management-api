namespace EstateManager.Models;

public class RoleModel
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public List<Guid> Permissions { get; set; } = new();
}
