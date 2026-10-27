using System.ComponentModel.DataAnnotations;

namespace Restaurante.Models
{
    public class Empleado
    {
        [Key]
        public int ID_Empleado { get; set; }

        [Required(ErrorMessage = "Ingresa el nombre.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa el apellido.")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa el cargo.")]
        [StringLength(80)]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa el teléfono.")]
        [RegularExpression(@"^[67]\d{7}$", ErrorMessage = "Ingresa 8 dígitos comenzando con 6 o 7.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
