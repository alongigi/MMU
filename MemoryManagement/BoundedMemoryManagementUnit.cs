using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace MemoryManagement
{
    class BoundedMemoryManagementUnit : MemoryManagementUnit
    {
        //your code here
        private int[] m_aMemory; //physical memory

        //Singleton implementation
        public static void SetMemoryManagementUnitType(int cInts)
        {
            m_mmuInstance = new BoundedMemoryManagementUnit(cInts);
        }

        private BoundedMemoryManagementUnit(int cInts)
        {
            m_aMemory = new int[cInts];//this is the only allowed "new" in this class
            //your code here
        }

        public override IntArray New(Thread tOwner, int cInts)
        {
            //your code here
            if ( false )//change false to checking if there is no available memory
                throw new OutOfMemoryException("Cannot allocate " + cInts + " ints.");
            return new IntArray(cInts, tOwner);
        }

        public override void Delete(IntArray aToDelete)
        {
            //your code here
        }

        public override void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue)
        {
            int iStart = 0;//find the begining of the memory block assigned to this thread in the physical memory
            m_aMemory[iStart + iPrivateAddress] = iValue;
        }

        public override int ValueAt(Thread tOwner, int iPrivateAddress)
        {
            int iStart = 0;//find the begining of the memory block assigned to this thread in the physical memory
            int iValue = m_aMemory[iStart + iPrivateAddress];
            return iValue;
        }
    }
}
