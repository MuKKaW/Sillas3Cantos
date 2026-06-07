using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IProductoArchivoService, ProductoArchivoService>();
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<DummyJsonOptions>(builder.Configuration.GetSection(DummyJsonOptions.SectionName));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepository, MySqlUsuarioRepository>();
builder.Services.AddScoped<IMarcaRepository, MySqlMarcaRepository>();
builder.Services.AddScoped<ICategoriaRepository, MySqlCategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, MySqlProductoRepository>();
builder.Services.AddScoped<IProductoArchivoRepository, MySqlProductoArchivoRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IProductosExternosService, DummyJsonProductosExternosService>((serviceProvider, client) =>
{
    DummyJsonOptions options = serviceProvider.GetRequiredService<IOptions<DummyJsonOptions>>().Value;

    string baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
        ? "https://dummyjson.com/"
        : options.BaseUrl.Trim();

    if (!baseUrl.EndsWith('/'))
    {
        baseUrl += "/";
    }

    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? baseUri))
    {
        baseUri = new Uri("https://dummyjson.com/", UriKind.Absolute);
    }

    client.BaseAddress = baseUri;
    int timeoutSeconds = options.TimeoutSeconds <= 0 ? 5 : options.TimeoutSeconds;
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddEndpointsApiExplorer();

JwtOptions jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
byte[] jwtKey = Encoding.UTF8.GetBytes(jwtOptions.Key);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT. Ejemplo: Bearer eyJhbGciOi..."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
