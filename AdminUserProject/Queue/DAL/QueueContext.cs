using Queue.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace Queue.DAL
{
    using global::Queue.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;


    public class QueueContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }


        public QueueContext() : base("name=QueueContext")
        {
            Database.SetInitializer<QueueContext>(null);
        }


        public static QueueContext Create()
        {
            return new QueueContext();
        }

        public DbSet<RoleViewModel> RoleViewModels { get; set; }
        public DbSet<Agent_Employee> Agent_Employee { get; set; }
        public DbSet<Agent_Job> Agent_Job { get; set; }
        public DbSet<Agent_Empresa> Agent_Empresa { get; set; }
        public DbSet<Agent_GenericError> Agent_GenericError { get; set; }
        public DbSet<Agent_Configuration> Agent_Configuration { get; set; }
        public DbSet<Agent_UserCompanies> Agent_UserCompany { get; set; }
        public DbSet<Agent_ProgramClasification> Agent_ProgramClasification { get; set; }

        public DbSet<Agent_GroupHorary> Agent_GroupHorary { get; set; }
        public DbSet<Agent_GroupHoraryDetail> Agent_GroupHoraryDetail { get; set; }
        public DbSet<Agent_CompanyDepartment> Agent_CompanyDepartment { get; set; }

        public DbSet<Agent_EmployeesGroups> Agent_EmployeesGroups { get; set; }

        public DbSet<Agent_EmployeeGroupsEmployee> Agent_EmployeeGroupsEmployee { get; set; }
        public DbSet<Alerts> Alerts { get; set; }
        public DbSet<License> License { get; set; }       
        public DbSet<AlertAsociados> AlertAsociados { get; set; }

        public DbSet<Alertas> Alertas { get; set; }
    }
}