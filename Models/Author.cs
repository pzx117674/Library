using System.ComponentModel.DataAnnotations;

namespace Library.Models;

public class Author
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Imię autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Imię musi mieć od 1 do 100 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Nazwisko musi mieć od 1 do 100 znaków")]
    public string LastName { get; set; } = string.Empty;

    // Navigation property - kolekcja książek autora (jeden do wielu)
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

