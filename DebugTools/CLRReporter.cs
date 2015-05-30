using System.Collections.Generic;
using Microsoft.Diagnostics.Runtime;

namespace DebugTools
{
    public enum DebugMode
    {
        AttachToProcess,
        LoadDump
    }

    public class CLRReporter
    {
        private DataTarget _dataTarget;
        private ClrRuntime _runtime;

        private string _mscordacwks;

        public CLRReporter()
        {
            // TODO: Get mscordacwks from config
            _mscordacwks = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscordacwks.dll";
        }

        [Logged]
        public CLRReporter(int pid)
            : this()
        {
            // TODO: Get timeout from config
            var timeout = 5000U;
            _dataTarget = DataTarget.AttachToProcess(pid, timeout);
            _runtime = _dataTarget.CreateRuntime(_mscordacwks);
        }

        [Logged]
        public CLRReporter(string dumpFile)
            : this()
        {
            _dataTarget = DataTarget.LoadCrashDump(dumpFile, CrashDumpReader.ClrMD);
            _runtime = _dataTarget.CreateRuntime(_mscordacwks);
        }

        public IList<ClrStackFrame> CLRStack(ClrThread thread)
        {
            return thread.StackTrace;
        }

        // DumpArray

        // DumpAssembly

        // DumpClass

        [Logged]
        public IList<ClrAppDomain> DumpDomain()
        {
            return _runtime.AppDomains;
        }

        public IEnumerable<ClrMemoryRegion> EEHeap()
        {
            return _runtime.EnumerateMemoryRegions();
        }

        public ClrHeap GetHeap()
        {
            return _runtime.GetHeap();
        }

        public IEnumerable<ClrObjectInfo> DumpHeap()
        {
            var heap = GetHeap();

            if (!heap.CanWalkHeap)
            {
                return null;
            }

            List<ClrObjectInfo> dump = new List<ClrObjectInfo>();

            foreach (var segment in heap.Segments)
            {
                for (ulong address = segment.FirstObject; address != 0; address = segment.NextObject(address))
                {
                    var type = heap.GetObjectType(address);

                    // If heap corruption, continue past this object.
                    if (type == null) { continue; }

                    var objInfo = new ClrObjectInfo(address, type, segment.GetGeneration(address), type.GetSize(address));

                    dump.Add(objInfo);
                }
            }

            return dump;
        }

        private static void ObjSize(ClrHeap heap, ulong obj, out uint count, out ulong size)
        {
            // Evaluation stack
            Stack<ulong> evaluationStack = new Stack<ulong>();

            // To make sure we don't count the same object twice, we'll keep a set of all objects
            // we've seen before.  Note the ObjectSet here is basically just "HashSet<ulong>".
            // However, HashSet<ulong> is *extremely* memory inefficient.  So we use our own to
            // avoid OOMs.

            Dictionary<ulong, bool> considered = new Dictionary<ulong, bool>();

            count = 0;
            size = 0;
            evaluationStack.Push(obj);

            while (evaluationStack.Count > 0)
            {
                // Pop an object, ignore it if we've seen it before.
                obj = evaluationStack.Pop();
                if (considered.ContainsKey(obj)) { continue; }

                considered.Add(obj, false);

                // Grab the type. We will only get null here in the case of heap corruption.
                ClrType type = heap.GetObjectType(obj);
                if (type == null)
                {
                    continue;
                }

                count++;
                size += type.GetSize(obj);

                // Now enumerate all objects that this object points to, add them to the
                // evaluation stack if we haven't seen them before.
                type.EnumerateRefsOfObject(obj, delegate(ulong child, int offset)
                {
                    if (child != 0 && !considered.ContainsKey(child))
                        evaluationStack.Push(child);
                });
            }
        }

        // DumpIL

        // DumpMD

        // DumpMT

        // DumpModule

        // DumpObject

        // DumpStackObjects

        public IEnumerable<ulong> FinalizeQueue()
        {
            return _runtime.EnumerateFinalizerQueue();
        }

        public IEnumerable<ClrHandle> GCHandles()
        {
            return _runtime.EnumerateHandles();
        }

        // GCHandleLeaks

        // GCInfo

        // GCRoot

        // ObjSize

        // PrintException

        // SyncBlk

        [Logged]
        public IList<ClrThread> Threads()
        {
            return _runtime.Threads;
        }

        // TraverseHeap

        // VerifyHeap

        // VerifyObj

        // VMMap

        // VMStat
    }
}
