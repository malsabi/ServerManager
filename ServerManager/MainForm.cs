using ServerManager.Core.ServerWorkstation;
using ServerManager.Utilities;
using System;
using System.Windows.Forms;

namespace ServerManager
{
    public partial class MainForm : Form
    {
        private Logger MainLogger;
        private ServerController Server;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainLogger = new Logger(3000);
            Server = new ServerController(MainLogger);
            Server.Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Server.Stop();
        }
    }
}
