using System;
using System.IO;
using System.Runtime.CompilerServices;
using ManagedApp;

public static class Program
{

    public static void Main()
    {
        using var counter = Setup();

        while (true)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            counter.Increment();
        }

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Counter Setup()
    {
        var log = Console.Out;

        log.WriteLine($"Current directory = {Directory.GetCurrentDirectory()}");

        return new Counter
        {
            OnChanged = value => { log.WriteLine($"Counter = {value}"); }
        };
    }
}
