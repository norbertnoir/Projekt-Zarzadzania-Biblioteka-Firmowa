using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.Middleware;
using SystemZarzadzaniaBibliotekaFirmy.Models;
using SystemZarzadzaniaBibliotekaFirmy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var response = new
            {
                error = "Walidacja nie powiodła się",
                errors = errors,
                statusCode = 400
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library Management API",
        Version = "v1",
        Description = "API do zarządzania firmową biblioteką"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey nie jest skonfigurowany");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seed users
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Tworzenie bazy danych...");
        context.Database.EnsureCreated();
        
        logger.LogInformation("Inicjalizacja użytkowników...");
        await SeedUsersAsync(context, logger);
        
        logger.LogInformation("Inicjalizacja książek...");
        await SeedBooksAsync(context, logger);
        
        logger.LogInformation("Inicjalizacja zakończona pomyślnie.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Błąd podczas inicjalizacji bazy danych");
    // Nie przerywamy uruchomienia - aplikacja może działać bez seed data
}

app.Run();

static async Task SeedUsersAsync(LibraryDbContext context, ILogger logger)
{
    try
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Użytkownicy już istnieją, pomijam seed data.");
            return;
        }

        // Pobierz istniejących pracowników z seed data
        var employees = await context.Employees.ToListAsync();
        
        if (employees.Count < 4)
        {
            logger.LogWarning("Ostrzeżenie: Oczekiwano 4 pracowników, znaleziono {Count}. Użytkownicy mogą nie zostać utworzeni poprawnie.", employees.Count);
        }

        // Znajdź pracowników po emailu (bardziej niezawodne niż Id)
        var janKowalski = employees.FirstOrDefault(e => e.Email == "jan.kowalski@firma.pl");
        var annaNowak = employees.FirstOrDefault(e => e.Email == "anna.nowak@firma.pl");
        var piotrWisniewski = employees.FirstOrDefault(e => e.Email == "piotr.wisniewski@firma.pl");
        var mariaDabrowska = employees.FirstOrDefault(e => e.Email == "maria.dabrowska@firma.pl");

        // Utwórz użytkowników - EmployeeId jest opcjonalne
        var users = new List<User>();

        users.Add(new User 
        { 
            Username = "admin", 
            Email = "admin@firma.pl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmployeeId = mariaDabrowska?.Id
        });

        users.Add(new User 
        { 
            Username = "bibliotekarz", 
            Email = "piotr.wisniewski@firma.pl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Librarian123!"),
            Role = "Librarian",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmployeeId = piotrWisniewski?.Id
        });

        users.Add(new User 
        { 
            Username = "jan.kowalski", 
            Email = "jan.kowalski@firma.pl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"),
            Role = "Employee",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmployeeId = janKowalski?.Id
        });

        users.Add(new User 
        { 
            Username = "anna.nowak", 
            Email = "anna.nowak@firma.pl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!"),
            Role = "Employee",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmployeeId = annaNowak?.Id
        });

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        logger.LogInformation("Utworzono {Count} użytkowników.", users.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Błąd podczas seedowania użytkowników");
        throw;
    }
}

