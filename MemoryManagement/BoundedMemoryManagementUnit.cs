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
		private Dictionary<string, int> m_dMemory;
		public static int indexOfArray; //to know where i am in "m_aMemory"

		//Singleton implementation
		public static void SetMemoryManagementUnitType(int cInts)
		{
			m_mmuInstance = new BoundedMemoryManagementUnit(cInts);
		}

		private BoundedMemoryManagementUnit(int cInts)
		{
			m_aMemory = new int[cInts]; //this is the only allowed "new" in this class
			m_dMemory = new Dictionary<string, int>();
			indexOfArray = 0; //in the start we are at 0 location in the memory array
		}

		public override IntArray New(Thread tOwner, int cInts)
		{
			if (m_aMemory.Length - indexOfArray < cInts)//checking if there is no available memory
				throw new OutOfMemoryException("Cannot allocate " + cInts + " ints.");

			m_mMutex.WaitOne();

			m_dMemory[tOwner.Name] = indexOfArray;
			indexOfArray += cInts;

			m_mMutex.ReleaseMutex();

			return new IntArray(cInts, tOwner);
		}

		public override void Delete(IntArray aToDelete)
		{
			m_mMutex.WaitOne();
			m_dMemory.Remove(aToDelete.Owner.Name);
			m_mMutex.ReleaseMutex();
		}

		public override void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue)
		{
			m_mMutex.WaitOne();
			int iStart = m_dMemory[tOwner.Name]; //find the begining of the memory block assigned to this thread in the physical memory
			m_aMemory[iStart + iPrivateAddress] = iValue;
			m_mMutex.ReleaseMutex();
		}

		public override int ValueAt(Thread tOwner, int iPrivateAddress)
		{
			m_mMutex.WaitOne();
			int iStart = m_dMemory[tOwner.Name]; //find the begining of the memory block assigned to this thread in the physical memory
			int iValue = m_aMemory[iStart + iPrivateAddress];
			m_mMutex.ReleaseMutex();

			return iValue;
		}
	}
}