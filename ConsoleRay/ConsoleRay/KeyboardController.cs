using System;

namespace ConsoleRay
{
    public class KeyboardController
    {
        Keyboard keyboard = Keyboard.instance;

        bool wantGameEnd = false;

        public void GetInput()
        {
            if(keyboard.HasKey())
            {
                ConsoleKey? key = keyboard.GetKey();

                if(key == ConsoleKey.Escape) 
                {
                    wantGameEnd = true;
                }
            }
        }

        public bool IsEndGame()
        {
            return wantGameEnd;
        }

        public void Update()
        {
            GetInput();
        }
    }
}
