using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MemoryManagement
{
    class MultipleArrayMemoryManagementUnit : MemoryManagementUnit
    {
        //Mapping every thread name to its private memory
        private Dictionary<string, int[]> m_dMemory;        

        //singleton implementation
        public static void SetMemoryManagementUnitType()
        {
            m_mmuInstance = new MultipleArrayMemoryManagementUnit();
        }

        private MultipleArrayMemoryManagementUnit()
        {
            m_dMemory = new Dictionary<string, int[]>();
        }

        public override IntArray New(Thread tOwner, int cInts)
        {
            m_mMutex.WaitOne();
            m_dMemory[tOwner.Name] = new int[cInts];
            m_mMutex.ReleaseMutex();
            return new IntArray(cInts, tOwner);
        }

        public override void Delete(IntArray aToDelete)
        {
            m_mMutex.WaitOne();
            m_dMemory.Remove( aToDelete.Owner.Name );
            m_mMutex.ReleaseMutex();
        }

        public override void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue)
        {
            m_mMutex.WaitOne();
            int[] a = m_dMemory[tOwner.Name];
            m_mMutex.ReleaseMutex();
            a[iPrivateAddress] = iValue;
        }

        public override int ValueAt(Thread tOwner, int iPrivateAddress)
        {
            m_mMutex.WaitOne();
            int[] a = m_dMemory[tOwner.Name];
            m_mMutex.ReleaseMutex();
            return a[iPrivateAddress];
        }
    }
}
