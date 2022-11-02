﻿using MonoCore;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E3Core.Server
{
    public class PubClient
    {
        public static ConcurrentQueue<string> _pubCommands = new ConcurrentQueue<string>();

        Task _serverThread;
        private Int32 _port;
        public void Start(Int32 port)
        {
            _port = port;
            _serverThread = Task.Factory.StartNew(() => { Process(); }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }
        public void Process()
        {
            while(Core._isProcessing)
            {
                //some type of delay if our sub errors out.
                System.Threading.Thread.Sleep(100);
                TimeSpan recieveTimeout = new TimeSpan(0, 0, 0, 0, 1);
                using (var subSocket = new SubscriberSocket())
                {
                    try
                    {
                        subSocket.Options.ReceiveHighWatermark = 1000;
                        subSocket.Options.TcpKeepalive = true;
                        subSocket.Options.TcpKeepaliveIdle = TimeSpan.FromSeconds(5);
                        subSocket.Options.TcpKeepaliveInterval = TimeSpan.FromSeconds(1);
                        subSocket.Connect("tcp://127.0.0.1:" + _port);
                        subSocket.Subscribe("OnCommand");
                        while (Core._isProcessing)
                        {
                            string messageTopicReceived;
                            if (subSocket.TryReceiveFrameString(recieveTimeout, out messageTopicReceived))
                            {
                                string messageReceived = subSocket.ReceiveFrameString();
                                if (messageTopicReceived == "OnCommand")
                                {
                                    _pubCommands.Enqueue(messageReceived);
                                }
                            }
                        }
                    }
                    catch(Exception)
                    {

                    }
                    
                }
               
            }
           
        }
    }
}
