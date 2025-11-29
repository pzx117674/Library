using System.ComponentModel.DataAnnotations;

namespace Library.DTOs;

// DTO do zwracania informacji o książce (bez cyklu)
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string? ISBN { get; set; }
    public int AuthorId { get; set; }
    public AuthorDto? Author { get; set; }
    public int AvailableCopies { get; set; }
}

// DTO do zwracania pełnych informacji o książce wraz z egzemplarzami
public class BookWithCopiesDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string? ISBN { get; set; }
    public int AuthorId { get; set; }
    public AuthorDto? Author { get; set; }
    public List<BookCopyDto> Copies { get; set; } = new();
}

// DTO do tworzenia książki
public class CreateBookDto
{
    [Required(ErrorMessage = "Tytuł książki jest wymagany")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Tytuł musi mieć od 1 do 200 znaków")]
    public string Title { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "Rok wydania musi być w zakresie od 1450 do 2100")]
    public int PublicationYear { get; set; }

    [StringLength(20, ErrorMessage = "ISBN może mieć maksymalnie 20 znaków")]
    public string? ISBN { get; set; }

    [Required(ErrorMessage = "ID autora jest wymagane")]
    public int AuthorId { get; set; }
}

// DTO do aktualizacji książki
public class UpdateBookDto
{
    [Required(ErrorMessage = "Tytuł książki jest wymagany")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Tytuł musi mieć od 1 do 200 znaków")]
    public string Title { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "Rok wydania musi być w zakresie od 1450 do 2100")]
    public int PublicationYear { get; set; }

    [StringLength(20, ErrorMessage = "ISBN może mieć maksymalnie 20 znaków")]
    public string? ISBN { get; set; }

    [Required(ErrorMessage = "ID autora jest wymagane")]
    public int AuthorId { get; set; }
}

