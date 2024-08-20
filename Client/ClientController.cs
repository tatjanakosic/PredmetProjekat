using Client.Interfaces;
using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Client
{
    public  class ClientController : IClientController
    {
        /// <summary>
        /// Gets input datetime in correct format
        /// </summary>
        /// <returns></returns>
        public DateTime GetInput() 
        {    
            do 
            {
                DateTime dateTime;
                string temp;
                Console.WriteLine("Enter datetime in format yyyy-mm-dd: ");
                temp = Console.ReadLine();
                
                if(VerifyDate(temp, out dateTime))
                {
                    return dateTime;
                }
                Console.WriteLine("Please enter datetime in required format!");
            } 
            while(true);
        }

        /// <summary>
        /// Check the datetime format
        /// </summary>
        /// <param name="dateString"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool VerifyDate(string dateString, out DateTime dateTime)
        {
            var dateValid = dateString.Split('-');
            if (dateValid.Length == 0 && 12 < int.Parse(dateValid[1]))
            {
                dateTime = DateTime.Now;
                return false;
            }

            return DateTime.TryParse(dateString, out dateTime);
        }

        /// <summary>
        /// Writes results from server on console 
        /// </summary>
        /// <param name="loads"></param>
         public void GetRequest(List<Load> loads)
         {
            if (loads.Count() != 0)
            {
                foreach (Load load in loads)
                {
                    Console.WriteLine(load.ToString());
                }
            }
         } 
    }
}
