using System.ComponentModel.DataAnnotations;

namespace Library.Models;

public class BookCopy
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Numer inwentarzowy jest wymagany")]
    [StringLength(50, ErrorMessage = "Numer inwentarzowy może mieć maksymalnie 50 znaków")]
    public string InventoryNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [StringLength(100, ErrorMessage = "Stan może mieć maksymalnie 100 znaków")]
    public string? Condition { get; set; }

    // Klucz obcy do książki (relacja wiele do jednego)
    public int BookId { get; set; }

    // Navigation property - książka
    public Book Book { get; set; } = null!;
}

