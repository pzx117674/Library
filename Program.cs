using Microsoft.EntityFrameworkCore;
using Library.Data;
using Library.Models;
using Library.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja bazy danych SQLite
builder.Services.AddDbContext<LibraryDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=library.db"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Serwowanie plików statycznych (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// Automatyczne tworzenie bazy danych i migracji przy starcie + seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();

    // Seed data - dodaj przykładowe dane jeśli baza jest pusta
    if (!db.Authors.Any())
    {
        var authors = new[]
        {
            new Author { FirstName = "Adam", LastName = "Mickiewicz" },
            new Author { FirstName = "Henryk", LastName = "Sienkiewicz" },
            new Author { FirstName = "Stanisław", LastName = "Lem" },
            new Author { FirstName = "Andrzej", LastName = "Sapkowski" }
        };
        db.Authors.AddRange(authors);
        db.SaveChanges();

        var books = new[]
        {
            new Book { Title = "Pan Tadeusz", PublicationYear = 1834, ISBN = "978-83-08-07426-6", AuthorId = authors[0].Id },
            new Book { Title = "Dziady", PublicationYear = 1823, ISBN = "978-83-08-05123-6", AuthorId = authors[0].Id },
            new Book { Title = "Quo Vadis", PublicationYear = 1896, ISBN = "978-83-08-06789-3", AuthorId = authors[1].Id },
            new Book { Title = "Potop", PublicationYear = 1886, ISBN = "978-83-08-04567-2", AuthorId = authors[1].Id },
            new Book { Title = "Solaris", PublicationYear = 1961, ISBN = "978-83-08-01234-5", AuthorId = authors[2].Id },
            new Book { Title = "Cyberiada", PublicationYear = 1965, ISBN = "978-83-08-02345-6", AuthorId = authors[2].Id },
            new Book { Title = "Wiedźmin: Ostatnie życzenie", PublicationYear = 1993, ISBN = "978-83-7578-001-0", AuthorId = authors[3].Id },
            new Book { Title = "Wiedźmin: Miecz przeznaczenia", PublicationYear = 1992, ISBN = "978-83-7578-002-7", AuthorId = authors[3].Id }
        };
        db.Books.AddRange(books);
        db.SaveChanges();

        var copies = new[]
        {
            new BookCopy { InventoryNumber = "INV-001", IsAvailable = true, Condition = "Dobry", BookId = books[0].Id },
            new BookCopy { InventoryNumber = "INV-002", IsAvailable = false, Condition = "Bardzo dobry", BookId = books[0].Id },
            new BookCopy { InventoryNumber = "INV-003", IsAvailable = true, Condition = "Nowy", BookId = books[2].Id },
            new BookCopy { InventoryNumber = "INV-004", IsAvailable = true, Condition = "Dobry", BookId = books[4].Id },
            new BookCopy { InventoryNumber = "INV-005", IsAvailable = false, Condition = "Zużyty", BookId = books[4].Id },
            new BookCopy { InventoryNumber = "INV-006", IsAvailable = true, Condition = "Nowy", BookId = books[6].Id },
            new BookCopy { InventoryNumber = "INV-007", IsAvailable = true, Condition = "Bardzo dobry", BookId = books[6].Id },
            new BookCopy { InventoryNumber = "INV-008", IsAvailable = true, Condition = "Dobry", BookId = books[7].Id }
        };
        db.BookCopies.AddRange(copies);
        db.SaveChanges();

        Console.WriteLine("✅ Dodano przykładowe dane do bazy!");
    }
}

// ========== HELPER METHODS - MAPOWANIE DTO ==========

static AuthorDto MapToAuthorDto(Author author) => new()
{
    Id = author.Id,
    FirstName = author.FirstName,
    LastName = author.LastName,
    BookCount = author.Books.Count
};

