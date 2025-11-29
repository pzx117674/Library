using System.ComponentModel.DataAnnotations;

namespace Library.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tytuł książki jest wymagany")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Tytuł musi mieć od 1 do 200 znaków")]
    public string Title { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "Rok wydania musi być w zakresie od 1450 do 2100")]
    public int PublicationYear { get; set; }

    [StringLength(20, ErrorMessage = "ISBN może mieć maksymalnie 20 znaków")]
    public string? ISBN { get; set; }

    // Klucz obcy do autora (relacja wiele do jednego)
    public int AuthorId { get; set; }

    // Navigation property - autor książki
    public Author Author { get; set; } = null!;

    // Navigation property - kolekcja egzemplarzy książki (jeden do wielu)
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}

