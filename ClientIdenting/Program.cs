using Grpc.Net.Client;
using Identificator_Serv.Protos;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClientIdenting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Identer.IdenterClient(channel);

            while (true)
            {
                Console.WriteLine("Нажмите клавишу 'G' чтобы получить идентификатор или клавишу 'S' чтобы проверить факт исполения по полученному идентификатору");
                char mode = Char.ToUpper(Console.ReadKey(true).KeyChar);
                while (mode != 'G' && mode != 'S')
                {
                    Console.WriteLine("Нужно нажать клавишу 'G', либо клавишу 'S'!");
                    mode = Char.ToUpper(Console.ReadKey(true).KeyChar);
                }
                    
                if (mode == 'G')
                {
                    Console.WriteLine("-----");
                    Console.WriteLine("Список всех доступных групп:");
                    var src = await client.GroupListAsync(new VoidRequest { });
                    Console.Write(src.Content);

                    Console.Write("Введите номер группы: ");
                    int group_id;
                    while (!int.TryParse(Console.ReadLine(),out group_id) || group_id <= 0)
                    {
                        Console.Write("Номером группы может быть только целое положительное число! Введите номер группы: ");
                    }

                    var reply = await client.sendGroupAsync(new GroupRequest { GroupId = group_id });
                    Console.WriteLine("Ваш идентификатор: " + reply.Code);
                }
                else if (mode == 'S')
                {
                    Console.WriteLine("-----");
                    Console.Write("Введите идентификатор: ");
                    string code = Console.ReadLine();

                    Regex rgx = new Regex(@"^[A-Z]+[1-9]\d*$");
                    if (!rgx.IsMatch(code))
                    {
                        Console.WriteLine("Неправильный формат введенных данных");
                        continue;
                    }

                    var status = await client.CheckStatusAsync(new Identificator { Code = code });

                    if (status.Flag < 0)
                    {
                        Console.WriteLine("Введенный идентификатор отсутствует в базе");
                    }
                    else
                    {
                        Console.WriteLine("{0}: {1}", code, status.Flag == 1 ? "Исполнен" : "Не исполнен");
                    }
                }
                else
                {
                    Console.WriteLine("Неизвестный режим");
                }

                Console.WriteLine("Нажмите клавишу 'Q', чтобы выйти из программы, либо любую другую, если хотите продолжить");
                if (Char.ToUpper(Console.ReadKey(true).KeyChar) == 'Q') break;
            }

        }
    }
}
