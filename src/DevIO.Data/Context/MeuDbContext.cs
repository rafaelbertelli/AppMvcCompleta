using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevIO.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace DevIO.Data.Context
{
    public class MeuDbContext : DbContext
    {
        // Para criar uma migration: Add-Migration <nome> -Verbose -Context MeuDbContext
        // Para criar um sql script: script-migration -Context MeuDbContext
        // Para atualizar o schema do banco: Update-Database -Context MeuDbContext

        public MeuDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<Produto> Produtos { get; set; }

        public DbSet<Endereco> Enderecos { get; set; }

        public DbSet<Fornecedor> Fornecedores { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeuDbContext).Assembly);

            // impede o delete cascade
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(modelBuilder);
        }
    }
}
