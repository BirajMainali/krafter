namespace Krafter.example;

public class Product
{
    public int Id { get; set; }
    
    [Krafter(true, true, true, true)]
    public string Name { get; set; }
    
    [Krafter(true, true, true, true)]
    public decimal Price { get; set; }
}