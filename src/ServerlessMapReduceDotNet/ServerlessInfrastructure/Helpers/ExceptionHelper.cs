using System;
using System.Diagnostics;

namespace ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers
{
    class ExceptionHelper
    {
        public static void LogExceptionAndContinue(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Demystify());
            }
        }
    }
}