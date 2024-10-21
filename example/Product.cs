using Krafter.Attributes;

namespace Krafter.example;

public class Product
{
    public int Id { get; set; }
    
    [Krafter()]
    public string Name { get; set; }
    
    [Krafter()]
    public decimal Price { get; set; }
}