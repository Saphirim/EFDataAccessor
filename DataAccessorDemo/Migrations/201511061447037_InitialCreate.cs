using System.Diagnostics.CodeAnalysis;

namespace DataAccessorDemo.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MEntities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NEntities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OtherEntities",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(),
                        NEntityId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NEntities", t => t.NEntityId, cascadeDelete: true)
                .Index(t => t.NEntityId);
            
            CreateTable(
                "dbo.NEntityMEntities",
                c => new
                    {
                        NEntity_Id = c.Guid(nullable: false),
                        MEntity_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.NEntity_Id, t.MEntity_Id })
                .ForeignKey("dbo.NEntities", t => t.NEntity_Id, cascadeDelete: true)
                .ForeignKey("dbo.MEntities", t => t.MEntity_Id, cascadeDelete: true)
                .Index(t => t.NEntity_Id)
                .Index(t => t.MEntity_Id);
            
        }
        
        [ExcludeFromCodeCoverage]
        //Sollte die Testabdeckung nicht negativ beinträchtigen, hat nichts mit der zu testenden Logik zu tun!
        public override void Down()
        {
            DropForeignKey("dbo.OtherEntities", "NEntityId", "dbo.NEntities");
            DropForeignKey("dbo.NEntityMEntities", "MEntity_Id", "dbo.MEntities");
            DropForeignKey("dbo.NEntityMEntities", "NEntity_Id", "dbo.NEntities");
            DropIndex("dbo.NEntityMEntities", new[] { "MEntity_Id" });
            DropIndex("dbo.NEntityMEntities", new[] { "NEntity_Id" });
            DropIndex("dbo.OtherEntities", new[] { "NEntityId" });
            DropTable("dbo.NEntityMEntities");
            DropTable("dbo.OtherEntities");
            DropTable("dbo.NEntities");
            DropTable("dbo.MEntities");
        }
    }
}
