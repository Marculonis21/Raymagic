using System;

namespace ConsoleRay
{
    public class Keyboard
    {
        private Keyboard()
        {
        }

        public static readonly Keyboard instance = new Keyboard();

        public bool HasKey()
        {
            return Console.KeyAvailable;
        }

        public ConsoleKey? GetKey()
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey(true).Key;
            }
            return null;
        }
    }
}