static async Task SeedBooksAsync(LibraryDbContext context, ILogger logger)
{
    try
    {
        // Sprawdź czy są książki z Lorem Ipsum (stare dane) - jeśli tak, usuń je
        var existingBooks = await context.Books.ToListAsync();
        if (existingBooks.Any())
        {
            // Sprawdź czy są książki z polskimi tytułami technicznymi
            var hasPolishTitles = existingBooks.Any(b => 
                b.Title.Contains("Czysty Kod") || 
                b.Title.Contains("Wzorce Projektowe") || 
                b.Title.Contains("Programowanie") ||
                b.Title.Contains("Algorytmy"));
            
            if (hasPolishTitles)
            {
                logger.LogInformation("Książki z polskimi tytułami już istnieją, pomijam seed data.");
                return;
            }
            
            // Jeśli są stare książki z Lorem Ipsum, usuń je
            logger.LogInformation("Usuwanie starych książek z Lorem Ipsum...");
            
            // Najpierw usuń relacje BookAuthor
            var bookIds = existingBooks.Select(b => b.Id).ToList();
            var oldBookAuthors = await context.BookAuthors
                .Where(ba => bookIds.Contains(ba.BookId))
                .ToListAsync();
            if (oldBookAuthors.Any())
            {
                context.BookAuthors.RemoveRange(oldBookAuthors);
                await context.SaveChangesAsync();
            }
            
            // Teraz usuń książki
            context.Books.RemoveRange(existingBooks);
            await context.SaveChangesAsync();
            logger.LogInformation("Usunięto {Count} starych książek i ich relacje.", existingBooks.Count);
        }

        logger.LogInformation("Generowanie 500 książek z polskimi tytułami technicznymi...");

        // Pobierz istniejące kategorie i autorów
        var categories = await context.Categories.ToListAsync();
        var authors = await context.Authors.ToListAsync();

        if (categories.Count == 0 || authors.Count == 0)
        {
            logger.LogWarning("Brak kategorii lub autorów. Książki nie mogą zostać utworzone.");
            return;
        }

        // Polskie tytuły techniczne
        var polishTitles = new[]
        {
            "Czysty Kod",
            "Wzorce Projektowe",
            "Programowanie w C#",
            "Algorytmy i struktury danych",
            "Mistrz Czystego Kodu",
            "Zwinne wytwarzanie oprogramowania",
            "Architektura Systemów",
            "Bezpieczeństwo aplikacji webowych",
            "Mikroserwisy w praktyce",
            "Wstęp do sztucznej inteligencji",
            "Bazy danych i SQL",
            "Programowanie obiektowe",
            "Testowanie oprogramowania",
            "DevOps i CI/CD",
            "Cloud Computing",
            "Docker i Kubernetes",
            "RESTful API Design",
            "GraphQL w praktyce",
            "React - zaawansowane techniki",
            "Node.js i Express",
            "Python dla programistów",
            "Java - kompletny przewodnik",
            "JavaScript ES6+",
            "TypeScript od podstaw",
            "Git i kontrola wersji",
            "Systemy rozproszone",
            "Kolejkowanie i komunikacja",
            "Monitoring i logowanie",
            "Performance tuning",
            "Code Review best practices",
            "Refaktoryzacja kodu",
            "SOLID principles",
            "Design Patterns w praktyce",
            "TDD - Test Driven Development",
            "BDD - Behavior Driven Development",
            "Continuous Integration",
            "Continuous Deployment",
            "Infrastructure as Code",
            "Kubernetes orchestration",
            "Microservices architecture",
            "Event-driven architecture",
            "Domain-Driven Design",
            "Clean Architecture",
            "Hexagonal Architecture",
            "Serverless computing",
            "Machine Learning basics",
            "Deep Learning fundamentals",
            "Data Science w praktyce",
            "Big Data processing",
            "Cybersecurity essentials",
            "Web Security best practices"
        };

        // Utwórz generator Faker z językiem polskim
        var random = new Random();
        var bookFaker = new Bogus.Faker<Book>("pl")
            .RuleFor(b => b.ISBN, f => f.Random.Replace("##########"))
            .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
            .RuleFor(b => b.Year, f => f.Random.Int(2000, 2024))
            .RuleFor(b => b.Pages, f => f.Random.Int(100, 800))
            .RuleFor(b => b.Description, f => f.Lorem.Paragraph(2))
            .RuleFor(b => b.IsAvailable, f => f.Random.Bool(0.5f)) // 50% dostępnych
            .RuleFor(b => b.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(b => b.CreatedAt, f => f.Date.Past(5))
            .RuleFor(b => b.Title, (f, b) => 
            {
                var baseTitle = f.PickRandom(polishTitles);
                // Dodaj losowy numer wydania lub rok dla unikalności
                // Jeśli wybieramy rok, użyj tego samego co rok wydania
                var editionType = f.Random.Bool(0.5f);
                if (editionType)
                {
                    var edition = f.Random.Int(1, 10);
                    return $"{baseTitle} (Edycja {edition})";
                }
                else
                {
                    // Użyj roku wydania książki w tytule
                    return $"{baseTitle} ({b.Year})";
                }
            });

        var books = bookFaker.Generate(500);

        // Generuj unikalne ISBN dla każdej książki
        var usedISBNs = new HashSet<string>();
        
        foreach (var book in books)
        {
            string isbn;
            do
            {
                isbn = random.Next(1000000000, int.MaxValue).ToString();
            } while (usedISBNs.Contains(isbn));
            
            usedISBNs.Add(isbn);
            book.ISBN = isbn;
        }

        // Zapisz książki w partiach dla lepszej wydajności
        const int batchSize = 100;
        for (int i = 0; i < books.Count; i += batchSize)
        {
            var batch = books.Skip(i).Take(batchSize).ToList();
            context.Books.AddRange(batch);
            await context.SaveChangesAsync();
            logger.LogInformation("Zapisano partię {BatchNumber} ({Count} książek)...", (i / batchSize) + 1, batch.Count);
        }

        // Pobierz zapisane książki z ich rzeczywistymi ID
        var savedBooks = await context.Books.OrderBy(b => b.CreatedAt).ToListAsync();
        
        // Przypisz autorów do książek (każda książka ma 1-3 autorów)
        var bookAuthors = new List<BookAuthor>();
        
        foreach (var book in savedBooks)
        {
            var numberOfAuthors = random.Next(1, 4);
            var selectedAuthors = authors.OrderBy(x => random.Next()).Take(numberOfAuthors).ToList();

            foreach (var author in selectedAuthors)
            {
                bookAuthors.Add(new BookAuthor
                {
                    BookId = book.Id,
                    AuthorId = author.Id
                });
            }
        }

        // Zapisz relacje BookAuthor w partiach
        for (int i = 0; i < bookAuthors.Count; i += batchSize)
        {
            var batch = bookAuthors.Skip(i).Take(batchSize).ToList();
            context.BookAuthors.AddRange(batch);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Utworzono {Count} książek z relacjami do autorów.", books.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Błąd podczas seedowania książek");
        throw;
    }
}

app.Run();
