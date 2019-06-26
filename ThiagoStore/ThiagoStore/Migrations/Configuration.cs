namespace ThiagoStore.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ThiagoStore.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ThiagoStore.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ThiagoStore.Models.ApplicationDbContext context)
        {
            context.Products.AddOrUpdate(
                 p => p.Id,
                 new Product { Id = 1, nome = "produto 1", codigo = "COD1", descricao = "descrição produto 1", preco = 10, cor = "azul", modelo = "A", peso = 1, altura = 1, largura = 1, comprimento = 1, diametro = 1, imagem = "http" },
                 new Product { Id = 2, nome = "produto 2", codigo = "COD2", descricao = "descrição produto 2", preco = 20, cor = "verde", modelo = "B", peso = 2, altura = 2, largura = 2, comprimento = 2, diametro = 2, imagem = "http" },
                 new Product { Id = 3, nome = "produto 3", codigo = "COD3", descricao = "descrição produto 3", preco = 30, cor = "roxo", modelo = "C", peso = 2, altura = 1, largura = 2, comprimento = 1, diametro = 2, imagem = "http" }
            );

        }
    }
}
