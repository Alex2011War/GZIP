using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using ArchGZip;

namespace ArchGZIP
{
    internal class Decompressor : GZip
    {
        int _counter;


        public Decompressor(string input, string output) : base(input, output)
        {


        }

        public override void Launch()
        {
            Console.WriteLine("Распаковка...\n");

            Thread _reader = new Thread(Read);
            _reader.Start();

            for (int i = 0; i < Threads; i++)
            {
                DoneEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Decompress, i);
            }

            Thread _writer = new Thread(Write);
            _writer.Start();

            WaitHandle.WaitAll(DoneEvents);

            if (!Cancelled)
            {
                Console.WriteLine("\n Распаковка успешно завершена");
                Success = true;

            }
        }

        private void Read()
        {
            try
            {
                using (FileStream _compressedFile = new FileStream(SourceFile, FileMode.Open))
                {
                    while (_compressedFile.Position < _compressedFile.Length)
                    {
                        byte[] lengthBuffer = new byte[8];//8
                        _compressedFile.Read(lengthBuffer, 0, lengthBuffer.Length);
                        int blockLength = BitConverter.ToInt32(lengthBuffer, 4);//4
                        byte[] compressedData = new byte[blockLength];
                        lengthBuffer.CopyTo(compressedData, 0);
                        _compressedFile.Read(compressedData, 8, blockLength - 8);
                        int _dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);//-4
                        byte[] lastBuffer = new byte[_dataSize];

                        ByteBlock _block = new ByteBlock(_counter, lastBuffer, compressedData);
                        QueueReader.EnqueueForWriting(_block);
                        _counter++;

                        ConsoleProgress.ProgressBar(_compressedFile.Position, _compressedFile.Length);

                    }
                    QueueReader.Stop();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Cancelled = true;
            }
        }

        private void Decompress(object i)
        {
            try
            {
                while (!Cancelled)
                {
                    ByteBlock _block = QueueReader.Dequeue();
                    if (_block == null)
                        return;

                    using (MemoryStream ms = new MemoryStream(_block.CompressedBuffer))
                    {
                        using (GZipStream _gz = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            _gz.Read(_block.Buffer, 0, _block.Buffer.Length);
                            byte[] decompressedData = _block.Buffer.ToArray();
                            ByteBlock block = new ByteBlock(_block.ID, decompressedData);
                            QueueWriter.EnqueueForWriting(block);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread number {0}. \n Error description: {1}", i, ex.Message);
                Cancelled = true;
            }
        }

        private void Write()
        {
            try
            {
                using (FileStream _decompressedFile = new FileStream(SourceFile.Remove(SourceFile.Length - 3), FileMode.Append))
                {
                    while (!Cancelled)
                    {
                        ByteBlock _block = QueueWriter.Dequeue();
                        if (_block == null)
                            return;

                        _decompressedFile.Write(_block.Buffer, 0, _block.Buffer.Length);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Cancelled = true;

            }
        }
    }


}
