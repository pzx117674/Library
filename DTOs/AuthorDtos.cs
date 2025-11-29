using System.ComponentModel.DataAnnotations;

namespace Library.DTOs;

// DTO do zwracania informacji o autorze
public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int BookCount { get; set; }
}

// DTO do zwracania pełnych informacji o autorze wraz z książkami
public class AuthorWithBooksDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<BookDto> Books { get; set; } = new();
}

// DTO do tworzenia autora
public class CreateAuthorDto
{
    [Required(ErrorMessage = "Imię autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Imię musi mieć od 1 do 100 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Nazwisko musi mieć od 1 do 100 znaków")]
    public string LastName { get; set; } = string.Empty;
}

// DTO do aktualizacji autora
public class UpdateAuthorDto
{
    [Required(ErrorMessage = "Imię autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Imię musi mieć od 1 do 100 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko autora jest wymagane")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Nazwisko musi mieć od 1 do 100 znaków")]
    public string LastName { get; set; } = string.Empty;
}

