namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class agregandogruposdeempleasos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agent_EmployeesGroups",
                c => new
                    {
                        idemployeesGroup = c.Guid(nullable: false),
                        Nombre = c.String(nullable: false),
                        Agent_Empresa_IdCompany = c.Guid(),
                    })
                .PrimaryKey(t => t.idemployeesGroup)
                .ForeignKey("dbo.Agent_Empresa", t => t.Agent_Empresa_IdCompany)
                .Index(t => t.Agent_Empresa_IdCompany);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Agent_EmployeesGroups", "Agent_Empresa_IdCompany", "dbo.Agent_Empresa");
            DropIndex("dbo.Agent_EmployeesGroups", new[] { "Agent_Empresa_IdCompany" });
            DropTable("dbo.Agent_EmployeesGroups");
        }
    }
}
