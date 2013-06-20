//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
using System;

namespace TouchThumbsticks
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TouchThumbsticksGame game = new TouchThumbsticksGame())
            {
                game.Run();
            }
        }
    }
#endif
}

