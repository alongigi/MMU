using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MemoryManagement
{
    class IntArray
    {
        public int Length { get; private set; } //The length of the array
        public Thread Owner { get; private set; } //The thread that owns (created) this array
        private bool m_bInMemory;

        public IntArray(int iLength, Thread tOwner)
        {
            Length = iLength;
            Owner = tOwner;
            m_bInMemory = true;
        }

        public int this[int iPrivateAddress]
        {
            get
            {
                if (iPrivateAddress < 0 || iPrivateAddress >= Length)
                    throw new IndexOutOfRangeException("Index " + iPrivateAddress + " out of bound.");
                return MemoryManagementUnit.getInstance().ValueAt(Owner, iPrivateAddress);
            }
            set
            {
                if (iPrivateAddress < 0 || iPrivateAddress >= Length)
                    throw new IndexOutOfRangeException("Index " + iPrivateAddress + " out of bound.");
                MemoryManagementUnit.getInstance().SetValueAt(Owner, iPrivateAddress, value);
            }
        }
    }
}
