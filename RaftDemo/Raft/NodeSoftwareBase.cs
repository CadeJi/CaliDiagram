﻿using Caliburn.Micro;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RaftDemo.Raft
{
    public abstract class NetworkSoftwareBase: PropertyChangedBase
    {
        public string Id
        {
            get;
            set;
        }

        public NetworkSoftwareBase()
        {
            Channels = new List<INodeChannel>();
        }
        List<INodeChannel> Channels
        {
            get;
            set;
        }
        protected virtual void OnInitialized() { }
        protected virtual void OnDestroyed()  {}
        protected virtual void OnChannelCreated(INodeChannel channel){}
        protected virtual void OnChannelDestroyed(INodeChannel channel){}
        protected virtual void OnMessageReceived(INodeChannel channel, object message){} 
        protected virtual void OnCommandReceived(string command){}

        protected virtual void OnTimerElapsed(TimeoutTimer timer) { }


        /// <summary>
        /// Queues message to be sent via channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(INodeChannel channel, object message)
        {
            if (!IsStarted)
                return false;

            channel.SendMessage(message);
            var onMessageSent = OnMessageSent;
            if(OnMessageSent != null)
                OnMessageSent(this, new OutboundMessage() { Message = message, DestinationChannel = channel });
            return true;
        }

        public event EventHandler<OutboundMessage> OnMessageSent;

        

        INodeChannel[] ThreadSafeChannels
        {
            get
            {
                INodeChannel[] channelsClonned = null;
                lock (Channels)
                {
                    channelsClonned = new INodeChannel[Channels.Count];
                    Channels.CopyTo(channelsClonned);
                }
                return channelsClonned;
            }
        }
        /// <summary>
        /// Broadcasts message to all active channels
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void BroadcastMessage(object message)
        {
            foreach (var channel in ThreadSafeChannels)
                SendMessage(channel, message);
        }

        /// <summary>
        /// Broadcasts message to all channels except 'except'
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public void BroadcastExcept(object message, INodeChannel except)
        {
            foreach (var channel in ThreadSafeChannels)
            {
                if (channel == except)
                    continue;
                SendMessage(channel, message);
            }            
        }

        private bool _IsStarted;
        public bool IsStarted
        {
            get { return _IsStarted; }
            set
            {
                if (_IsStarted != value)
                {
                    _IsStarted = value;
                    NotifyOfPropertyChange(() => IsStarted);
                }
            }
        }
        
        readonly object isStartedLock = new object();

        public void Start()
        {
            lock (isStartedLock)
            {
                if (IsStarted)
                    return;
                IsStarted = true;
            }
            InputQueue = new BlockingCollection<object>();
            OnInitialized();
            StartEventLoop();
        }

        public void Stop()
        {
            lock (isStartedLock)
            {
                if (IsStarted == false)
                    return;
                IsStarted = false;
            }
            InputQueue.CompleteAdding();
            InputQueue = null;
            OnDestroyed();
        }

        public BlockingCollection<object> InputQueue
        {
            get;
            private set;
        }

        void StartEventLoop()
        {
            Thread t = new Thread(EventLoop);
            t.IsBackground = true;
            t.Name = string.Format("Event loop {0}", Id);
            t.Start();
        }

        void EventLoop()
        {
            Console.WriteLine("Started event queue worker");
            foreach (object evt in InputQueue.GetConsumingEnumerable())
            {
                if (evt == null)
                    continue;
                if (!IsStarted)
                    return;

                /// timers are TimeoutTimer
                TimeoutTimer timer = evt as TimeoutTimer;
                if (timer != null)
                    OnTimerElapsed(timer);
                // strings are commands
                string command = evt as string;
                if (command != null)
                    OnCommandReceived(command);

               // channels as tuples
                var channelEvent = evt as Tuple<INodeChannel, bool>;
                if(channelEvent != null)
                {
                    if (IsStarted)
                    {
                        if (channelEvent.Item2 == false) // channel removing = false
                            OnChannelCreated(channelEvent.Item1);
                        else
                            OnChannelDestroyed(channelEvent.Item1);
                    }
                }

               // MessageReceived(null, evt);
                InboundMessage message = evt as InboundMessage;
                if (message != null)
                {
                    OnMessageReceived(message.SourceChannel, message.Message);
                }
            }

            Console.Write("Worker finished");
        }
        /// <summary>
        /// Raise text command to node algorithm
        /// </summary>
        /// <param name="command"></param>
        public void RaiseCommandReceived(string command)
        {
            InputQueue.Add(command);
        }

        /// <summary>
        /// should be called when underlying protocol detects that new connection is established
        /// </summary>
        /// <param name="connection"></param>
        public void RaiseChannelAdded(INodeChannel channel)
        {
           
            lock(Channels)
                Channels.Add(channel);

            if(IsStarted)
                InputQueue.Add(new Tuple<INodeChannel, bool>(channel, false));
        }
        /// <summary>
        /// Should be called when underlying protocol detects connection close or failure
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reason"></param>
        public void RaiseSocketDead(object socket, object reason = null)
        {
            var channelToRemove = GetChannelBySocket(socket);
            if (channelToRemove == null)
            {
                return;
            }

            if (!Channels.Remove(channelToRemove))
                throw new ArgumentException("Failed to remove channel");

            if (IsStarted)
            {
                if (!InputQueue.IsAddingCompleted)
                    InputQueue.Add(new Tuple<INodeChannel, bool>(channelToRemove, true));
            }
        }

        public void RaisePacketReceived(object packet, INodeChannel channel)
        {

            InboundMessage messageObject = new InboundMessage() { Message = packet, SourceChannel = channel };
            if(!InputQueue.IsAddingCompleted)
                InputQueue.Add(messageObject);
        }
        /// <summary>
        /// Gets channel by underlying socket
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public INodeChannel GetChannelBySocket(object socket)
        {
            lock(Channels)
                return Channels.FirstOrDefault(channel => channel.Socket == socket);
        }
    }
}
