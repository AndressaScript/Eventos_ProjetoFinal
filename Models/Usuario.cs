using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Eventos_ProjetoFinal.Models
{
    public class Usuario
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Senha { get; set; } = null!;

    }
}