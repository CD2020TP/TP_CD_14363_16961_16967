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

namespace Client
{
    public partial class ClientChat : Form
    {
        public ClientChat()
        {
            InitializeComponent();
        }

        internal byte[] m_dataBuffer = new byte[10];

        internal IAsyncResult m_result;

        /// <summary>
        /// Defines the m_pfnCallBack
        /// </summary>
        /// 
        public AsyncCallback m_pfnCallBack;

        public Socket m_clientSocket;

        private String clientId;

        private bool isFirstPacket;

        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = m_clientSocket;
                // Start listening to the data asynchronously
                m_result = m_clientSocket.BeginReceive(theSocPkt.dataBuffer,
                                                        0, theSocPkt.dataBuffer.Length,
                                                        SocketFlags.None,
                                                        m_pfnCallBack,
                                                        theSocPkt);
            }

            //Message in case of error
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }


        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;

            public byte[] dataBuffer = new byte[255];
        }


        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);

                // First packet is the ID for this client
                if (isFirstPacket)
                {
                    isFirstPacket = false;
                    clientId = szData;
                    UpdateControls(theSockId.thisSocket.Connected);
                }
                else
                {
                    LogIncomingMessageToForm(szData);
                }

                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
                UpdateControls(false);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
                UpdateControls(false);
            }
        }

        //Function that updates the state of the client in case of connecting or not to the server
        private void UpdateControls(bool connected)
        {
            Invoke(new Action(() =>
            {
                btnConnect.Enabled = !connected;
                btnDisconnect.Enabled = connected;
                string connectStatus = "";
                if (connected)
                {
                    connectStatus = String.Format("Connected ClientID:{1}", connectStatus, clientId);
                }
                else
                {
                    connectStatus = "Not Connected";
                    clientId = "";
                }
                txtConnectStatus.Text = connectStatus;
            }));
        }
        
        //Messages received by clients
        internal void LogIncomingMessageToForm(String msg)
        {
            Invoke(new Action(() =>
            {
                //String message = String.Format("{0} - {1}", GetTimeStamp(DateTime.Now), msg);
                txtRecevied.AppendText(msg);
                txtRecevied.AppendText(Environment.NewLine); // Only works as a seperate append...
                txtRecevied.ScrollToCaret();
            }));
        }

        //Funtion to get and write the time Of the messages
        internal String GetTimeStamp(DateTime value)
        {
            return value.ToString("hh:mm");
        }

        //funtion to get the IP
        internal String GetIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endpoint = socket.LocalEndPoint as IPEndPoint;
                return endpoint.Address.ToString();
            }
        }

        //connection to the server
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // See if we have text on the IP and Port text fields
            if (txtboxServer.Text == "" || txtboxPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return;
            }
            try
            {
                UpdateControls(false);
                // Create the socket instance
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Set the remote IP address
                IPAddress ip = IPAddress.Parse(txtboxServer.Text);
                int iPortNo = System.Convert.ToInt16(txtboxPort.Text);
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);
                // Connect to the remote host
                m_clientSocket.Connect(ipEnd);
                if (m_clientSocket.Connected)
                {

                    UpdateControls(true);
                    //Wait for data asynchronously 
                    isFirstPacket = true;
                    WaitForData();
                }
            }
            //Error message in case of server not inicialized
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                MessageBox.Show(str);
                UpdateControls(false);
            }
        }

        //Disconnect client from server
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
                UpdateControls(false);
            }
        }

        //Send the message written on the box
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                Object objData = txtSend.Text;
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                if (m_clientSocket != null)
                {
                    m_clientSocket.Send(byData);
                    // Clear the message box
                    Invoke(new Action(() =>
                    {
                        txtSend.Text = "";
                    }));
                }

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        //close the chat window
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
            }
            Close();
        }

        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtRecevied_TextChanged(object sender, EventArgs e)
        {

        }

        private void ClientChat_Load(object sender, EventArgs e)
        {

        }
    }
}
