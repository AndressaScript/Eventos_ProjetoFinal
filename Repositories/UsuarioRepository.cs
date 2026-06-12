using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Contexts;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos_ProjetoFinal.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly BdDbContext _context;

        public UsuarioRepository(BdDbContext context)
        {
            _context = context;
        }
        public async Task <Usuario?> BuscarPorEmail(string email)
        {
            return await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<string?> ObterRoleUsuario(int userId)
        {
            var isAdmin = await _context.Administrador.AnyAsync(a => a.Admin == userId);
            if(isAdmin) return "Admin";

            var isAluno = await _context.Aluno.AnyAsync( a => a.AlunoID == userId);
            if(isAluno) return "Aluno";

            return null;
        }
    }
}