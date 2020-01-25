using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace JA
{
    class ThreadManager
    {
        /// <summary>
        /// Number of selected threads.
        /// </summary>
        private int noOfThreads;
        /// <summary>
        /// Info about lib selection.
        /// </summary>
        bool isCppChecked;
        /// <summary>
        /// Reference to algorithm model instance.
        /// </summary>
        EdgeDetection algorithm;
        /// <summary>
        /// Threads set.
        /// </summary>
        List<Thread> threads;

        public ThreadManager(int noOfThreads, bool isCppChecked, ref EdgeDetection algorithm)
        {
            this.noOfThreads = noOfThreads;
            this.isCppChecked = isCppChecked;
            this.algorithm = algorithm;
        }
        /// <summary>
        /// Creates single thread. Sets the begining and end of the bounds of processed array.
        /// </summary>
        /// <param name="begin"> Begining of the array.</param>
        /// <param name="end"> End of array.</param>
        private void CreateThread(int begin, int end)
        {
            if (isCppChecked)
            {
                var t = new Thread(() => algorithm.RunCppDll(begin, end));
                threads.Add(t);
            }
            else
            {
                var t = new Thread(() => algorithm.RunAsmDll(begin, end));
                threads.Add(t);
            }
        }
        /// <summary>
        /// Creates set of threads.
        /// </summary>
        public void CreateThreadsSet()
        {
            if (noOfThreads == 0)
                noOfThreads = 1;

            const int VECTOR_LENGTH = 16;
            int vectorsPerThread = algorithm.getNoOfVectors() / noOfThreads;
            threads = new List<Thread>();

            int threadStep = vectorsPerThread * VECTOR_LENGTH;
            int begin = 0;
            int end = 0;

            for (int i = 0; i < noOfThreads; ++i)
            {
                begin = end;
                end += threadStep;
                CreateThread(begin, end);
            }

        }

        /// <summary>
        /// Run threads set.
        /// </summary>
        public string RunThreads()
        {

            Stopwatch clock = new Stopwatch();

            clock.Start();

            foreach (Thread th in threads)
            {
                th.Start();
            }
            foreach (Thread th in threads)
            {
                th.Join();
            }

            clock.Stop();


            return clock.ElapsedMilliseconds.ToString();

        }
    }
}

