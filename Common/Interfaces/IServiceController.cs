using Common;
using Common.Exceptions;
using Common.MethodProfiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
   [ServiceContract]
   public interface IServiceController
   {
        [OperationContract]
        [FaultContract(typeof(InternalCommunicationException))]
        List<Load> PostRequest(DateTime time);

        [OperationContract]
        List<Load> PrintLoad();

        [OperationContract]
        List<Audit> PrintAudit();

        [OperationContract]
        bool AddLoad(int id, DateTime time, double forecast, double measured);

        [OperationContract]
        bool RemoveAudit(DateTime time, object temp);

        [OperationContract]
        bool RemoveAllDataFromInMemory();


        // new methods
        [OperationContract]
        Load AddLoadMany(DateTime time, double forecast, double measured, out List<MethodProfile> profilerData);

        [OperationContract]
        void TrackObject(object obj);

        [OperationContract]
        long GetLoadObjectCount();

        [OperationContract]
        Dictionary<int, int> GetLoadObjectGenerations();

        [OperationContract]
        long GetMemoryUsage();

        [OperationContract]
        void ExportDataToCsv(long totalMemory, long instanceCount, Dictionary<int, int> generations, bool includeHeader);

   }
}
