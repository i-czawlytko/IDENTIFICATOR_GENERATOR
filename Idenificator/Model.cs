using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    public class EFIdentRepository //: IBidRepository
    {
        private IdentContext context;

        public EFIdentRepository(IdentContext ctx)
        {
            context = ctx;
        }

        public IEnumerable<Ident> Idents => context.Idents.Include(p => p.Group).ToArray();
        public IEnumerable<Group> Groups => context.Groups.ToArray();

        public Group GetGroup(int group_id)
        {
            return this.Groups.FirstOrDefault(x => x.Id == group_id);
        }

        public string AddIdentToGroup(Group group)
        {
            group.Next_Id++;

            Ident new_ident = new Ident { GroupId = group.Id, IdentId = group.Next_Id, Status = new System.Random().Next(0, 2) };
            context.Idents.Add(new_ident);
            context.SaveChanges();
            return $"{new_ident.Group.Prefix}{new_ident.IdentId}";//check
        }

        public string GetGroupList()
        {
            string result = String.Empty;

            foreach (var e in this.Groups)
            {
                result += $"{e.Id}. {e.Title}\n";
            }

            return result;
        }

        public Ident GetIdent(string code)
        {
            string pattern = @"(\d+)";
            string[] query_parts = Regex.Split(code, pattern);

            return this.Idents.FirstOrDefault(x => x.Group.Prefix == query_parts[0] && x.IdentId == int.Parse(query_parts[1]));
        }
    }

    public static class Restarter
    {
        public static void RestartIdents(IApplicationBuilder app, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<IdentContext>();

                context.Database.ExecuteSqlRaw("DELETE FROM [Idents]");

                foreach (var e in context.Groups)
                {
                    e.Next_Id = 0;
                }

                context.SaveChanges();
                logger.LogInformation($"{DateTime.Now}: Внимание! Идентификаторы сброшены");
            }

        }
    }
}
