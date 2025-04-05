using Microsoft.EntityFrameworkCore;
using PublicPersonelDataApi.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Bogus;
using PublicPersonelDataApi.Model;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS ayarları, belirli bir kaynaktan gelen isteklere izin vermek için kullanılır.
// Bu örnekte, "AllowedOrigin" ayarına göre gelen isteklere izin verilmektedir.
// Bu ayar, uygulamanın hangi kaynaklardan gelen isteklere izin vereceğini belirler.
builder.Services.AddCors(options=>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigin"]??string.Empty)
           .WithMethods("GET","POST","DELETE")
           .AllowAnyHeader();
    });
});
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Swagger ayarları. Swagger'dan authorization butonu çıkması için bu kısım gerekli.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Bearer Token ayarı
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer abc123\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



builder.Services.AddDbContext<PersonelDbContext>(options =>
    options.UseInMemoryDatabase("PersonelDb"));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


var app = builder.Build();

//In memory dbye kayıt atma start
using (var scope = app.Services.CreateScope())
{
    var _context = scope.ServiceProvider.GetRequiredService<PersonelDbContext>();
    // Faker ile sahte veriler oluştur
    int idCounter = 1; // Benzersiz ID için sayaç
    var faker = new Faker<Personel>()
        .RuleFor(p => p.Id, f => idCounter++) // Benzersiz ID oluşturmak için sayaç kullan
        .RuleFor(p => p.Name, f => f.Name.FullName()) // Rastgele isim
        .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber()) // Rastgele telefon numarası
        .RuleFor(p => p.Title, f => f.Name.JobTitle()); // Rastgele iş unvanı

    // 200 sahte müşteri oluştur
    var fakePersonels = faker.Generate(200);

    // Veritabanına ekle
    await _context.Personels.AddRangeAsync(fakePersonels);
    await _context.SaveChangesAsync();
    Console.WriteLine($"Toplam Kayıt Sayısı: {_context.Personels.Count()}");
     foreach (var personel in _context.Personels.Take(5))
    {
        Console.WriteLine($"ID: {personel.Id}, Name: {personel.Name}, Phone: {personel.Phone}, Title: {personel.Title}");
    }
   
}
//In memory dbye kayıt atma end

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseAuthentication(); // Kimlik doğrulama middleware'i
app.UseAuthorization();

app.MapControllers();

app.Run();
