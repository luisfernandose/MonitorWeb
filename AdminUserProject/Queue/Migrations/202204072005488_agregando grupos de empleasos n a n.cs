namespace Queue.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class agregandogruposdeempleasosnan : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Agent_EmployeeGroupsEmployee",
                c => new
                    {
                        idAgent_EmployeeGroupsEmployee = c.Guid(nullable: false),
                        Agent_Employee_idEmployee = c.Guid(),
                        Agent_EmployeesGroups_idemployeesGroup = c.Guid(),
                    })
                .PrimaryKey(t => t.idAgent_EmployeeGroupsEmployee)
                .ForeignKey("dbo.Agent_Employee", t => t.Agent_Employee_idEmployee)
                .ForeignKey("dbo.Agent_EmployeesGroups", t => t.Agent_EmployeesGroups_idemployeesGroup)
                .Index(t => t.Agent_Employee_idEmployee)
                .Index(t => t.Agent_EmployeesGroups_idemployeesGroup);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_EmployeesGroups_idemployeesGroup", "dbo.Agent_EmployeesGroups");
            DropForeignKey("dbo.Agent_EmployeeGroupsEmployee", "Agent_Employee_idEmployee", "dbo.Agent_Employee");
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_EmployeesGroups_idemployeesGroup" });
            DropIndex("dbo.Agent_EmployeeGroupsEmployee", new[] { "Agent_Employee_idEmployee" });
            DropTable("dbo.Agent_EmployeeGroupsEmployee");
        }
    }
}
