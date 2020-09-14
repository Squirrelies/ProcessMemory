# ProcessMemory
A library for reading and writing process memory.

## Usage
Reference ProcessMemory in your project.
Ideal using statements are...
```csharp
using ProcessMemory;
using static ProcessMemory.Extensions;
```

## Example
```csharp
using System;
using System.Diagnostics;
using ProcessMemory;
using static ProcessMemory.Extensions;

namespace ProcMemTester
{
    public static class Program
    {
        private static Process process = Process.GetProcessesByName("re3")[0];
        private static ProcessMemoryHandler processMemoryHandler = new ProcessMemoryHandler(process.Id);
        private static MultilevelPointer healthPointer = new MultilevelPointer(processMemoryHandler, IntPtr.Add(process.MainModule.BaseAddress, 0x08D89B90), 0x50, 0x20);
        public static int MaximumHealth => healthPointer.DerefInt(0x54);
        public static int CurrentHealth => healthPointer.DerefInt(0x58);

        public static void Main()
        {
            Console.WriteLine("HP: {0} / {1}", CurrentHealth, MaximumHealth);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
```
