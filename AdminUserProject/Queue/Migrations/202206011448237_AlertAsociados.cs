namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlertAsociados : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AlertAsociados",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IdAlerts = c.Guid(nullable: false),
                        Email = c.String(),
                        IdCompany = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Agent_Empresa", t => t.IdCompany, cascadeDelete: true)
                .ForeignKey("dbo.Alerts", t => t.IdAlerts, cascadeDelete: true)
                .Index(t => t.IdAlerts)
                .Index(t => t.IdCompany);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AlertAsociados", "IdAlerts", "dbo.Alerts");
            DropForeignKey("dbo.AlertAsociados", "IdCompany", "dbo.Agent_Empresa");
            DropIndex("dbo.AlertAsociados", new[] { "IdCompany" });
            DropIndex("dbo.AlertAsociados", new[] { "IdAlerts" });
            DropTable("dbo.AlertAsociados");
        }
    }
}
