namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlertasN : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AlertAsociados", "IdAlerts", "dbo.Alerts");
            DropIndex("dbo.AlertAsociados", new[] { "IdAlerts" });
            CreateTable(
                "dbo.Alertas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Alerta = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AlertAsociados", "Alertas_Id", c => c.Guid());
            CreateIndex("dbo.AlertAsociados", "Alertas_Id");
            AddForeignKey("dbo.AlertAsociados", "Alertas_Id", "dbo.Alertas", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AlertAsociados", "Alertas_Id", "dbo.Alertas");
            DropIndex("dbo.AlertAsociados", new[] { "Alertas_Id" });
            DropColumn("dbo.AlertAsociados", "Alertas_Id");
            DropTable("dbo.Alertas");
            CreateIndex("dbo.AlertAsociados", "IdAlerts");
            AddForeignKey("dbo.AlertAsociados", "IdAlerts", "dbo.Alerts", "IdAlerts", cascadeDelete: true);
        }
    }
}
