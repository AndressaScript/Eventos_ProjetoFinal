using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventos_ProjetoFinal.Models
{
    [Table("Galeria")]
    public class Galeria
    {
        [Key]
        public int FotoID { get; set; }

        [Required]
        public int EventoID { get; set; }

        [ForeignKey("EventoID")]
        public virtual Eventos? Evento { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual Usuario? Usuario { get; set; }

        [Required]
        [StringLength(255)]
        public string CaminhoImagem { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Legenda { get; set; }

        [Required]
        public DateTime DataUpload { get; set; } = DateTime.Now;
    }
}