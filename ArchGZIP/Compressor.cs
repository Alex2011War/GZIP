using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using ArchGZip;

namespace ArchGZIP
{
    class Compressor : GZip
    {
        public Compressor(string input, string output) : base(input, output)
        {

        }

        public override void Launch()
        {
            Console.WriteLine("Сжатие...\n");

            Thread _reader = new Thread(Read);
            _reader.Start();

            for (int i = 0; i < Threads; i++)
            {
                DoneEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(Compress, i);
            }

            Thread _writer = new Thread(Write);
            _writer.Start();

            WaitHandle.WaitAll(DoneEvents);

            if (!Cancelled)
            {
                Console.WriteLine("\nСжатие успешно завершено");
                Success = true;
            }
        }

        private void Read()
        {
            try
            {

                using (FileStream _fileToBeCompressed = new FileStream(SourceFile, FileMode.Open))
                {

                    int bytesRead;
                    byte[] lastBuffer;

                    while (_fileToBeCompressed.Position < _fileToBeCompressed.Length && !Cancelled)
                    {
                        if (_fileToBeCompressed.Length - _fileToBeCompressed.Position <= BlockSize)
                        {
                            bytesRead = (int)(_fileToBeCompressed.Length - _fileToBeCompressed.Position);
                        }

                        else
                        {
                            bytesRead = BlockSize;
                        }

                        lastBuffer = new byte[bytesRead];
                        _fileToBeCompressed.Read(lastBuffer, 0, bytesRead);
                        QueueReader.EnqueueForCompressing(lastBuffer);
                        ConsoleProgress.ProgressBar(_fileToBeCompressed.Position, _fileToBeCompressed.Length);
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

        private void Compress(object i)
        {
            try
            {
                while (!Cancelled)
                {
                    ByteBlock _block = QueueReader.Dequeue();

                    if (_block == null)
                        return;

                    using (MemoryStream _memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                        {

                            cs.Write(_block.Buffer, 0, _block.Buffer.Length);
                        }


                        byte[] compressedData = _memoryStream.ToArray();
                        ByteBlock _out = new ByteBlock(_block.ID, compressedData);
                        QueueWriter.EnqueueForWriting(_out);
                    }
                    WaitHandle doneEvent = DoneEvents[(int)i];
                    doneEvent.Dispose();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка в номере потока {0}. \n описание ошибки: {1}", i, ex.Message);
                Cancelled = true;
            }

        }

        private void Write()
        {
            try
            {
                using (FileStream _fileCompressed = new FileStream(path: DestinationFile + ".gz", mode: FileMode.Append))
                {
                    while (!Cancelled)
                    {
                        ByteBlock _block = QueueWriter.Dequeue();
                        if (_block == null)
                            return;

                        BitConverter.GetBytes(value: _block.Buffer.Length).CopyTo(array: _block.Buffer, index: 4);
                        _fileCompressed.Write(array: _block.Buffer, offset: 0, count: _block.Buffer.Length);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(value: ex.Message);
                Cancelled = true;
            }

        }
    }
}
