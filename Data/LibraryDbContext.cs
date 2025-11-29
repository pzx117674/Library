using Microsoft.EntityFrameworkCore;
using Library.Models;

namespace Library.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja relacji Author -> Book (jeden do wielu)
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Cascade); // Usunięcie autora usuwa jego książki

        // Konfiguracja relacji Book -> BookCopy (jeden do wielu)
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade); // Usunięcie książki usuwa jej egzemplarze

        // Indeksy
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN);

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.InventoryNumber)
            .IsUnique();
    }
}

