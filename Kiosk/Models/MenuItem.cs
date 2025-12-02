using System.ComponentModel.DataAnnotations;

public class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa jest wymagana")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Opis jest wymagany")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Cena jest wymagana")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Adres obrazu jest wymagany")]
    public string ImageUrl { get; set; }

    [Required(ErrorMessage = "Kategoria jest wymagana")]
    public int? CategoryId { get; set; }
    public Category Category { get; set; }
}