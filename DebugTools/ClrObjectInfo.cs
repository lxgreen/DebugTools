using Microsoft.Diagnostics.Runtime;

namespace DebugTools
{
    public class ClrObjectInfo
    {
        private ClrType _type;
        private ulong _size;
        private ulong _address;
        private int _generation;

        public ClrObjectInfo(ulong address, ClrType type, int generation, ulong size)
        {
            // TODO: Complete member initialization
            _address = address;
            _type = type;
            _generation = generation;
            _size = size;
        }
    }
}
