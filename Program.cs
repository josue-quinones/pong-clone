﻿using System;

namespace Pong
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new PongGameLoop())
                game.Run();
        }
    }
}
