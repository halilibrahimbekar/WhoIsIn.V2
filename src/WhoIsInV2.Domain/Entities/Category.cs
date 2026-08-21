namespace WhoIsInV2.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = [];
}