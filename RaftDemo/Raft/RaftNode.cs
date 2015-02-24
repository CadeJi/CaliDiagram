﻿using RaftDemo.Raft.Messages;
using RaftDemo.Raft.State;
using System;
using System.Collections.Generic;


/*
 * notes - election timeout 150 - 300 ms  
 * broadcastTime ≪ electionTimeout ≪ MTBF
 */

namespace RaftDemo.Raft
{
    public class RaftNode : NetworkSoftwareBase
    {
        public RaftNode(IRaftEventListener raftEventListener, IRaftNodeSettings raftSettings)
        {
            if (raftEventListener == null)
                throw new ArgumentNullException("raftWorld");
            RaftEventListener = raftEventListener;
            RaftSettings = raftSettings;
            CurrentTerm = 0;
            RaftTimer = new TimeoutTimer(this);
            LogEntries = new List<LogEntry>();
            LogEntries.Add(new LogEntry() { Data = "A=2", CommitIndex = 1, Term = 1 });
            LogEntries.Add(new LogEntry() { Data = "C=1", CommitIndex = 2, Term = 1 });
        }

        public IRaftEventListener RaftEventListener
        {
            get;
            private set;
        }
        public IRaftNodeSettings RaftSettings
        {
            get;
            private set;
        }
        public List<LogEntry> LogEntries
        {
            get;
            set;
        }

        public TimeoutTimer RaftTimer
        {
            get;
            private set;
        }   

        protected override void OnInitialized()
        {
            RaftTimer.Init();
            TranslateToState(RaftNodeState.Follower);
        }

        public void TranslateToState(RaftNodeState newState)
        {
            RaftStateBase newStateObject = null;

            if (newState == RaftNodeState.Candidate)
                newStateObject = new Candicate(this);
            else if (newState == RaftNodeState.Follower)
                newStateObject = new Follower(this);
            else if (newState == RaftNodeState.Leader)
                newStateObject = new Leader(this);
            else
                throw new ArgumentException();
           
            var state = State;
            if(state != null)
            {
                state.ExitState();
            }
            State = newStateObject;
            State.EnterState();            
        }

        private RaftStateBase _State;
        public RaftStateBase State
        {
            get { return _State; }
            set
            {
                if (_State != value)
                {
                    _State = value;
                    NotifyOfPropertyChange(() => State);
                }
            }
        }
        
        private int _CurrentTerm;
        public int CurrentTerm
        {
            get { return _CurrentTerm; }
            set
            {
                if (_CurrentTerm != value)
                {
                    _CurrentTerm = value;
                    VotedFor = null;
                    NotifyOfPropertyChange(() => CurrentTerm);
                }
            }
        }

        /// <summary>
        /// Candiate Id that received vote in current term
        /// </summary>
        private string _VotedFor;
        public string VotedFor
        {
            get { return _VotedFor; }
            set
            {
                if (_VotedFor != value)
                {
                    _VotedFor = value;
                    NotifyOfPropertyChange(() => VotedFor);
                }
            }
        }
        
        

        public void IncrementAndSend()
        {
            Message message = new Message() { Clock = ++CurrentTerm};
            BroadcastMessage(message);            
        }

       

        protected override void OnDestroyed()
        {
            RaftTimer.Dispose();
        }
        protected override void OnChannelCreated(INodeChannel channel)
        {
          
        }
        protected override void OnChannelDestroyed(INodeChannel channel)
        {
         
        }

        protected override void OnMessageReceived(INodeChannel channel, object message)
        {
            RaftMessageBase raftMessage = message as RaftMessageBase;
            if (raftMessage != null)
            {
                var appEntries = raftMessage as AppendEntries;
                var appEntriesResponse = raftMessage as AppendEntriesResponse;
                var requestVote = raftMessage as RequestVote;
                var requestVoteResponse = raftMessage as RequestVoteResponse;

                if (appEntries != null)
                    State.ReceiveAppendEntries(appEntries,channel);
                else if (appEntriesResponse != null)
                    State.ReceiveAppendEntriesResponse(appEntriesResponse, channel);
                else if (requestVote != null)
                    State.ReceiveRequestVote(requestVote, channel);
                else if (requestVoteResponse != null)
                    State.ReceiveRequestVoteResponse(requestVoteResponse, channel);
            }
        }

        protected override void OnCommandReceived(string command) // Queue processing thread
        {
            if (command == "send")
                IncrementAndSend();            
        }
        protected override void OnTimerElapsed(TimeoutTimer timer) // Queue processing thread
        {
            if (timer == RaftTimer)
            {
                if(State != null)
                    State.OnTimeout();
            }
        }        
    }
}
