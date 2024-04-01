using Eros.Domain.Aggregates.Roles;

namespace Eros.Domain.Aggregates.Estates;

public class Estate
{
    public Estate(
        string name,
        string address,
        Guid createdBy,
        string? latlng = default
    )
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        CreatedBy = createdBy;
        LatLng = latlng;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Estate() {}

    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public Guid CreatedBy { get; init; }
    public string? LatLng { get; set; }
    public IEnumerable<Role> Roles { get; set; } = new List<Role>();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}