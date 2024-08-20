using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ObjectTracker
{
    public class ObjectTracker
    {
        private readonly List<WeakReference> TrackedObjects = new List<WeakReference>();

        public void Track(object obj)
        {
            TrackedObjects.Add(new WeakReference(obj));
        }

        public long GetLoadObjectCount()
        {
            CleanUp();
            return TrackedObjects.Count(wr => wr.IsAlive && wr.Target is Load);
        }

        public Dictionary<int, int> GetLoadObjectGenerations()
        {
            CleanUp();
            var generations = new Dictionary<int, int>();
            int index = 0;

            foreach (var wr in TrackedObjects)
            {
                if (wr.Target is Load loadObj)
                {
                    int generation = GC.GetGeneration(loadObj);
                    generations[index++] = generation;
                }
            }

            return generations;
        }

        public long GetMemoryUsage()
        {
            return GC.GetTotalMemory(false);    // Gets the total memory used by the managed heap, false - doesn't start GC
        }

        public void CleanUp()
        {
            TrackedObjects.RemoveAll(wr => !wr.IsAlive);
        }
    }
}
