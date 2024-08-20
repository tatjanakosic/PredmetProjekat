using Common;
using Common.Exceptions;
using Common.Interfaces;
using Common.MethodProfiler;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public class Program
    {
        ClientController client = new ClientController();
        private static Random random = new Random();

        [STAThread]
        static void Main(string[] args)
        {
            // charts settings
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ChannelFactory<IServiceController> channel = new ChannelFactory<IServiceController>("Service");
            Console.WriteLine("Client is ready!");
            Program program = new Program();

            bool running = true;

            while (running)
            {
                IServiceController serviceController = channel.CreateChannel();

                try
                {
                    running = program.Run(serviceController);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(2000);
            }
        }
        private bool Run(IServiceController serviceController)
        {
            int option = -1;
            while (option != 0)
            {
                Console.WriteLine("\n========================================================");
                Console.WriteLine("Chose an option:");
                Console.WriteLine("\t[1] Get data from database");
                Console.WriteLine("\t[2] Write Load from In-Memory database");
                Console.WriteLine("\t[3] Write Audit from In-Memory database");
                Console.WriteLine("\t[4] Add Load objects");
                Console.WriteLine("\t[5] Delete Audit objects from In-Memory database");
                Console.WriteLine("\t[6] Clean In-Memory database");
                Console.WriteLine("\t[7] Create multiple Load objects and view method statistics");           // added options
                Console.WriteLine("\t[8] Print information (for option 7)");                                  // added options
                Console.WriteLine("\t[0] Exit");
                Console.WriteLine("----------------------------------------------------------");

                Console.Write("Chose an option: ");
                option = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("========================================================\n");
                CheckOption(option, serviceController);
            }
            return false;
        }

        private void CheckOption(int option, IServiceController serviceController)
        {
            switch (option)
            {
                case 1:
                    {
                        try
                        { 
                            var requestDate = client.GetInput();
                            var loads = serviceController.PostRequest(requestDate);
                            client.GetRequest(loads);

                            if (loads.Count() != 0)
                            {
                                CSVData cSV = new CSVData();
                                cSV.LogCSV(loads);
                                cSV.GenerateChart();
                                Console.WriteLine("Successfully written in file " + cSV.FileName + " on location " + cSV.GetPathFullName());
                            }
                        }
                        catch (FaultException<InternalCommunicationException> ex)
                        {
                            Console.WriteLine(ex.Detail.Message);
                        }

                        break;
                    }

                case 2:
                    {
                        List<Load> list = serviceController.PrintLoad();
                        if (list.Count == 0)
                        {
                            Console.WriteLine("No Load objects in In-Memory base!");
                        }
                        else
                        {
                            foreach (Load l in list)
                            {
                                Console.WriteLine(l.ToString());
                            }
                        }

                        break;
                    }

                case 3:
                    {
                        List<Audit> list = serviceController.PrintAudit();
                        if (list.Count == 0)
                        {
                            Console.WriteLine("No Audit objects in In-Memory base!");
                        }
                        else
                        {
                            foreach (Audit a in list)
                            {
                                Console.WriteLine(a.ToString());
                            }
                        }

                        break;
                    }

                case 4:
                    {
                        Console.WriteLine("Enter ID: ");
                        int number = -1;
                        string input = Console.ReadLine();
                        if(input != null && input != "" && input != "\n") 
                        {
                            number = Convert.ToInt32(input);
                            // proba
                            number = -1;
                        }

                        Console.WriteLine("Enter TIME STAMP in format yyyy-mm-dd HH:MM ");
                        DateTime date = DateTime.Now;
                        input = Console.ReadLine();
                        if (input != null && input != "" && input != "\n")
                        {
                            date = DateTime.Parse(input);
                        }

                        Console.WriteLine("Enter FORECAST VALUE: ");
                        double forecast = double.Parse(Console.ReadLine());
                        Console.WriteLine("Enter MEASURED VALUE:");
                        double measured = double.Parse(Console.ReadLine());

                        if (serviceController.AddLoad(number, date, forecast, measured))
                        {
                            Console.WriteLine("\nLoad object is successfully added!");
                        }
                        else
                        {
                            Console.WriteLine("\nError while writing Load object in In-Memory base!");
                        }

                        break;
                    }

                case 5:
                    {
                        object tempObj = new object();
                        var dataTimeToRemove = client.GetInput();
                        if (serviceController.RemoveAudit(dataTimeToRemove, tempObj))
                        {
                            Console.WriteLine("\nSuccessfully deleted Audit object from In-Memory base!");
                        }
                        else
                        {
                            Console.WriteLine("\nError while deleting Audit object from In-Memory base with date " + dataTimeToRemove.ToString());
                        }

                        break;
                    }

                case 6:
                    {
                        if (serviceController.RemoveAllDataFromInMemory())
                        {
                            Console.WriteLine("\nAll In-Memory data is successfully deleted!");
                        }
                        else 
                        {
                            Console.WriteLine("\nError while deleting all data from In-Memory base!");
                        }

                        break;
                    }

                case 7:
                    {
                        List<MethodProfile> profilerData = null;
                        var performanceCounterManager = new PerformanceCounterManager();
                        Logger logger = new Logger("profiling.log");

                        Console.WriteLine("Enter how many Load object you want to add: ");
                        int numberOfObjects = int.Parse(Console.ReadLine());

                        for (int i = 0; i < numberOfObjects; i++)
                        {
                            DateTime timestamp = DateTime.Now;
                            double forecast = GenerateRandomDouble(4000, 7000);
                            double measured = GenerateRandomDouble(4000, 7000);

                            Load load = serviceController.AddLoadMany(timestamp, forecast, measured, out profilerData);

                            Console.WriteLine($"Current CPU Usage: {performanceCounterManager.GetCurrentCpuUsage()}%");

                            if (load != null)
                            {
                                Console.WriteLine($"\n Load object with ID: {load.Id} is successfully added!");
                            }
                            else
                            {
                                Console.WriteLine("\nError while creating and writing Load object in In-Memory base!");
                            }
                     
                        } 

                        if (profilerData != null && profilerData.Count > 0)
                        {
                            Console.WriteLine("Profiler Report:");
                            foreach (var profile in profilerData)
                            {
                                Console.WriteLine($"Method: {profile.MethodName}");
                                Console.WriteLine($"  Call Count: {profile.CallCount}");
                                Console.WriteLine($"  Total Execution Time: {profile.TotalExecutionTime} ms");
                                Console.WriteLine($"  Average Execution Time: {profile.TotalExecutionTime / profile.CallCount} ms");
                                Console.WriteLine($"  Memory Usage: {profile.MemoryUsage} bytes");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Profiler data is empty.");
                        }

                        logger.LogProfiles(profilerData);
                        
                        GC.Collect();                       // Triggers garbage collection
                        GC.WaitForPendingFinalizers();
                        GC.Collect();                       // Runs GC again to ensure all finalizers are executed

                        break;
                    }

                case 8:
                    {
                        try
                        {
                            long memoryUsage = serviceController.GetMemoryUsage();
                            long objectCount = serviceController.GetLoadObjectCount();
                            var generations = serviceController.GetLoadObjectGenerations();


                            Console.WriteLine($"Total number of Load objects: {objectCount}");
                            Console.WriteLine($"Memory usage: {memoryUsage} bytes");

                            
                            foreach (var generation in generations)
                            {
                                Console.WriteLine($"Load object {generation.Key} is in generation {generation.Value}");
                            }

                            serviceController.ExportDataToCsv(memoryUsage, objectCount, generations, true);
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\nFailed to retrieve tracking information: " + ex.Message);
                        }
                        break;
                    }

                case 0:
                    {
                        InMemoryBase.dbMemory.Clear();
                        return;
                    }

                default:
                    {
                        throw new Exception("\nNo option entered. Try again!");
                    }
            }
        }

        private static double GenerateRandomDouble(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }
    }
}
