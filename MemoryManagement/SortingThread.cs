using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MemoryManagement
{
	class SortingThread
	{
		private IntArray m_aArrayToSort; //array that requires sorting
		private Thread m_tThread; //the thread that runs the process
		private static int ID = 0;
		private static Mutex m_mCreationMutex = new Mutex(); //mutex for assigning thread names

		public SortingThread()
		{
			m_mCreationMutex.WaitOne();
			m_tThread = new Thread(Sort);
			m_tThread.Name = "T" + ID;
			ID++;
			m_mCreationMutex.ReleaseMutex();
		}

		//runs the thread
		public void Start()
		{
			m_tThread.Start();
		}

		//waits (sleeps) until the thread terminates
		public void Join()
		{
			m_tThread.Join();
		}

		public void DeleteArray()
		{
			MemoryManagementUnit.getInstance().Delete(m_aArrayToSort);
		}

		//Copies a section of the memory from another IntArray
		public void CopyFrom(IntArray a, int iStart, int iEnd)
		{
			int idx = 0;
			m_aArrayToSort = MemoryManagementUnit.getInstance().New(m_tThread, iEnd - iStart + 1);
			for (idx = iStart; idx <= iEnd; idx++)
				m_aArrayToSort[idx - iStart] = a[idx];
		}

		//Copies data from a regular array (to be called only from Main)
		public void CopyFrom(int[] a)
		{
			int idx = 0;
			m_aArrayToSort = MemoryManagementUnit.getInstance().New(m_tThread, a.Length);
			for (idx = 0; idx < a.Length; idx++)
				m_aArrayToSort[idx] = a[idx];
		}

		//Copies data into a regular array (to be called only from Main)
		public void CopyTo(int[] a)
		{
			int idx = 0;
			for (idx = 0; idx < a.Length; idx++)
				a[idx] = m_aArrayToSort[idx];
		}

		//returns the value of the array at a certain index
		public int ValueAt(int idx)
		{
			return m_aArrayToSort[idx];
		}

		//writes the current contents of the thread memory
		public override string ToString()
		{
			string sArray = "";
			int idx = 0;
			for (idx = 0; idx < m_aArrayToSort.Length; idx++)
				sArray += m_aArrayToSort[idx] + " ";
			return sArray;
		}

		//uses quick sort to sort an array
		public void Sort()
		{
			int iLength = m_aArrayToSort.Length; //more comfort to the eye

			if (iLength <= 1)
				return;

			Random random = new Random();
			int randomNumber = random.Next(iLength); //piking a random number between 0 and the length of the current array

			//pick a median
			//partition the array into numbers larger than median and numbers smaller than median
			int pivot = partition(m_aArrayToSort, 0, iLength - 1, randomNumber);


			//Create two SortingThreads
			SortingThread thread_one = new SortingThread();
			SortingThread thread_two = new SortingThread();

			//Copy the smaller part of the array into the first thread
			thread_one.CopyFrom(m_aArrayToSort, 0, pivot - 1);
			//Copy the larger part of the array into the second thread
			thread_two.CopyFrom(m_aArrayToSort, pivot + 1, iLength - 1);

			//Run the two threads and wait for them to terminate
			thread_one.Start();
			thread_two.Start();
			thread_one.Join();
			thread_two.Join();

			//Copy the two sorted arrays into the original array

			for (int i = 0; i < pivot; i++)
			{
				m_aArrayToSort[i] = thread_one.m_aArrayToSort[i];
			}

			int k = 0;
			for (int j = pivot + 1; j < iLength; j++)
			{
				m_aArrayToSort[j] = thread_two.m_aArrayToSort[k];
				k++;
			}


			//Delete the arrays
			thread_one.DeleteArray();
			thread_two.DeleteArray();


		}

		//algorithem from assignment
		private int partition(IntArray a, int left, int right, int pivotIndex)
		{
			int pivotValue, storeIndex;

			pivotValue = a[pivotIndex];
			swap(a, pivotIndex, right);

			storeIndex = left;

			for (int i = left; i < right; i++)
			{
				if (a[i] <= pivotValue)
				{
					swap(a, i, storeIndex);
					storeIndex++;
				}
			}
			swap(a, storeIndex, right);

			return storeIndex;
		}

		private void swap(IntArray curArr, int a, int b)
		{
			int temp;
			temp = curArr[a];
			curArr[a] = curArr[b];
			curArr[b] = temp;
		}
	}
}