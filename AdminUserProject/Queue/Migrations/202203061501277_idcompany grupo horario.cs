namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class idcompanygrupohorario : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Agent_Horary", "IdCompany", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Agent_Horary", "IdCompany");
        }
    }
}
