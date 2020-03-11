using System;

namespace ArchGZIP
{
    class Program
    {
        static GZip _zipper;

        static int Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            Console.CancelKeyPress += CancelKeyPress;

            ShowInfo();

            try
            {

                args = new string[3];
                args[0] = Convert.ToString(Console.ReadLine());
                args[1] = Convert.ToString(Console.ReadLine());
                args[2] = Convert.ToString(Console.ReadLine());

                ValidationStr.StringReadValidation(args);

                switch (args[0].ToLower())
                {
                    case "compress":
                        _zipper = new Compressor(args[1], args[2]);
                        break;
                    case "decompress":
                        _zipper = new Decompressor(args[1], args[2]);
                        break;
                }

                _zipper.Launch();
                return _zipper.CallBackResult();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description {1}", ex.TargetSite, ex.Message);
                return 1;
            }
        }

        static void ShowInfo()
        {
            Console.WriteLine("Для архивации или распаковки файлов, пожалуйста, выполните следующие действия, чтобы ввести:\n" +
                              " GZipTest.exe de/compress [путь к исходному файлу] + [путь к файлу назначения]\n" +
                              "Чтобы завершить программу правильно, пожалуйста, используйте комбинацию CTRL + C");
        }


        static void CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\n ...");
                args.Cancel = true;
                _zipper.Cancel();

            }
        }
    }
}

