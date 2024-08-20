using Common;
using Common.Interfaces;
using Common.MethodProfiler;
using Common.ObjectTracker;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace Service
{
    [ServiceBehavior]
    public class ServiceController : IServiceController, IInMManipulation
    {
        private readonly ObjectTracker _objectTracker = new ObjectTracker();
        private static Profiler _profiler = new Profiler();

        /// <summary>
        /// Checks if object with corresponding input datetime exists (Load or Audit) in In-Memory base 
        /// </summary>
        /// <param name="time"></param>
        /// <returns>vraca true ako je pronadjen objekat sa istim datumom</returns>
        [OperationBehavior]
        public bool CheckInMemoryBase(DateTime time)
        {
            bool isHere = false;

            foreach (var obj in InMemoryBase.dbMemory.Values)
            {
                if (obj.GetType() == typeof(Load))
                {
                    try
                    {
                        Load load = (Load)obj;
                        if (load.Equals(time))
                        {
                            isHere = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nInvalid Load object, error: " + ex.Message);
                    }
                }
                if (obj.GetType() == typeof(Audit))
                {
                    try
                    {
                        Audit audit = (Audit)obj;
                        if (audit.Equals(time))
                            isHere |= true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nInvalid Neispravan Audit object, error: " + ex.Message);
                    }
                }
            }
            return isHere;
        }

        /// <summary>
        /// Prints all Load object from In-Memory base
        /// </summary>
        /// <returns>list</returns>
        [OperationBehavior]
        public List<Load> PrintLoad()
        {
            List<Load> list = new List<Load>(20);
            foreach (var obj in InMemoryBase.dbMemory.Values)
            {
                if (obj.GetType() == typeof(Load))
                {
                    try
                    {
                        Load load = (Load)obj;
                        list.Add(load);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nInvalid Load object, error: " + ex.Message);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Prints all Audit object from In-Memory base
        /// </summary>
        /// <returns>list</returns>
        [OperationBehavior]
        public List<Audit> PrintAudit()
        {
            List<Audit> list = new List<Audit>(10);
            foreach (var obj in InMemoryBase.dbMemory.Values)
            {
                if (obj.GetType() == typeof(Audit))
                {
                    try
                    {
                        Audit audit = (Audit)obj;
                        list.Add(audit);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nInvalid Audit object, error: " + ex.Message);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Accesses the databases and looks for object with forwarded datetime
        /// </summary>
        /// <param name="time"></param>
        /// <returns>list</returns>
        [OperationBehavior]
        public List<Load> PostRequest(DateTime time)
        {
            List<Load> list = new List<Load>(25);

            if (CheckInMemoryBase(time))
            {
                list = GetDataFromInMemory(time);
                Console.WriteLine("\nObject with forwarded datetime exists in In-memory base!");
            }
            else
            {
                XMLBase xmlBase = new XMLBase();
                list = xmlBase.ReadData(time);
                if (list.Count() != 0)
                {
                    Console.WriteLine("\nObject with forwarded datetime exists in XML base!");
                }
                else
                {
                    Console.WriteLine("\nObject with forwarded datetime does't exist in XML base!");
                }

            }
            return list;
        }

        /// <summary>
        /// Looks for Load object in In-Memory base with forwarded datetime
        /// </summary>
        /// <param name="time"></param>
        /// <returns>loads</returns>
        public List<Load> GetDataFromInMemory(DateTime time)
        {
            List<Load> loads = new List<Load>(25);
            foreach (var l in InMemoryBase.dbMemory.Values)
            {
                if (l.GetType() == typeof(Load))
                {
                    try
                    {
                        Load load = (Load)l;
                        if (load.Equals(time))
                            loads.Add(load);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nInvalid Load object in In-Memory base, error: " + ex.Message);
                    }
                }
            }
            return loads;
        }

        /// <summary>
        /// Deletes old data form In-Memory base when needed
        /// </summary>
        public static void RemoveOldData()
        {
            int dataTimeout = 1;

            try
            {
                dataTimeout = Int32.Parse(ConfigurationManager.AppSettings["DataTimeout"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while deleting old data, error: " + ex.Message);

                XMLBase xML = new XMLBase();
                xML.WriteAuditData(IdCounter.AuditCounter++, AuditType.Warning, DateTime.Now);
            }

            while (true)
            {
                List<int> toRemove;
                object tempObj = new object();

                // finding old objects
                FindToRemove(dataTimeout, out toRemove);
                RemoveFromInMemory(toRemove, tempObj);

                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        /// <summary>
        /// Looks for object in In-Memory which creation time is older than time set in configuration
        /// </summary>
        /// <param name="dataTimeout">parameter from app.config-a</param>
        /// <param name="toRemove"></param>
        private static void FindToRemove(int dataTimeout, out List<int> toRemove)
        {
            toRemove = new List<int>(25);
            foreach (var obj in InMemoryBase.dbMemory.Values)
            {
                Load load = new Load();
                if (obj.GetType() == typeof(Load))
                {
                    load = (Load)obj;
                    if (load.TimeStamp < DateTime.Now.AddMinutes(dataTimeout * -1))
                    {
                        toRemove.Add(load.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes objects from list of old objects
        /// </summary>
        /// <param name="toRemove"></param>
        /// <param name="tempObj"></param>
        private static void RemoveFromInMemory(List<int> toRemove, object tempObj)
        {
            foreach (int key in toRemove)
            {
                bool removed = InMemoryBase.dbMemory.TryRemove(key, out tempObj);
                if (removed)
                    Console.WriteLine($"Deleted old object {key}");
            }
        }

        /// <summary>
        /// Adds new Load object in In-Memory base 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time"></param>
        /// <param name="forecast"></param>
        /// <param name="measured"></param>
        /// <returns>true if the object is successfully added</returns>
        [OperationBehavior]
        public bool AddLoad(int id, DateTime time, double forecast, double measured)
        {
            if (id < 0)
            {
                id = IdCounter.LoadCounter++;
            }

            Load load = new Load(id, time, forecast, measured);

            try
            {
                if (load != null)
                    InMemoryBase.dbMemory.TryAdd(id, load);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while writing Load objects in In-Memory base!" + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Deletes Audit from In-Memoru based on forwarded datetime
        /// </summary>
        /// <param name="time"></param>
        /// <param name="temp"></param>
        /// <returns>true if the object is found and deleted</returns>

        [OperationBehavior]
        public bool RemoveAudit(DateTime time, object temp)
        {
            foreach (var obj in InMemoryBase.dbMemory.Values)
            {
                Audit audit = new Audit();
                if (obj.GetType() == typeof(Audit))
                {
                    try
                    {
                        audit = (Audit)obj;
                        if (audit.Equals(time))
                        {
                            return InMemoryBase.dbMemory.TryRemove(audit.ID, out temp);
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nError while deleting Audit object: " + ex.Message);
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Deletes all object from In-Memory base
        /// </summary>
        /// <returns></returns>
        public bool RemoveAllDataFromInMemory()
        {
            InMemoryBase.dbMemory.Clear();

            if(InMemoryBase.dbMemory.Count == 0)
            {
                return true;
            }

            return false;
        }

        [OperationBehavior]
        public void TrackObject(object obj)
        {
            _objectTracker.Track(obj);
        }

        public long GetLoadObjectCount()
        {
            return _objectTracker.GetLoadObjectCount();
        }

        public Dictionary<int, int> GetLoadObjectGenerations()
        {
            return _objectTracker.GetLoadObjectGenerations();
        }

        [OperationBehavior]
        public long GetMemoryUsage()
        {
            return _objectTracker.GetMemoryUsage();
        }

        [OperationBehavior]
        public Load AddLoadMany(DateTime time, double forecast, double measured, out List<MethodProfile> profilerData)
        {
            SampleEventSource.Log.MethodEnter(nameof(AddLoadMany));
            _profiler.StartProfiling(nameof(AddLoadMany));

            int id = IdCounter.LoadCounter++;
            Load load = new Load(id, time, forecast, measured);

            _objectTracker.Track(load);
            InMemoryBase.dbMemory.TryAdd(id, load);

            Thread.Sleep(100);
            _profiler.StopProfiling(nameof(AddLoadMany));
            SampleEventSource.Log.MethodExit(nameof(AddLoadMany));

            profilerData = _profiler.GetProfilesData();

            return load;
        }

        public void ExportDataToCsv(long totalMemory, long instanceCount, Dictionary<int, int> generations, bool includeHeader)
        {
            using (var writer = new StreamWriter("memory_profile.csv", true))
            {
                if (includeHeader)
                {
                    writer.WriteLine("TotalMemory,InstanceCount,InstanceIndex,Generation");
                }

                foreach (var entry in generations)
                {
                    writer.WriteLine($"{totalMemory},{instanceCount},{entry.Key},{entry.Value}");
                }
            }
        }
    }
}
