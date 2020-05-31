using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;


namespace Server
{
    
    public partial class Server : Form
    {
        internal const int MAX_CLIENTS = 10; //limit of clients connected at the same time

        public AsyncCallback pfnWorkerCallBack;

        private Socket m_mainSocket;

        private Socket[] m_workerSocket = new Socket[30];// limit of 30 TOTAL CONNECTIONS!!

        private int m_clientCount = 0;

        //control to send status of server
        private void UpdateControls(bool listening)
        {
            btnStart.Enabled = !listening;
            btnStop.Enabled = listening;
        }
        internal void SendMsgToAll(String msg)
        {
            SendToAll(AddMetaToMessage("", msg));
        }
        internal void SendMsgToAll(String id, String msg)
        {
            SendToAll(AddMetaToMessage(id, msg));
        }
        internal void LogIncomingMessageToForm(String msg)
        {
            LogIncomingMessageToForm("", msg);
        }

        //select all clients to send message
        internal void SendToAll(String message)
        {
            try
            {
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(message);
                for (int i = 0; i < m_clientCount; i++)
                {
                    if (m_workerSocket[i] != null)
                    {
                        if (m_workerSocket[i].Connected)
                        {
                            m_workerSocket[i].Send(byData);
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private CheckBox broadcastCheckbox;
        //Get date 
        internal String GetTimeStamp(DateTime value)
        {
            return value.ToString("hh:mm");
        }

        //Message format
        internal String AddMetaToMessage(String id, String msg)
        {
            id = id == "" ? "S" : id;
            String message = String.Format("{0}#{1} - {2}", GetTimeStamp(DateTime.Now), id, msg);
            return message;
        }
        private bool broadcastIncomingMessages = false;

        public class SocketPacket
        {
            // Defines the m_currentSocket
            public System.Net.Sockets.Socket m_currentSocket;

            // Defines the dataBuffer
            public byte[] dataBuffer = new byte[255];
        
            /// Defines the id
            public int id;
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                int iRx = 0;
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                iRx = socketData.m_currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.dataBuffer,
                                         0, iRx, chars, 0);
                System.String szData = new System.String(chars);
                LogIncomingMessageToForm(socketData.id.ToString(), szData);
                if (broadcastIncomingMessages)
                {
                    SendMsgToAll(socketData.id.ToString(), szData);
                }

                // Continue the waiting for data on the Socket
                WaitForData(socketData.id);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void WaitForData(int id)
        {
            Socket soc = m_workerSocket[id];
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = soc;
                theSocPkt.id = id;
                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.dataBuffer, 0,
                                   theSocPkt.dataBuffer.Length,
                                   SocketFlags.None,
                                   pfnWorkerCallBack,
                                   theSocPkt);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        internal void LogIncomingMessageToForm(String clientId, String msg)
        {
            String message = AddMetaToMessage(clientId, msg);
            Invoke(new Action(() =>
            {
                TextBoxfromClients.AppendText(message);
                TextBoxfromClients.AppendText(Environment.NewLine); // Only works as a seperate append...
                TextBoxfromClients.ScrollToCaret();
            }));
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                m_workerSocket[m_clientCount] = m_mainSocket.EndAccept(asyn);

                // Display this client connection as a status message on the GUI	
                String str = String.Format("Client # {0} connected", m_clientCount);
                
                LogIncomingMessageToForm(str);
                if (broadcastIncomingMessages)
                {
                    SendMsgToAll(str);
                }

                // Send the client their ID (First thing the server sends!)
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(m_clientCount.ToString());
                m_workerSocket[m_clientCount].Send(byData);

                // Let the worker Socket do the further processing for the 
                // just connected client
                WaitForData(m_clientCount);

                // Now increment the client count
                ++m_clientCount;
                // Since the main Socket is now free, it can go back and wait for
                // other clients who are attempting to connect
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }
        public Server()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxfromClients_TextChanged(object sender, EventArgs e)
        {

        }
        
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                // Check the port value
                if (Port.Text == "")
                {
                    MessageBox.Show("Please enter a Port Number");
                    return;
                }
                string portStr = Port.Text;
                int port = System.Convert.ToInt32(portStr);
                // Create the listening socket...
                m_mainSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port);
                // Bind to local IP Address...
                m_mainSocket.Bind(ipLocal);
                // Start listening...
                m_mainSocket.Listen(4);
                // Create the call back for any client connections...
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);

                UpdateControls(true);

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            CloseSockets();
            UpdateControls(false);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            String msg = TextBoxMsgtoClients.Text;
            SendMsgToAll(msg);
            Invoke(new Action(() =>
            {
                TextBoxMsgtoClients.Text = "";
            }));
        }

        private void BroadcastcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            broadcastIncomingMessages = checkbox.Checked;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseSockets();
            Close();
        }

        internal void CloseSockets()
        {
            if (m_mainSocket != null)
            {
                m_mainSocket.Close();
            }
            for (int i = 0; i < m_clientCount; i++)
            {
                if (m_workerSocket[i] != null)
                {
                    m_workerSocket[i].Close();
                    m_workerSocket[i] = null;
                }
            }
        }
    }

}

