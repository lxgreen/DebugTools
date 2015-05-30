using DebugTools;

namespace DebugToolsTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CLRReporter reporter = new CLRReporter(@"D:\Test2.11.exe.dmp");
            //reporter.CLRStack();
        }
    }
}
