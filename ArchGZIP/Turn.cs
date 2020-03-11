using System;
using System.Collections.Generic;
using System.Threading;

namespace ArchGZIP
{
    public class ByteBlock
    {
        private int id;
        private byte[] buffer;
        private byte[] compressedBuffer;

        public int ID { get { return id; } }
        public byte[] Buffer { get { return buffer; } }
        public byte[] CompressedBuffer { get { return compressedBuffer; } }


        public ByteBlock(int id, byte[] buffer) : this(id, buffer, new byte[0])
        {

        }

        public ByteBlock(int id, byte[] buffer, byte[] compressedBuffer)
        {
            this.id = id;
            this.buffer = buffer;
            this.compressedBuffer = compressedBuffer;
        }

    }

    public class QueueManager
    {
        private object locker = new object();
        Queue<ByteBlock> queue = new Queue<ByteBlock>();
        bool _isDead;
        private int _blockId;

        public void EnqueueForWriting(ByteBlock block)
        {
            int id = block.ID;
            lock (locker)
            {
                if (_isDead)
                    throw new InvalidOperationException("Очередь уже остановилась");

                while (id != _blockId)
                {
                    Monitor.Wait(locker);
                }

                queue.Enqueue(block);
                _blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public void EnqueueForCompressing(byte[] buffer)
        {
            lock (locker)
            {
                if (_isDead)
                    throw new InvalidOperationException("Очередь уже остановилась");

                ByteBlock _block = new ByteBlock(_blockId, buffer);
                queue.Enqueue(_block);
                _blockId++;
                Monitor.PulseAll(locker);
            }
        }


        public ByteBlock Dequeue()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !_isDead)
                    Monitor.Wait(locker);

                if (queue.Count == 0)
                    return null;

                return queue.Dequeue();

            }
        }

        public void Stop()
        {
            lock (locker)
            {
                _isDead = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}
