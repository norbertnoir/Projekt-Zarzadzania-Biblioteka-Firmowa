using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<BookAuthor> BookAuthors { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfiguracja BookAuthor jako tabela łącząca
        modelBuilder.Entity<BookAuthor>()
            .HasKey(ba => new { ba.BookId, ba.AuthorId });

        modelBuilder.Entity<BookAuthor>()
            .HasOne(ba => ba.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(ba => ba.BookId);

        modelBuilder.Entity<BookAuthor>()
            .HasOne(ba => ba.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(ba => ba.AuthorId);

        // Konfiguracja relacji Book - Category
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Konfiguracja relacji Loan - Book
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        // Konfiguracja relacji Loan - Employee
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Employee)
            .WithMany(e => e.Loans)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indeksy dla lepszej wydajności
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        // Konfiguracja relacji User - Employee
        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithOne(e => e.User)
            .HasForeignKey<User>(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Seed data - przykładowe dane
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Kategorie
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Programowanie", Description = "Książki o programowaniu" },
            new Category { Id = 2, Name = "Zarządzanie", Description = "Książki o zarządzaniu" },
            new Category { Id = 3, Name = "Biznes", Description = "Książki biznesowe" },
            new Category { Id = 4, Name = "Technologia", Description = "Książki o technologii" }
        );

        // Autorzy
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "Robert", LastName = "Martin", Biography = "Znany autor książek o programowaniu" },
            new Author { Id = 2, FirstName = "Eric", LastName = "Evans", Biography = "Ekspert w dziedzinie Domain-Driven Design" },
            new Author { Id = 3, FirstName = "Martin", LastName = "Fowler", Biography = "Architekt oprogramowania" }
        );

        // Pracownicy
        modelBuilder.Entity<Employee>().HasData(
            new Employee 
            { 
                Id = 1, 
                FirstName = "Jan", 
                LastName = "Kowalski", 
                Email = "jan.kowalski@firma.pl",
                Department = "IT",
                Position = "Programista",
                CreatedAt = DateTime.UtcNow
            },
            new Employee 
            { 
                Id = 2, 
                FirstName = "Anna", 
                LastName = "Nowak", 
                Email = "anna.nowak@firma.pl",
                Department = "HR",
                Position = "Specjalista HR",
                CreatedAt = DateTime.UtcNow
            },
            new Employee 
            { 
                Id = 3, 
                FirstName = "Piotr", 
                LastName = "Wiśniewski", 
                Email = "piotr.wisniewski@firma.pl",
                Department = "Biblioteka",
                Position = "Bibliotekarz",
                CreatedAt = DateTime.UtcNow
            },
            new Employee 
            { 
                Id = 4, 
                FirstName = "Maria", 
                LastName = "Dąbrowska", 
                Email = "maria.dabrowska@firma.pl",
                Department = "Zarządzanie",
                Position = "Dyrektor",
                CreatedAt = DateTime.UtcNow
            }
        );

    }
}

