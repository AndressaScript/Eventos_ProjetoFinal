using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Eventos_ProjetoFinal.Models
{
    public class Avaliacao
    {
        [Key]

        public int AvaliacaoID { get; set; }

        public string? Comentario { get; set; }

        [ForeignKey("AlunoID")]
        public int AlunoID { get; set; }

        [ForeignKey("EventoID")]
        public int EventoID { get; set; }

    }
}