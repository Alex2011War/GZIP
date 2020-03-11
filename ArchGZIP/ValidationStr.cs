using System;
using System.IO;

namespace ArchGZIP
{
    class ValidationStr
    {
        public static void StringReadValidation(string[] args)
        {

            if (args.Length == 0 || args.Length > 3)
            {
                throw new Exception("Пожалуйста, введите аргументы в следующем порядке:\n сжатие (распаковка) [исходный файл] [конечный файл].");
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                throw new Exception("Первый аргумент должен быть сжать или распаковать");
            }

            if (args[1].Length == 0)
            {
                throw new Exception("Имя исходного файла не указано.");
            }

            if (!File.Exists(args[1]))
            {
                throw new Exception("Исходный файл не найден.");
            }

            FileInfo _fileIn = new FileInfo(args[1]);
            FileInfo _fileOut = new FileInfo(args[2]);

            if (args[1] == args[2])
            {
                throw new Exception("Исходные и конечные файлы должны отличаться.");
            }

            if (_fileIn.Extension == ".gz" && args[0] == "compress")
            {
                throw new Exception("Файл уже был сжат.");
            }

            if (_fileOut.Extension == ".gz" && _fileOut.Exists)
            {
                throw new Exception("Файл назначения уже существует. Пожалуйста, укажите другое имя файла");
            }

            if (args[2].Length == 0)
            {
                throw new Exception("Имя файла назначения не было указано.");
            }
        }


    }
}