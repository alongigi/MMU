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
		private MemoryBlock m_mbList;
		private List<MemoryBlock> m_lLru; //for the delete of memory in right order
		private Dictionary<string, MemoryBlock> m_dMemory;

		//singleton implementation
		public static void SetMemoryManagementUnitType(int cInts)
		{
			m_mmuInstance = new SwappingMemoryManagementUnit(cInts);
		}

		private SwappingMemoryManagementUnit(int cInts)
		{
			m_aMemory = new int[cInts];//this is the only "new" allowed in this class
			m_mbList = new MemoryBlock(0, cInts - 1, null, null); // in the start we have a bog hole.
			m_lLru = new List<MemoryBlock>();
			m_dMemory = new Dictionary<string, MemoryBlock>();
		}

		//Returns the allocated memory blocked that was not accessed for the longest time
		private MemoryBlock GetLeastRecentlyAccessedMemoryBlock()
		{
			MemoryBlock toSwap = m_lLru[0];
			m_lLru.RemoveAt(0); //remove the oldest item from the so called "queue"
			return toSwap;
		}

		//Merges holes before and after a free memory block
		private void MergeHoles(MemoryBlock mbFree)
		{
			if (mbFree.Next != null && mbFree.Next.Free())//check if we have a hole from right
			{
				mbFree.End = mbFree.Next.End;
				mbFree.Next = mbFree.Next.Next;

				if (mbFree.Next != null)
				{
					mbFree.Next.Previous = mbFree;
				}

				//if (mbFree.Next != null && mbFree.Next.Next != null)
				//    mbFree.Next.Next.Previous = mbFree;
			}

			if (mbFree.Previous != null && mbFree.Previous.Free()) //check if we have a hole from left
			{
				if (mbFree.Previous.Start == 0)
				{
					m_mbList = mbFree; // making sure that i kip the pointer  to the first node
				}

				mbFree.Start = mbFree.Previous.Start;
				mbFree.Previous = mbFree.Previous.Previous;

				if (mbFree.Previous != null)
				{
					mbFree.Previous.Next = mbFree;
				}

				//if (mbFree.Previous != null && mbFree.Previous.Previous != null)
				//    mbFree.Previous.Previous.Next = mbFree;
			}
		}

		//Returns the first available free block. 
		//If there isn't any sufficient hole, swap allocated blocks to the disk until a sufficient hole is created.
		private MemoryBlock GetFirstHole(int cInts)
		{
			MemoryBlock tempHole = m_mbList;

			while (tempHole != null)
			{
				if (tempHole.Free() && tempHole.Size() >= cInts)
				{
					return tempHole;
				}

				tempHole = tempHole.Next;
			}

			//there isn't any sufficient hole

			MemoryBlock toSwap = GetLeastRecentlyAccessedMemoryBlock(); // this is the oldest item in memory

			SwapOut(toSwap); //moving the oldest memory  to the disk. and checking again if, now, we have space


			return GetFirstHole(cInts);

		}

		public override IntArray New(Thread tOwner, int cInts)
		{
			m_mMutex.WaitOne();

			if (cInts > m_aMemory.Length)//change false to checking if there is no available memory (new array is larger then memory capacity)
				throw new OutOfMemoryException("Requested " + cInts + " units. Memory capacity " + m_aMemory.Length + " units.");

			if (cInts == 0)
			{
				m_mMutex.ReleaseMutex();
				return new IntArray(0, tOwner); //returning an empty array (nothing will happend)
			}

			MemoryBlock sufficientHole = GetFirstHole(cInts); //this hole have anough space for the current thread and size of int

			/* we need to check if  the hole is bigger then the current size so we will split it */
			if (sufficientHole.Size() != cInts) // hence, its smaller
			{
				/* creating a new MemoryBlock object left to the hole we found */
				MemoryBlock newMemomryBlock = new MemoryBlock(tOwner, sufficientHole.Start, sufficientHole.Start + cInts - 1, sufficientHole.Previous, sufficientHole);

				sufficientHole.Start = newMemomryBlock.End + 1;
				if (sufficientHole.Previous != null)
				{
					sufficientHole.Previous.Next = newMemomryBlock;
				}

				sufficientHole.Previous = newMemomryBlock;

				if (newMemomryBlock.Start == 0)
				{
					m_mbList = newMemomryBlock;
				}

				m_dMemory.Add(tOwner.Name, newMemomryBlock); // add to the dictionary the current memoryblock
				m_lLru.Add(newMemomryBlock); // entering it to the queue

			}
			else //cInt and hole size are exactly the same size
			{
				sufficientHole.Owner = tOwner;
				m_lLru.Add(sufficientHole); // entering it to the queue
				m_dMemory.Add(tOwner.Name, sufficientHole); // add to the dictionary the current memoryblock
			}



			m_mMutex.ReleaseMutex();


			return new IntArray(cInts, tOwner);
		}


		public override void Delete(IntArray aToDelete)
		{
			m_mMutex.WaitOne();

			string name = aToDelete.Owner.Name + ".data"; //the name of the file in the bin\debug folder

			if (m_dMemory.ContainsKey(aToDelete.Owner.Name)) //check if memoryBlock in the dictionary
			{
				m_lLru.Remove(m_dMemory[aToDelete.Owner.Name]); //remove the item from the lru
				m_dMemory[aToDelete.Owner.Name].Owner = null;  //the memory block is now a hole
				MergeHoles(m_dMemory[aToDelete.Owner.Name]);  // merging holes right and left to the new hole
				m_dMemory.Remove(aToDelete.Owner.Name); // remove from dictionary
			}

			File.Delete(name); //delete file from the disk if we have one
			m_mMutex.ReleaseMutex();
		}


		public override void SetValueAt(Thread tOwner, int iPrivateAddress, int iValue)
		{
			m_mMutex.WaitOne();

			if (!m_dMemory.ContainsKey(tOwner.Name))//if thread memory is not in the physical memory
			{
				SwapIn(tOwner);
			}
			else //thread memory is in the phsical memory need to update lru
			{
				m_lLru.Remove(m_dMemory[tOwner.Name]);
				m_lLru.Add(m_dMemory[tOwner.Name]);
			}

			int iStart = m_dMemory[tOwner.Name].Start;

			m_aMemory[iStart + iPrivateAddress] = iValue;

			m_mMutex.ReleaseMutex();
		}

		public override int ValueAt(Thread tOwner, int iPrivateAddress)
		{
			m_mMutex.WaitOne();
			if (!m_dMemory.ContainsKey(tOwner.Name))//if thread memory is not in the physical memory
			{
				SwapIn(tOwner);
			}
			else //thread memory is in the phsical memory need to update lru
			{
				m_lLru.Remove(m_dMemory[tOwner.Name]);
				m_lLru.Add(m_dMemory[tOwner.Name]);
			}

			int iStart = m_dMemory[tOwner.Name].Start; //the location of the 
			int iValue = m_aMemory[iStart + iPrivateAddress];

			m_mMutex.ReleaseMutex();

			return iValue;
		}



		private void SwapOut(MemoryBlock aOut)
		{
			Thread tOut = aOut.Owner;
			StreamWriter sw = new StreamWriter(tOut.Name + ".data");

			//write all relevant data to the file
			sw.Write(aOut.Size() + ", ");

			for (int i = aOut.Start; i <= aOut.End; i++) // running from start to end of current Node
			{
				sw.Write(m_aMemory[i]);
				if (i != aOut.End)
				{
					sw.Write(" ");
				}
			}

			sw.Close();
			aOut.Owner = null;
			MergeHoles(aOut); //merge holes right\left to "aOut"
			m_lLru.Remove(aOut); // remove from lru
			m_dMemory.Remove(tOut.Name); //remove from dictionary
		}

		private void SwapIn(Thread tIn)
		{
			StreamReader sr = new StreamReader(tIn.Name + ".data");
			char[] charToDEl = { ' ', ',' };

			//read relevant data from the file
			string[] fileString = sr.ReadToEnd().Split(charToDEl);


			int size = Int32.Parse(fileString[0]);

			//allocate the required memory using New
			IntArray newArr = New(tIn, size);

			sr.Close();

			//copy data to the allocated memory
			for (int i = 2; i < fileString.Length; i++)
			{
				// i is equal 2 because the first two places in the array are empty
				newArr[i - 2] = Int32.Parse(fileString[i]);
			}
		}

	}
}