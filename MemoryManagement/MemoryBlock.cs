using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MemoryManagement
{

    class MemoryBlock
    {
        public Thread Owner;
        public MemoryBlock Next;
        public MemoryBlock Previous;
        public int Start;
        public int End;

        public MemoryBlock(Thread tOwner, int iStart, int iEnd, MemoryBlock aPrevious, MemoryBlock aNext)
            : this(iStart, iEnd, aPrevious, aNext)
        {
            Owner = tOwner;
        }
        public MemoryBlock(int iStart, int iEnd, MemoryBlock aPrevious, MemoryBlock aNext)
        {
            Start = iStart;
            End = iEnd;
            Previous = aPrevious;
            Next = aNext;
            Owner = null;
        }

        public int Size()
        {
            return End - Start + 1;
        }
        public override string ToString()
        {
            string s = "MemoryBlock [" + Start + "," + End + "]";
            if (Owner != null)
                s += " allocated to " + Owner.Name;
            return s;
        }

        public bool Free()
        {
            return Owner == null;
        }
    }
}