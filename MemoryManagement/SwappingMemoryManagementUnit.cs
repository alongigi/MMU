using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace MemoryManagement
{
    class SwappingMemoryManagementUnit : MemoryManagementUnit
    {
        //you code here        
        private int[] m_aMemory;//physical memory

        //singleton implementation
        public static void SetMemoryManagementUnitType(int cInts)
        {
            m_mmuInstance = new SwappingMemoryManagementUnit(cInts);
        }

        private SwappingMemoryManagementUnit(int cInts)
        {
            m_aMemory = new int[cInts];//this is the only "new" allowed in this class
            //your code here
        }

        //Returns the allocated memory blocked that was not accessed for the longest time
        private MemoryBlock GetLeastRecentlyAccessedMemoryBlock()
        {
            //your code here
            throw new NotImplementedException();
        }

        //Merges holes before and after a free memory block
        private void MergeHoles(MemoryBlock mbFree)
        {
            //your code here
            throw new NotImplementedException();
        }

        //Returns the first available free block. 
        //If there isn't any sufficient hole, swap allocated blocks to the disk until a sufficient hole is created.
        private MemoryBlock GetFirstHole(int cInts)
        {
            //your code here
            throw new NotImplementedException();
        }

        public override IntArray New(Thread tOwner, int cInts)
        {
            //your code here
            if (false)//change false to checking if there is no available memory (new array is larger then memory capacity)
                throw new OutOfMemoryException("Requested " + cInts + " units. Memory capacity " + m_aMemory.Length + " units.");
            return new IntArray(cInts, tOwner);
        }

        public override void Delete(IntArray aToDelete)
        {
            //your code here
            throw new NotImplementedException();
        }

        public override void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue)
        {
            if (false)//if thread memory is not in the physical memory
                SwapIn(tOwner);
            int iStart = 0;//the location of the 
            m_aMemory[iStart + iPrivateAddress] = iValue;
        }

        public override int ValueAt(Thread tOwner, int iPrivateAddress)
        {
            if (false)//if thread memory is not in the physical memory
                SwapIn(tOwner);
            int iStart = 0;//the location of the memory chunk assigned to the thread
            int iValue = m_aMemory[iStart + iPrivateAddress];
            return iValue;
        }

        private void SwapOut(MemoryBlock aOut)
        {
            Thread tOut = aOut.Owner;
            StreamWriter sw = new StreamWriter(tOut.Name + ".data");
            //write all relevant data to the file
            sw.Close();
            aOut.Owner = null;
        }

        private void SwapIn(Thread tIn)
        {
            StreamReader sr = new StreamReader(tIn.Name + ".data");
            //read relevant data from the file
            //allocate the required memory using New
            //copy data to the allocated memory
            sr.Close();
        }
    }
}
