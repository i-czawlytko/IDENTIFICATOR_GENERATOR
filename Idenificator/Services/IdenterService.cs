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
        private EFIdentRepository repository;
        public IdenterService(ILogger<IdenterService> logger, EFIdentRepository repo)
        {
            _logger = logger;
            repository = repo;
        }

        public override Task<CodeReply> sendGroup(GroupRequest request, ServerCallContext context)
        {
            string result;

            var group = repository.GetGroup(request.GroupId);

            if (group is null)
            {
                result = "Группы с таким номером не существует";
            }
            else
            {
                result = repository.AddIdentToGroup(group);
            }

            return Task.FromResult(new CodeReply
            {
                Code = result
            });
        }

       public override Task<ListReply> GroupList(VoidRequest request, ServerCallContext context)
        {
            string result = repository.GetGroupList();

            return Task.FromResult(new ListReply
            {
                Content = result
            });
        }

        public override Task<Protos.Status> CheckStatus(Protos.Identificator request, ServerCallContext context)
        {
            var ident= repository.GetIdent(request.Code);

            if(ident is null)
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
                    Flag = ident.Status
                });
            }

        }
    }
}
