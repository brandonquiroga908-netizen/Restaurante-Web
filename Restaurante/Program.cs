
using Microsoft.EntityFrameworkCore;
using Restaurante.Data;

var builder = WebApplication.CreateBuilder(args);

// Registrar los controladores y las vistas
builder.Services.AddControllersWithViews();

// Configurar la conexión a la base de datos
builder.Services.AddDbContext<RestauranteDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configurar el pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (builder.Configuration.GetValue<bool>("Inicializacion:AplicarMigracionesYDatosPrueba"))
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestauranteDbContext>();
    db.Database.Migrate();

    if (!db.Clientes.Any())
    {
        db.Clientes.AddRange(ClientesDatosPrueba.Generar());
        db.SaveChanges();
    }
}

app.Run();
