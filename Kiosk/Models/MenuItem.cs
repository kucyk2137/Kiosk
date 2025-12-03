public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }  // Burgers, Drinks, Pizza, etc.
    public string ImageUrl { get; set; }  // ścieżka do zdjęcia
}
