using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Eventos_ProjetoFinal.Models
{
    public class Eventos
    {
        [Key]

        public int EventoID { get; set; }

        [Required]
        public string NomeEvento { get; set; } = null!;

        [StringLength(100)]
        public string TipoEvento { get; set; } = null!;

        public DateOnly DataEvento { get; set; }

        [StringLength(100)]
        public string LocalEvento { get; set; } = null!;

        public DateTime HorarioEvento { get; set; }

        [StringLength(100)]
        public string CargaHorariaEvento { get; set; } = null!;

        [StringLength(100)]
        public string StatusEvento { get; set; } = null!;

        [StringLength(100)]
        public string NomePalestrante { get; set; } = null!;
        public int CapacidadeEvento { get; set; }

        [ForeignKey("AdminID")]
        public int AdminID { get; set; }
    }
}