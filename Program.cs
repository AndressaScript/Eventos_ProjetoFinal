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

builder.Services.AddScoped<IModeracaoService, ModeracaoService>();


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
// SEED E LIMPEZA DE EVENTOS DE TESTE (Para a Galeria de Fotos)
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

        // 1. Limpeza automática de dados antigos do Seed (para sincronizar o banco de dados)
        var eventoMulheresVelho = context.Eventos.FirstOrDefault(e => e.NomeEvento == "Dia das Mulheres" && e.CargaHorariaEvento == "2 horas");
        if (eventoMulheresVelho != null)
        {
            context.Eventos.Remove(eventoMulheresVelho);
        }

        var eventoJhonHallVelho = context.Eventos.FirstOrDefault(e => e.NomeEvento == "Jhon Hall");
        if (eventoJhonHallVelho != null)
        {
            context.Eventos.Remove(eventoJhonHallVelho);
        }

        // 2. Seed do Outubro Rosa: Mantido como único evento passado e configurado com capa fixa e status INATIVO
        var eventoOutubro = context.Eventos.FirstOrDefault(e => e.NomeEvento == "Outubro Rosa");
        if (eventoOutubro == null)
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
                StatusEvento = "Inativo", // Força o status como Inativo (vermelho)
                NomePalestrante = "Vários",
                AdminID = adminId,
                ImagemUrl = "/img/mulheres.png"
            });
        }
        else
        {
            // Força a inativação e a imagem fixa se ele já existir no banco do usuário
            eventoOutubro.StatusEvento = "Inativo";
            eventoOutubro.ImagemUrl = "/img/mulheres.png" ;
        }

        context.SaveChanges();
        Console.WriteLine(">> Seed do banco de dados atualizado: Apenas 'Outubro Rosa' mantido como Inativo e com Capa Fixa!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(">> Erro ao executar o Seed do banco de dados: " + ex.Message);
    }
}

app.Run();