using IViewNet.Server;
using IViewNet.Common;
using IViewNet.Common.Models;
using IViewNet.Pipes;
using ServerManager.Core.ServerWorkstation.Handlers;
using ServerManager.Utilities;
using ServerManager.Models;
using System;

namespace ServerManager.Core.ServerWorkstation
{
    public class ServerController
    {
        #region "Constants"
        private const int MAX_PIPES = 1;
        private const int MAXIMUM_BACKLOG = 1000;
        private const int MAXIMUM_CONNECTIONS = 500;
        private const int MESSAGE_SIZE = 1024 * 1024 * 10;
        private const int BUFFER_SIZE = 1024 * 200;
        private const int HEADER_SIZE = 4;
        #endregion

        #region "Private Fields"
        private Logger Logger;
        private NetConfig ServerConfig;
        private Server ServerInstance;
        private PipeConfig Config;
        private IViewPipeServer Pipeline;
        private PacketHandler PacketHandler;
        #endregion

        public ServerController(Logger Logger)
        {
            this.Logger = Logger;
            InitializePipe();
            InitializeServer();
        }

        #region "Private Methods"
        #region "Pipeline"
        private void InitializePipe()
        {
            Pipeline = new IViewPipeServer(CreatePipeConfig())
            {
                PacketManager = CreatePacketManager()
            };
            Logger.Log(new Log("ServerPipelining Successfully Initialized ", ConsoleColor.Green));
        }
        private PipeConfig CreatePipeConfig()
        {
            Config = new PipeConfig(BUFFER_SIZE, MAX_PIPES);
            return Config;
        }
        private void AddPipeHandlers()
        {
            Pipeline.PipeConnectedEvent += SetOnPipeConnected;
            Pipeline.PipeReceivedEvent += SetOnPipeReceived;
            Pipeline.PipeSentEvent += SetOnPipeSent;
            Pipeline.PipeClosedEvent += SetOnPipeClosed;
            Pipeline.PipeExceptionEvent += SetOnPipeException;
        }
        private void RemovePipeHandlers()
        {
            Pipeline.PipeConnectedEvent -= SetOnPipeConnected;
            Pipeline.PipeReceivedEvent -= SetOnPipeReceived;
            Pipeline.PipeSentEvent -= SetOnPipeSent;
            Pipeline.PipeClosedEvent -= SetOnPipeClosed;
            Pipeline.PipeExceptionEvent -= SetOnPipeException;
        }
        private void SetOnPipeConnected()
        {
            Logger.Log(new Log("End Pipe Connected Successfully", ConsoleColor.Green));
        }
        private void SetOnPipeReceived(Packet Message)
        {
            Logger.Log(new Log(string.Format("Pipe Received From PyExecutor: {0}", Message.Name), ConsoleColor.Yellow));
            ServerInstance.OnlineClients[0].SendPacket(Message);
        }
        private void SetOnPipeSent(Packet Message)
        {
            Logger.Log(new Log(string.Format("Pipe Sent To PyExecutor: {0}", Message.Name), ConsoleColor.Blue));
        }
        private void SetOnPipeClosed()
        {
            Logger.Log(new Log("End Pipe Closed Successfully", ConsoleColor.Cyan));
        }
        private void SetOnPipeException(Exception Error)
        {
            Logger.Log(new Log(string.Format("Pipe Exception: {0}", Error.Message), ConsoleColor.Red));
        }
        #endregion

