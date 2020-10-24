using Grpc.Core;
using Identificator_Serv.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Identificator_Serv.Services
{
    public class IdenterService : Identer.IdenterBase
    {
        private readonly ILogger<IdenterService> _logger;
        public IdenterService(ILogger<IdenterService> logger)
        {
            _logger = logger;
        }

        public override Task<CodeReply> sendGroup(GroupRequest request, ServerCallContext context)
        {
            var db = new IdentContext();
            db.Idents.Load();
            db.Groups.Load();

            string result;

            var group = db.Groups.FirstOrDefault(x => x.Id == request.GroupId);

            if (group is null)
            {
                result = "Группы с таким номером не существует";
            }
            else
            {
                group.Next_Id = group.Next_Id + 1;

                Ident new_code = new Ident { GroupId = request.GroupId, IdentId = group.Next_Id, Status = new Random().Next(0, 2) };
                db.Idents.Add(new_code);
                db.SaveChanges();
                result = $"{group.Prefix}{new_code.IdentId}";
            }


            return Task.FromResult(new CodeReply
            {
                Code = result
            });
        }

       public override Task<ListReply> GroupList(VoidRequest request, ServerCallContext context)
        {
            var db = new IdentContext();

            db.Groups.Load();

            string result = String.Empty;

            foreach(var e in db.Groups)
            {
                result += $"{e.Id}. {e.Title}\n";
            }


            return Task.FromResult(new ListReply
            {
                Content = result
            });
        }

        public override Task<Protos.Status> CheckStatus(Protos.Identificator request, ServerCallContext context)
        {
            var db = new IdentContext();

            db.Idents.Load();

            string pattern = @"(\d+)";
            string[] query_parts = Regex.Split(request.Code, pattern);

            var elem = db.Idents.Include(e => e.Group).FirstOrDefault(x=> x.Group.Prefix== query_parts[0] && x.IdentId==int.Parse(query_parts[1]));

            if(elem is null)
            {
                return Task.FromResult(new Protos.Status
                {
                    Flag = -1
                });
            }
            else
            {
                return Task.FromResult(new Protos.Status
                {
                    Flag = elem.Status
                });
            }

        }
    }
}
