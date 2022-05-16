namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alertas3 : DbMigration
    {
        public override void Up()
        {
        //    DropForeignKey("dbo.Alerts", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
        //    DropIndex("dbo.Alerts", new[] { "Agent_Employee_idEmployee" });
        //    DropTable("dbo.Alerts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Alerts",
                c => new
                {
                    IdAlerts = c.Guid(nullable: false),
                    tipo = c.Int(nullable: false),
                    date = c.DateTime(nullable: false),
                    status = c.Boolean(nullable: false),
                    Agent_Employee_idEmployee = c.Guid(),
                })
                .PrimaryKey(t => t.IdAlerts);

            CreateIndex("dbo.Alerts", "Agent_Employee_idEmployee");
            AddForeignKey("dbo.Alerts", "Agent_Employee_idEmployee", "dbo.Agent_Employee", "idEmployee");
        }
    }
}
