using System;
using System.Configuration;
using System.ServiceModel;
using System.Threading;

namespace Service
{
    class Program
     {
        private static ManualResetEvent resetEvent = new ManualResetEvent(false);   // manual start the reset
        public delegate void DelRemoveOldData();

         static void Main(string[] args)
         {
            InMemoryBase inMemoryBase = new InMemoryBase();
            IdCounter.GetIDs();
            
            string path = ConfigurationManager.AppSettings["LoadDatoteka"];
            Console.WriteLine(path);
            
            using (ServiceHost host = new ServiceHost(typeof(ServiceController)))
            {
                host.Open();
                Console.WriteLine("Service is successfully started!");

                DelRemoveOldData removeOldData = ServiceController.RemoveOldData;
                Thread thread = new Thread(() => removeOldData.Invoke());
                thread.Start();
                
                resetEvent.Set();
                thread.Join();

                Console.ReadKey();
                host.Close();
                resetEvent.Dispose();  
            }
            
            inMemoryBase = null;
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
                
         }
     }
}
