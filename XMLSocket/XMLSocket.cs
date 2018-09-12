using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.IO;
using System.Windows.Forms;

namespace SocketLib
{
    #region Log
    public enum LogType { Exception, Alert, Note, Passo }
    public class Log
    {
        public string Name;
        public Log(String name)
        {
            this.Name = name;
        }
        public void Add(string Title, string Message, LogType type)
        {
            //MessageBox.Show(Message, Title); // BETA
        }
    }
    #endregion

    #region Args
    public class XMLEventArgs
    {
        //public XmlDocument XML = new XmlDocument();
        public string XML;
        public XMLEventArgs(string xml)
        {
            //this.XML.LoadXml(xml);
            this.XML = xml;
        }
    }
    public class ConnectEventArgs
    {
        //public bool Sucess;
        public ConnectEventArgs()//(bool sucess)
        {
            //this.Sucess = sucess;
        }
    }
    public class CloseEventArgs
    {
        //public bool Sucess;
        public CloseEventArgs()//(bool sucess)
        {
            //this.Sucess = sucess;
        }
    }
    #endregion

    public class XMLSocket
    {
        public delegate void connect(ConnectEventArgs args);
        public event connect OnConnect;

        public delegate void close(CloseEventArgs args);
        public event close OnClose;

        public delegate void sending(XMLEventArgs args);
        public event sending OnSend;

        public delegate void receiving(XMLEventArgs args);
        public event receiving OnReceive;

        public Socket socket;
        public TcpListener tcplistener;
        public IPEndPoint endereco;

        public Log log;

        public int BuffMaxLength;

        public XMLSocket(int buffMaxLength = 8192)
        {
            log = new Log("XMLSocket");
            this.BuffMaxLength = buffMaxLength;

            // Events
            this.OnClose += XMLSocket_OnClose;
            this.OnConnect += XMLSocket_OnConnect;
            this.OnReceive += XMLSocket_OnReceive;
            this.OnSend += XMLSocket_OnSend;
        }

        #region Events
        private void XMLSocket_OnClose(CloseEventArgs args)
        {
            
        }
        private void XMLSocket_OnConnect(ConnectEventArgs args)
        {

        }
        private void XMLSocket_OnReceive(XMLEventArgs args)
        {

        }
        private void XMLSocket_OnSend(XMLEventArgs args)
        {

        }
        #endregion

        #region Server
        public void Listen(int Porta = 2121) // Server
        {
            new Thread(() =>
            {
                tcplistener = new TcpListener(IPAddress.Any, Porta);
                tcplistener.Start();
                try
                {
                    socket = tcplistener.AcceptSocket();
                    if (socket.Connected)
                    {
                        this.OnConnect.Invoke(new ConnectEventArgs());
                        log.Add("Server - Client connected", socket.RemoteEndPoint.ToString(), LogType.Passo);
                        socket.BeginReceive(new byte[] { }, 0, 0, 0, BeginReceiveCallBack, 0);
                    }
                }
                catch (Exception ex) { log.Add("Server - Listen Exception", ex.Message, LogType.Exception); }
            }).Start();
        }
        public void BeginReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                if(socket.Connected)
                {
                    socket.EndReceive(ar);
                    byte[] buff = new byte[BuffMaxLength];
                    int rec = socket.Receive(buff, buff.Length, 0);
                    if (rec < buff.Length)
                    {
                        Array.Resize<byte>(ref buff, rec);
                    }
                    XMLEventArgs args = new XMLEventArgs(Encoding.ASCII.GetString(buff));
                    this.OnReceive.Invoke(args);
                    log.Add("Server - BeginReceiveCallBack received: ", ASCIIEncoding.ASCII.GetString(buff), LogType.Passo);
                }
                else OnClose.Invoke(new CloseEventArgs());
            }
            catch (Exception ex) { log.Add("Server - BeginReceiveCallBack Exception", ex.Message, LogType.Exception); }
            finally { if (socket.Connected) socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, BeginReceiveCallBack, 0); }
        }
        #endregion

        #region Client
        public void Connect(string ip, int Porta = 2121, bool tryReconect = false) 
        {
            try
            {
                endereco = new IPEndPoint(IPAddress.Parse(ip), Porta);
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endereco);
                if (!socket.Connected)
                {
                    if (tryReconect)
                        Connect(ip, Porta);
                }
                else
                {
                    new Thread(() =>
                    {
                        this.OnConnect.Invoke(new ConnectEventArgs());
                        log.Add("Client - Connected to server", socket.RemoteEndPoint.ToString(), LogType.Passo);
                        socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, ConnectCallBack, null);
                    }).Start();
                }
            }
            catch (Exception ex) { log.Add("Connect Exception", ex.Message, LogType.Exception); }
        }
        private void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                if (socket.Connected)
                {
                    socket.EndReceive(ar);
                    byte[] buff = new byte[BuffMaxLength];
                    int rec = socket.Receive(buff, buff.Length, 0);
                    if (rec < buff.Length)
                    {
                        Array.Resize<byte>(ref buff, rec);
                    }
                    this.OnReceive.Invoke(new XMLEventArgs(Encoding.ASCII.GetString(buff)));
                    log.Add("Client - ConnectCallBack received: ", ASCIIEncoding.ASCII.GetString(buff), LogType.Passo);
                }
                else OnClose.Invoke(new CloseEventArgs());
            }
            catch (Exception ex) { log.Add("ConnectCallBack Exception", ex.Message, LogType.Exception); }
            finally { if (socket.Connected) socket.BeginReceive(new byte[] { 0 }, 0, 0, 0, BeginReceiveCallBack, 0); }
        }
        #endregion

        public void SendText(string texto)
        {
            try
            {
                if(socket.Connected)
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(texto + "\0");
                    socket.Send(buffer);
                    XMLEventArgs args = new XMLEventArgs(Encoding.ASCII.GetString(buffer));
                    this.OnSend.Invoke(args);
                }
            }
            catch (Exception ex) { log.Add("Send Exception", ex.Message, LogType.Exception); }
        }

        public void SendBytes(byte[] Bytes)
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Send(Bytes);
                    XMLEventArgs args = new XMLEventArgs(Encoding.ASCII.GetString(Bytes));
                    this.OnSend.Invoke(args);
                }
            }
            catch (Exception ex) { log.Add("Send Exception", ex.Message, LogType.Exception); }
        }

        public void Close()
        {
            socket.Close();
        }
    }
}
