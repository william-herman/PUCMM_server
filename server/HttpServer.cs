﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace server
{
    class HttpServer : IDisposable
    {
        #region Members
        private HttpServerState _state;
        public TcpListener _tcpListener;
        private object _synLock = new object();
        private Dictionary<HttpClient, bool> _clients = new Dictionary<HttpClient, bool>();
        private AutoResetEvent clientsChangedEvent = new AutoResetEvent(false);
        private int _readBufferSize;
        private int _writeBufferSize;
        private TimeSpan _readTime;
        private TimeSpan _writeTime;
        private TimeSpan _shutDown;
        private string _serverBanner;
        private bool _disposed = false;
        #endregion

        public HttpServer(int port)
        {
            EndPoint = new IPEndPoint(IPAddress.Loopback, port);
            State = new HttpServerState();
            ReadBufferSize = 4096;
            WriteBufferSize = 4096;
            ShutdownTimeOut = TimeSpan.FromSeconds(90);
            WriteTimeOut = TimeSpan.FromSeconds(90);
            ReadTimeOut = TimeSpan.FromSeconds(90);
            ServerBanner = String.Format("PUCMM_HTTP/{0}", GetType().Assembly.GetName().Version);


        }

        #region Methods
        public void _Start()
        {
            _tcpListener = new TcpListener(EndPoint);
            try
            {
                _tcpListener.Start();
                EndPoint = (IPEndPoint)_tcpListener.LocalEndpoint;
                State = HttpServerState.Started;
            }
            catch
            {
                Console.WriteLine("Server could not start.");
                State = HttpServerState.Stopped;
            }
        }

        public void _Stop()
        {
            try
            {
                _tcpListener.Stop();
                State = HttpServerState.Stopped;
            }
            catch
            {
                Console.WriteLine("Server could not stop.");
                State = HttpServerState.Stopping;
            }
        }

        private void BeginAcceptTcpClient() { }
        private void AcceptTcpClientCallback(IAsyncResult ar) { }
        private void RegisterClient(HttpServer serv) { }

        void IDisposable.Dispose()
        {
            _disposed = true;
        } 

        private void VerifyState(HttpServerState state)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_state != state)
                throw new InvalidOperationException(String.Format("Expected server to be in the '{0}' state", state));
        }
        #endregion

        #region Properties
        public HttpServerState State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnStateChanged(EventArgs.Empty);
            }
        }

        public event EventHandler StateChanged;
        protected virtual void OnStateChanged(EventArgs args)
        {
            var ev = StateChanged;
            if (ev != null)
            {
                ev(this, args);
            }
        }

        public int ReadBufferSize
        {
            get { return _readBufferSize; }
            set { _readBufferSize = value; }
        }

        public int WriteBufferSize
        {
            get { return _writeBufferSize; }
            set { _writeBufferSize = value; }
        }

        public TimeSpan ReadTimeOut
        {
            get { return _readTime; }
            set { _readTime = value; }
        }

        public TimeSpan WriteTimeOut
        {
            get { return _writeTime; }
            set { _writeTime = value; }
        }

        public TimeSpan ShutdownTimeOut
        {
            get { return _shutDown; }
            set { _shutDown = value; }
        }

        public string ServerBanner
        {
            get { return _serverBanner; }
            set { _serverBanner = value; }
        }

        public IPEndPoint EndPoint { get; set; }
        #endregion

    }
}
