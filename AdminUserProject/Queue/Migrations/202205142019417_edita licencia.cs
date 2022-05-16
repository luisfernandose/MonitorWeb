namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editalicencia : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.License", "startdate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.License", "startdate", c => c.DateTime(nullable: false));
        }
    }
}
