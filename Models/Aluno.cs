using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventos_ProjetoFinal.Models
{
    public class Aluno
    {
        [Key]
        public int AlunoID { get; set; }

        [Required]
        [StringLength(100)]
        public string Matricula { get; set; } = null!;

    }
}