using System;


namespace ArchGZip
{
    public static class ConsoleProgress
    {
        public static void ProgressBar(long current, long overall)

        {

            Console.CursorLeft = 0;
            Console.Write("[");
            Console.CursorLeft = 32;
            Console.Write("]");
            Console.CursorLeft = 1;
            float onechunk = 30.0f / overall;


            int position = 1;
            for (int i = 0; i < onechunk * current; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }


            for (int i = position;
                i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }


            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(current.ToString() + " of " + overall.ToString() + "    " + "Success" + "\n Press button Ctrl+C"); //blanks at the end remove any excess

        }



    }
}


