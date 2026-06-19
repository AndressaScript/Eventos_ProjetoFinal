using Eventos_ProjetoFinal.Contexts;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Eventos_ProjetoFinal.Repositories;
using Eventos_ProjetoFinal.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//config session 
builder.Services.AddSession(options =>
{
options.IdleTimeout = TimeSpan.FromMinutes(30);

options.Cookie.HttpOnly = true;
options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<BdDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IEventosRepository, EventosRepository>();
builder.Services.AddScoped<IEventosService, EventosService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSession();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// =================================================================
// SEED DE EVENTOS DE TESTE (Para a Galeria de Fotos)
// =================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BdDbContext>();
        
        // Garante que o banco existe e está criado
        context.Database.EnsureCreated();

        // Tenta encontrar um ID de Administrador válido para associar aos eventos
        int adminId = 1;
        var firstAdmin = context.Administrador.FirstOrDefault();
        if (firstAdmin != null)
        {
            adminId = firstAdmin.AdminID;
        }
        else
        {
            var firstUser = context.Usuario.FirstOrDefault();
            if (firstUser != null)
            {
                adminId = firstUser.UserID;
            }
        }

        // 1. Seed: Dia das Mulheres (Data no passado: 08/03/2026)
        if (!context.Eventos.Any(e => e.NomeEvento == "Dia das Mulheres"))
        {
            context.Eventos.Add(new Eventos
            {
                NomeEvento = "Dia das Mulheres",
                TipoEvento = "Palestra",
                LocalEvento = "Auditório Central",
                CapacidadeEvento = 150,
                DataEvento = new DateOnly(2026, 3, 8),
                HorarioEvento = new DateTime(2026, 3, 8, 14, 0, 0),
                CargaHorariaEvento = "2 horas",
                StatusEvento = "Ativo",
                NomePalestrante = "Jane Cruz",
                AdminID = adminId
            });
        }

        // 2. Seed: Jhon Hall (Data no passado: 10/04/2026)
        if (!context.Eventos.Any(e => e.NomeEvento == "Jhon Hall"))
        {
            context.Eventos.Add(new Eventos
            {
                NomeEvento = "Jhon Hall",
                TipoEvento = "Workshop",
                LocalEvento = "Laboratório de Redes",
                CapacidadeEvento = 60,
                DataEvento = new DateOnly(2026, 4, 10),
                HorarioEvento = new DateTime(2026, 4, 10, 16, 0, 0),
                CargaHorariaEvento = "4 horas",
                StatusEvento = "Ativo",
                NomePalestrante = "Alphonso Correa",
                AdminID = adminId
            });
        }

        // 3. Seed: Outubro Rosa (Data no passado: 23/10/2025)
        if (!context.Eventos.Any(e => e.NomeEvento == "Outubro Rosa"))
        {
            context.Eventos.Add(new Eventos
            {
                NomeEvento = "Outubro Rosa",
                TipoEvento = "Workshop",
                LocalEvento = "Auditório de Inovação",
                CapacidadeEvento = 100,
                DataEvento = new DateOnly(2025, 10, 23),
                HorarioEvento = new DateTime(2025, 10, 23, 19, 0, 0),
                CargaHorariaEvento = "3 horas",
                StatusEvento = "Ativo",
                NomePalestrante = "Vários",
                AdminID = adminId
            });
        }

        context.SaveChanges();
        Console.WriteLine(">> Seed executado com sucesso: 3 eventos passados adicionados!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(">> Erro ao executar o Seed do banco de dados: " + ex.Message);
    }
}

app.Run();
