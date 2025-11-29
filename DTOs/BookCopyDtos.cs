using System.ComponentModel.DataAnnotations;

namespace Library.DTOs;

// DTO do zwracania informacji o egzemplarzu
public class BookCopyDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Condition { get; set; }
    public int BookId { get; set; }
    public BookDto? Book { get; set; }
}

// DTO do tworzenia egzemplarza
public class CreateBookCopyDto
{
    [Required(ErrorMessage = "Numer inwentarzowy jest wymagany")]
    [StringLength(50, ErrorMessage = "Numer inwentarzowy może mieć maksymalnie 50 znaków")]
    public string InventoryNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [StringLength(100, ErrorMessage = "Stan może mieć maksymalnie 100 znaków")]
    public string? Condition { get; set; }

    [Required(ErrorMessage = "ID książki jest wymagane")]
    public int BookId { get; set; }
}

// DTO do aktualizacji egzemplarza
public class UpdateBookCopyDto
{
    [Required(ErrorMessage = "Numer inwentarzowy jest wymagany")]
    [StringLength(50, ErrorMessage = "Numer inwentarzowy może mieć maksymalnie 50 znaków")]
    public string InventoryNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    [StringLength(100, ErrorMessage = "Stan może mieć maksymalnie 100 znaków")]
    public string? Condition { get; set; }

    [Required(ErrorMessage = "ID książki jest wymagane")]
    public int BookId { get; set; }
}

