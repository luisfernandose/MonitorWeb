namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class licencia : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.License",
                c => new
                    {
                        IdLicense = c.Guid(nullable: false),
                        startdate = c.DateTime(nullable: false),
                        enddate = c.DateTime(nullable: false),
                        Agent_Empresa_IdCompany = c.Guid(),
                    })
                .PrimaryKey(t => t.IdLicense)
                .ForeignKey("dbo.Agent_Empresa", t => t.Agent_Empresa_IdCompany)
                .Index(t => t.Agent_Empresa_IdCompany);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.License", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropIndex("dbo.License", new[] { "Agent_Empresa_IdCompany" });
            DropTable("dbo.License");
        }
    }
}
