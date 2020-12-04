using IViewNet.Common;
using IViewNet.Common.Models;
using IViewNet.Pipes;
using ServerManager.Models;
using ServerManager.Utilities;
using System;

namespace ServerManager.Core.ServerWorkstation.Handlers
{
    public class PacketHandler
    {
        private readonly Logger Logger;
        private readonly IViewPipeServer Pipeline;

        public PacketHandler(Logger Logger, IViewPipeServer Pipeline)
        {
            this.Logger = Logger;
            this.Pipeline = Pipeline;
        }
        public void HandleFromClient(Operation Client, Packet Command)
        {
            if (Command.Code == 1111)           //SetDetectionType
            {
                Pipeline.SendMessage(new Packet(1111, "SetDetectionType", Command.Content));
            }
            else if (Command.Code == 1112)      //SetOrientation
            {
                Pipeline.SendMessage(new Packet(1112, "SetOrientation", Command.Content));
            }
            else if (Command.Code == 1113)      //GetDetectedFrame
            {
                Pipeline.SendMessage(new Packet(1113, "GetDetectedFrame", Command.Content));
            }
            else
            {
                Logger.Log(new Log("Invalid packet code: " + Command.Code, ConsoleColor.Red));
            }
        }
    }
}