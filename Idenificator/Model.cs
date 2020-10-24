using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Identificator_Serv
{
    public class IdentContext : DbContext
    {
        public DbSet<Ident> Idents { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=idents.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ident>().HasKey(e => new { e.IdentId, e.GroupId });
            modelBuilder.Entity<Group>().HasIndex(u => u.Prefix).IsUnique();
        }
    }

    public class Ident
    {
        public int IdentId { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public int Status { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [RegularExpression(@"^[A-Z]+[1-9]\d*$")]
        public string Prefix { get; set; }

        public int Next_Id { get; set; }

    }

    public static class Handler
    {
        public static void ResetIdent(object obj)
        {
            var db = new IdentContext();
            db.Database.ExecuteSqlRaw("DELETE FROM [Idents]");
            db.Groups.Load();

            foreach(var e in db.Groups)
            {
                e.Next_Id = 0;
            }

            db.SaveChanges();
        }
    }
}
