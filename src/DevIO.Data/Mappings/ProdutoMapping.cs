using DevIO.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevIO.Data.Mappings
{
    public class ProdutoMapping : IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            // aqui eu posso configurar algo customizado para cada campo da tabela.
            // se eu não configurar nada, o EF vai criar o tipo automaticamente de acordo com o definido na model
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome).IsRequired().HasColumnType("varchar(200)");
            builder.Property(p => p.Descricao).IsRequired().HasColumnType("varchar(1000)");
            builder.Property(p => p.Imagem).IsRequired().HasColumnType("varchar(100)");

            builder.ToTable("Produtos");
        }
    }
}