static BookDto MapToBookDto(Book book) => new()
{
    Id = book.Id,
    Title = book.Title,
    PublicationYear = book.PublicationYear,
    ISBN = book.ISBN,
    AuthorId = book.AuthorId,
    Author = book.Author != null ? new AuthorDto
    {
        Id = book.Author.Id,
        FirstName = book.Author.FirstName,
        LastName = book.Author.LastName,
        BookCount = book.Author.Books.Count
    } : null,
    AvailableCopies = book.Copies.Count(c => c.IsAvailable)
};

static BookCopyDto MapToBookCopyDto(BookCopy copy) => new()
{
    Id = copy.Id,
    InventoryNumber = copy.InventoryNumber,
    IsAvailable = copy.IsAvailable,
    Condition = copy.Condition,
    BookId = copy.BookId,
    Book = copy.Book != null ? new BookDto
    {
        Id = copy.Book.Id,
        Title = copy.Book.Title,
        PublicationYear = copy.Book.PublicationYear,
        ISBN = copy.Book.ISBN,
        AuthorId = copy.Book.AuthorId,
        Author = copy.Book.Author != null ? new AuthorDto
        {
            Id = copy.Book.Author.Id,
            FirstName = copy.Book.Author.FirstName,
            LastName = copy.Book.Author.LastName,
            BookCount = 0 // Unikamy dodatkowego zapytania
        } : null,
        AvailableCopies = 0 // Unikamy dodatkowego zapytania
    } : null
};

// Helper do walidacji
static IResult ValidateModel<T>(T model)
{
    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    var context = new System.ComponentModel.DataAnnotations.ValidationContext(model!);
    
    if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model!, context, validationResults, true))
    {
        var errors = validationResults
            .Select(r => r.ErrorMessage)
            .ToList();
        return Results.BadRequest(new { Errors = errors });
    }
    
    return null!;
}

// ========== AUTHOR ENDPOINTS ==========

// GET /authors - pobierz wszystkich autorów
app.MapGet("/authors", async (LibraryDbContext db) =>
{
    var authors = await db.Authors
        .Include(a => a.Books)
        .ToListAsync();
    
    return Results.Ok(authors.Select(MapToAuthorDto).ToList());
});

// GET /authors/{id} - pobierz autora po ID
app.MapGet("/authors/{id}", async (int id, LibraryDbContext db) =>
{
    var author = await db.Authors
        .Include(a => a.Books)
        .FirstOrDefaultAsync(a => a.Id == id);
    
    if (author == null)
        return Results.NotFound(new { Error = $"Autor o ID {id} nie został znaleziony" });

    return Results.Ok(MapToAuthorDto(author));
});

// POST /authors - utwórz nowego autora
app.MapPost("/authors", async (CreateAuthorDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    var author = new Author
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName
    };

    db.Authors.Add(author);
    await db.SaveChangesAsync();

    return Results.Created($"/authors/{author.Id}", MapToAuthorDto(author));
});

// PUT /authors/{id} - aktualizuj autora
app.MapPut("/authors/{id}", async (int id, UpdateAuthorDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    var author = await db.Authors
        .Include(a => a.Books)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (author == null)
        return Results.NotFound(new { Error = $"Autor o ID {id} nie został znaleziony" });

    author.FirstName = dto.FirstName;
    author.LastName = dto.LastName;

    await db.SaveChangesAsync();

    return Results.Ok(MapToAuthorDto(author));
});

