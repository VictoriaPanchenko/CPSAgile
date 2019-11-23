using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using CpsBoostAgile.DAO;

namespace CpsBoostAgile.DbContext
{
    public class CpsContext : System.Data.Entity.DbContext
    {
        public CpsContext() : base("DefaultConnection")
        {

        }

        public static CpsContext Create()
        {
            return new CpsContext();
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<RetrospectiveItem> RetrospectiveItems { get; set; }
        public DbSet<Retrospective> Retrospectives { get; set; }
        public DbSet<UserStory> UserStories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}