using Bogus;
using Restaurante.Models;

namespace Restaurante.Data
{
    public static class ClientesDatosPrueba
    {
        // Solo para cargar la tabla la primera vez. Después se leen de SQL Server.
        public static List<Cliente> Generar()
        {
            var apellidos = new[]
            {
                "Vedia", "Tellez", "Flores", "maizares",
                "Vargas", "Rojas", "Pérez", "López",
                "Gutiérrez", "Fernández", "García", "Cruz",
                "Quiroga", "Mendoza", "Aguilar", "Suárez",
                "Torrez", "Arce", "Salazar", "Romero"
            };

            int secuencia = 1;

            var generador = new Faker<Cliente>("es")
                .UseSeed(12345)
                .RuleFor(c => c.Nombre, f => f.Name.FirstName())
                .RuleFor(c => c.Apellido, f =>
                    $"{f.PickRandom(apellidos)} {f.PickRandom(apellidos)}")
                .RuleFor(c => c.Telefono, f =>
                    f.Random.Replace("########")
                        .Remove(0, 1)
                        .Insert(0, f.PickRandom("6", "7")))
                .RuleFor(c => c.Correo, (f, c) =>
                    f.Internet.Email(
                        c.Nombre,
                        c.Apellido.Replace(" ", ""),
                        "gmail.com",
                        (secuencia++).ToString()))
                .RuleFor(c => c.Direccion, f => f.Address.StreetAddress());

            return generador.Generate(100);
        }
    }
}
