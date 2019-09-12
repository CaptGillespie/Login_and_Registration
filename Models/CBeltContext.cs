using Microsoft.EntityFrameworkCore;

    namespace CBelt.Models
    {
        public class CBeltContext : DbContext
        {
            public CBeltContext(DbContextOptions options) : base(options) { }
            public DbSet<RegisterUser> Logged_In_User {get;set;}
            public DbSet<Happening> happenings {get;set;}
            public DbSet<Association> Attendee {get;set;}

        }
    }
