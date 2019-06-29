namespace ThiagoStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Final : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "pesoTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Orders", "precoFrete", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "precoFrete", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Orders", "pesoTotal");
        }
    }
}