        #region "Server"
        private void InitializeServer()
        {
            ServerInstance = new Server(CreateServerConfig())
            {
                PacketManager = CreatePacketManager()
            };
            PacketHandler = new PacketHandler(Logger, Pipeline);
            Logger.Log(new Log("Server Successfully Initialized", ConsoleColor.Green));
        }
        private NetConfig CreateServerConfig()
        {
            ServerConfig = new NetConfig();
            ServerConfig.SetMaxBackLogConnections(MAXIMUM_BACKLOG);
            ServerConfig.SetMaxConnections(MAXIMUM_CONNECTIONS);
            ServerConfig.SetMaxMessageSize(MESSAGE_SIZE);
            ServerConfig.SetBufferSize(BUFFER_SIZE);
            ServerConfig.SetHeaderSize(HEADER_SIZE);
            ServerConfig.SetEnableKeepAlive(false);
            ServerConfig.SetPort(1669);
            return ServerConfig;
        }
        private void AddServerHandlers()
        {
            ServerInstance.OnClientConnect += OnClientConnectHandler;
            ServerInstance.OnClientReceive += OnClientReceiveHandler;
            ServerInstance.OnClientSend += OnClientSendHandler;
            ServerInstance.OnClientDisconnect += OnClientDisconnectHandler;
            ServerInstance.OnClientException += OnClientExceptionHandler;
            ServerInstance.OnException += OnServerException;
        }
        private void RemoveServerHandlers()
        {
            ServerInstance.OnClientConnect -= OnClientConnectHandler;
            ServerInstance.OnClientReceive -= OnClientReceiveHandler;
            ServerInstance.OnClientSend -= OnClientSendHandler;
            ServerInstance.OnClientDisconnect -= OnClientDisconnectHandler;
            ServerInstance.OnClientException -= OnClientExceptionHandler;
            ServerInstance.OnException -= OnServerException;
        }
        private void OnServerException(Exception Ex)
        {
            Logger.Log(new Log(string.Format("Server Exception: {0}", Ex.Message), ConsoleColor.Red));
        }
        private void OnClientConnectHandler(Operation Client)
        {
            Logger.Log(new Log(string.Format("Client[{0}] Successfully Connected", Client.EndPoint), ConsoleColor.Green));
        }
        private void OnClientReceiveHandler(Operation Client, Packet Command)
        {
            Logger.Log(new Log(string.Format("Client[{0}] Received Command Name: {1}, Command Content: {2} Bytes", Client.EndPoint, Command.Name, Command.Content.Length), ConsoleColor.Blue));
            PacketHandler.HandleFromClient(Client, Command);
        }
        private void OnClientSendHandler(Operation Client, Packet Command)
        {
            Logger.Log(new Log(string.Format("Client[{0}] Sent Command Name: {1}, Command Content: {2} Bytes", Client.EndPoint, Command.Name, Command.Content.Length), ConsoleColor.Yellow));
        }
        private void OnClientDisconnectHandler(Operation Client, string Reason)
        {
            Logger.Log(new Log(string.Format("Client[{0}] Disconnected {1}", Client.EndPoint, Reason), ConsoleColor.Cyan));
        }
        private void OnClientExceptionHandler(Operation Client, Exception Ex)
        {
            Logger.Log(new Log(string.Format("Client[{0}] Exception {1}", Client.EndPoint, Ex.Message), ConsoleColor.Red));
        }
        #endregion

        #region "Shared"
        private PacketManager CreatePacketManager()
        {
            PacketManager PacketManager = new PacketManager();
            PacketManager.AddPacket(new Packet(1111, "SetDetectionType", null));
            PacketManager.AddPacket(new Packet(1112, "SetOrientation", null));
            PacketManager.AddPacket(new Packet(1113, "GetDetectedFrame", null));
            PacketManager.AddPacket(new Packet(1114, "SetDetectedFrame", null));
            PacketManager.AddPacket(new Packet(1115, "EndOfFrame", null));
            return PacketManager;
        }
        #endregion
        #endregion

        #region "Public Methods"
        public void StartPipeline()
        {
            if (Pipeline.IsPipeConnected == false)
            {
                AddPipeHandlers();
                Pipeline.StartPipeServer();
            }
        }
        public void StopPipeline()
        {
            if (Pipeline.IsPipeShutdown == false)
            {
                RemovePipeHandlers();
                Pipeline.ClosePipeServer();
            }
        }
        public void SendPipeline(Packet Message)
        {
            if (Pipeline.IsPipeConnected)
            {
                Pipeline.SendMessage(Message);
            }
        }
        public void Start()
        {
            StartListenerResult ListenerResult = ServerInstance.StartListener();
            if (ListenerResult.IsOperationSuccess)
            {
                Logger.Log(new Log(ListenerResult.Message, ConsoleColor.Green));
                StartAcceptorResult AcceptorResult = ServerInstance.StartAcceptor();
                if (AcceptorResult.IsOperationSuccess)
                {
                    Logger.Log(new Log(AcceptorResult.Message, ConsoleColor.Green));
                    AddServerHandlers();
                }
                else
                {
                    Logger.Log(new Log(AcceptorResult.Message, ConsoleColor.Red));
                }
            }
            else
            {
                Logger.Log(new Log(ListenerResult.Message, ConsoleColor.Red));
            }
        }
        public void Stop()
        {
            if (ServerInstance.IsListening)
            {
                ShutdownResult Result = ServerInstance.Shutdown();
                if (Result.IsOperationSuccess)
                {
                    Logger.Log(new Log(Result.Message, ConsoleColor.Cyan));
                    RemoveServerHandlers();
                }
                else
                {
                    Logger.Log(new Log(Result.Message, ConsoleColor.Red));
                }
            }
        }
        #endregion
    }
}
