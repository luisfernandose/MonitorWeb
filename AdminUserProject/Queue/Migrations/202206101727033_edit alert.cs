namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editalert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alertas", "type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Alertas", "type");
        }
    }
}
