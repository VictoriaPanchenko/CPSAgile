using CpsBoostAgile.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CpsBoostAgile.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}