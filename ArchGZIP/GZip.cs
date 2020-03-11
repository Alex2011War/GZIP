using System;
using System.Threading;

namespace ArchGZIP
{

    public abstract class GZip
    {
        protected bool Cancelled;
        protected bool Success = false;
        protected string SourceFile, DestinationFile;
        protected static int Threads = Environment.ProcessorCount;

        protected int BlockSize = 10000000;
        protected QueueManager QueueReader = new QueueManager();
        protected QueueManager QueueWriter = new QueueManager();
        protected WaitHandle[] DoneEvents = new WaitHandle[Threads];

        public GZip()
        {

        }
        public GZip(string input, string output)
        {
            this.SourceFile = input;
            this.DestinationFile = output;
        }

        public int CallBackResult()
        {
            if (!Cancelled && Success)
                return 0;
            return 1;
        }

        public void Cancel()
        {
            Cancelled = true;
        }

        public abstract void Launch();
    }
}