// DELETE /authors/{id} - usuń autora
app.MapDelete("/authors/{id}", async (int id, LibraryDbContext db) =>
{
    var author = await db.Authors.FindAsync(id);

    if (author == null)
        return Results.NotFound(new { Error = $"Autor o ID {id} nie został znaleziony" });

    db.Authors.Remove(author);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// ========== BOOK ENDPOINTS ==========

// GET /books - pobierz wszystkie książki (z opcjonalnym filtrowaniem po authorId)
app.MapGet("/books", async (int? authorId, LibraryDbContext db) =>
{
    var query = db.Books
        .Include(b => b.Author)
            .ThenInclude(a => a.Books)
        .Include(b => b.Copies)
        .AsQueryable();

    if (authorId.HasValue)
    {
        query = query.Where(b => b.AuthorId == authorId.Value);
    }

    var books = await query.ToListAsync();
    
    return Results.Ok(books.Select(MapToBookDto).ToList());
});

// GET /books/{id} - pobierz książkę po ID
app.MapGet("/books/{id}", async (int id, LibraryDbContext db) =>
{
    var book = await db.Books
        .Include(b => b.Author)
            .ThenInclude(a => a.Books)
        .Include(b => b.Copies)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (book == null)
        return Results.NotFound(new { Error = $"Książka o ID {id} nie została znaleziona" });

    return Results.Ok(MapToBookDto(book));
});

// POST /books - utwórz nową książkę
app.MapPost("/books", async (CreateBookDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    // Sprawdź czy autor istnieje
    var authorExists = await db.Authors.AnyAsync(a => a.Id == dto.AuthorId);
    if (!authorExists)
        return Results.BadRequest(new { Error = $"Autor o ID {dto.AuthorId} nie istnieje" });

    var book = new Book
    {
        Title = dto.Title,
        PublicationYear = dto.PublicationYear,
        ISBN = dto.ISBN,
        AuthorId = dto.AuthorId
    };

    db.Books.Add(book);
    await db.SaveChangesAsync();

    // Załaduj autora dla odpowiedzi
    await db.Entry(book).Reference(b => b.Author).LoadAsync();
    await db.Entry(book.Author).Collection(a => a.Books).LoadAsync();

    return Results.Created($"/books/{book.Id}", MapToBookDto(book));
});

// PUT /books/{id} - aktualizuj książkę
app.MapPut("/books/{id}", async (int id, UpdateBookDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    var book = await db.Books
        .Include(b => b.Author)
            .ThenInclude(a => a.Books)
        .Include(b => b.Copies)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (book == null)
        return Results.NotFound(new { Error = $"Książka o ID {id} nie została znaleziona" });

    // Sprawdź czy nowy autor istnieje
    if (dto.AuthorId != book.AuthorId)
    {
        var authorExists = await db.Authors.AnyAsync(a => a.Id == dto.AuthorId);
        if (!authorExists)
            return Results.BadRequest(new { Error = $"Autor o ID {dto.AuthorId} nie istnieje" });
    }

    book.Title = dto.Title;
    book.PublicationYear = dto.PublicationYear;
    book.ISBN = dto.ISBN;
    book.AuthorId = dto.AuthorId;

    await db.SaveChangesAsync();

    // Przeładuj autora jeśli się zmienił
    if (book.Author == null || book.Author.Id != dto.AuthorId)
    {
        await db.Entry(book).Reference(b => b.Author).LoadAsync();
        if (book.Author != null)
        {
            await db.Entry(book.Author).Collection(a => a.Books).LoadAsync();
        }
    }

    return Results.Ok(MapToBookDto(book));
});

// DELETE /books/{id} - usuń książkę
app.MapDelete("/books/{id}", async (int id, LibraryDbContext db) =>
{
    var book = await db.Books.FindAsync(id);

    if (book == null)
        return Results.NotFound(new { Error = $"Książka o ID {id} nie została znaleziona" });

    db.Books.Remove(book);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// ========== BOOK COPY ENDPOINTS ==========

// GET /bookcopies - pobierz wszystkie egzemplarze
app.MapGet("/bookcopies", async (LibraryDbContext db) =>
{
    var copies = await db.BookCopies
        .Include(bc => bc.Book)
            .ThenInclude(b => b.Author)
        .ToListAsync();
    
    return Results.Ok(copies.Select(MapToBookCopyDto).ToList());
});

// GET /bookcopies/{id} - pobierz egzemplarz po ID
app.MapGet("/bookcopies/{id}", async (int id, LibraryDbContext db) =>
{
    var copy = await db.BookCopies
        .Include(bc => bc.Book)
            .ThenInclude(b => b.Author)
        .FirstOrDefaultAsync(bc => bc.Id == id);

    if (copy == null)
        return Results.NotFound(new { Error = $"Egzemplarz o ID {id} nie został znaleziony" });

    return Results.Ok(MapToBookCopyDto(copy));
});

// POST /bookcopies - utwórz nowy egzemplarz
app.MapPost("/bookcopies", async (CreateBookCopyDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    // Sprawdź czy książka istnieje
    var bookExists = await db.Books.AnyAsync(b => b.Id == dto.BookId);
    if (!bookExists)
        return Results.BadRequest(new { Error = $"Książka o ID {dto.BookId} nie istnieje" });

    // Sprawdź czy numer inwentarzowy jest unikalny
    var inventoryExists = await db.BookCopies.AnyAsync(bc => bc.InventoryNumber == dto.InventoryNumber);
    if (inventoryExists)
        return Results.BadRequest(new { Error = $"Egzemplarz o numerze inwentarzowym '{dto.InventoryNumber}' już istnieje" });

    var copy = new BookCopy
    {
        InventoryNumber = dto.InventoryNumber,
        IsAvailable = dto.IsAvailable,
        Condition = dto.Condition,
        BookId = dto.BookId
    };

    db.BookCopies.Add(copy);
    await db.SaveChangesAsync();

    // Załaduj książkę i autora dla odpowiedzi
    await db.Entry(copy).Reference(bc => bc.Book).LoadAsync();
    await db.Entry(copy.Book).Reference(b => b.Author).LoadAsync();

    return Results.Created($"/bookcopies/{copy.Id}", MapToBookCopyDto(copy));
});

// PUT /bookcopies/{id} - aktualizuj egzemplarz
app.MapPut("/bookcopies/{id}", async (int id, UpdateBookCopyDto dto, LibraryDbContext db) =>
{
    var validationResult = ValidateModel(dto);
    if (validationResult != null)
        return validationResult;

    var copy = await db.BookCopies
        .Include(bc => bc.Book)
            .ThenInclude(b => b.Author)
        .FirstOrDefaultAsync(bc => bc.Id == id);

    if (copy == null)
        return Results.NotFound(new { Error = $"Egzemplarz o ID {id} nie został znaleziony" });

    // Sprawdź czy nowa książka istnieje
    if (dto.BookId != copy.BookId)
    {
        var bookExists = await db.Books.AnyAsync(b => b.Id == dto.BookId);
        if (!bookExists)
            return Results.BadRequest(new { Error = $"Książka o ID {dto.BookId} nie istnieje" });
    }

    // Sprawdź czy numer inwentarzowy jest unikalny (jeśli się zmienia)
    if (dto.InventoryNumber != copy.InventoryNumber)
    {
        var inventoryExists = await db.BookCopies.AnyAsync(bc => bc.InventoryNumber == dto.InventoryNumber);
        if (inventoryExists)
            return Results.BadRequest(new { Error = $"Egzemplarz o numerze inwentarzowym '{dto.InventoryNumber}' już istnieje" });
    }

    copy.InventoryNumber = dto.InventoryNumber;
    copy.IsAvailable = dto.IsAvailable;
    copy.Condition = dto.Condition;
    copy.BookId = dto.BookId;

    await db.SaveChangesAsync();

    // Przeładuj książkę jeśli się zmieniła
    if (copy.Book == null || copy.Book.Id != dto.BookId)
    {
        await db.Entry(copy).Reference(bc => bc.Book).LoadAsync();
        if (copy.Book != null)
        {
            await db.Entry(copy.Book).Reference(b => b.Author).LoadAsync();
        }
    }

    return Results.Ok(MapToBookCopyDto(copy));
});

// DELETE /bookcopies/{id} - usuń egzemplarz
app.MapDelete("/bookcopies/{id}", async (int id, LibraryDbContext db) =>
{
    var copy = await db.BookCopies.FindAsync(id);

    if (copy == null)
        return Results.NotFound(new { Error = $"Egzemplarz o ID {id} nie został znaleziony" });

    db.BookCopies.Remove(copy);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
