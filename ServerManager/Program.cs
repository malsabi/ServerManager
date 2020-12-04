using ServerManager.Core.ServerWorkstation;
using ServerManager.Utilities;
using System;

namespace ServerManager
{
    class Program
    {
        private static Logger MainLogger;
        private static ServerController Server;
        
        static void Main(string[] args)
        {
            Initialize();
            Execute();
            Console.ReadKey();
        }
        private static void Initialize()
        {
            MainLogger = new Logger(3000);
            Server = new ServerController(MainLogger);
        }
        private static void Execute()
        {
            Server.StartPipeline();
            Server.Start();
        }
    }
}