namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlertasCampo : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AlertAsociados", "IdAlerts");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AlertAsociados", "IdAlerts", c => c.Guid(nullable: false));
        }
    }
}
