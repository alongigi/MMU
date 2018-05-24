using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace MemoryManagement
{
    abstract class MemoryManagementUnit
    {
        protected Mutex m_mMutex;

        //singleton implementation
        protected static MemoryManagementUnit m_mmuInstance = null;
        public static MemoryManagementUnit getInstance()
        {
            return m_mmuInstance;
        }

        public MemoryManagementUnit()
        {
            m_mMutex = new Mutex();
        }

        //Assigns memory to a thread
        public abstract IntArray New(Thread tOwner, int cInts);

        //Releases the array
        public abstract void Delete(IntArray aToDelete);

        //Sets the value of a thread memory cell at iPrivateAddress to iValue
        public abstract void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue);

        //Returns the value of a thread memory cell at iPrivateAddress
        public abstract int ValueAt(Thread tOwner, int iPrivateAddress);
    }
}
