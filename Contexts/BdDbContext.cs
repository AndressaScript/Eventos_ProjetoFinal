using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventos_ProjetoFinal.Interfaces;
using Eventos_ProjetoFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos_ProjetoFinal.Contexts
{
    public class BdDbContext : DbContext
    {
      public BdDbContext(DbContextOptions<BdDbContext> options) : base (options){} 

      public DbSet<Usuario> Usuario {get; set;} = null!;

      public DbSet<Aluno> Aluno {get;set;} = null!;

      public DbSet<Administrador> Administrador {get;set;} = null!;

      public DbSet<Avaliacao> Avaliacao {get;set;} = null!;

      public DbSet<Inscricao> Inscricao {get;set;} = null!;

      public DbSet<Eventos> Eventos {get;set;} = null!;

    //  public DbSet<Galeria> Galeria {get;set;} = null!;

    }

}