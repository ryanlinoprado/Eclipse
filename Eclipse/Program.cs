using Eclipse.Data;
using Eclipse.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
var builder = WebApplication.CreateBuilder(args);

// registra o serviço de MVC
builder.Services.AddControllersWithViews();

// registra o serviço de páginas HTML+CSS+JS+C#
builder.Services.AddRazorPages();

// registra  BD
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DbPath")));

// lê a chave dos User Secrets para acessar a API da Open AI
var apiKey = builder.Configuration["OpenAI:ApiKey"];

// registra um HttpClient nomeado para a OpenAI
builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
});

// registra o serviço de chat que usa esse HttpClient
builder.Services.AddScoped<ChatAiServiceHttp>();

// compilação da aplicação
var app = builder.Build();

// define como as solicitações HTTP vão ser gerenciadas
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// registra o uso de HTTPS
app.UseHttpsRedirection();

// registra o uso de arquivos estáticos
app.UseStaticFiles();

// registra o uso de rotas de navegação
app.UseRouting();

// registra o uso de elementos de autenticação (login)
app.UseAuthorization();

//define como as rotas de navegação vão ser executadas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// roda o web app
app.Run();