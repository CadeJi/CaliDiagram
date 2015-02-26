﻿
namespace RaftAlgorithm.Messages
{
    public class RequestVoteResponse : RaftMessageBase
    {
        public bool VoteGranted { get; set; }
        public int CurrentTerm { get; set; }
        public string VoterId { get; set; }

        public override string ToString()
        {
            if (VoteGranted)
                return "Yes";
            else
                return "No";
        }
    }
}
