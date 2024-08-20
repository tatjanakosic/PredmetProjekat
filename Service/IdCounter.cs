using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Service
{
    public class IdCounter
    {
        public static int LoadCounter { get; set; }
        public static int AuditCounter { get; set;}

        public static void GetIDs()
        {
            LoadCounter = XMLload() +1;
            AuditCounter = XMLaudit() +1;
        }

        /// <summary>
        /// Returns max ID of Audit object from the list of forwarded Audit objects
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private static int GetMaxId(List<int> ids) 
        {
            int max = ids[0];
            for(int i = 1; i < ids.Count; i++)
            {
                if (max < ids[i])
                {
                    max = ids[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Returns max ID of retrieved Audit object from XML base
        /// </summary>
        /// <returns></returns>
        private static int XMLaudit() 
        {
            XmlDocument xmlDoc = new XmlDocument();
            string path = ConfigurationManager.AppSettings["AuditDatoteka"];
            xmlDoc.Load(path);
            List<int> ids = new List<int>();

            foreach(XmlNode node  in xmlDoc.DocumentElement)
            {
                int id = 0;
                try
                {
                    if (node.FirstChild != null)
                    {
                        id = int.Parse(node.FirstChild.InnerText);
                    }
                    //id = int.Parse(node.FirstChild.InnerText);
                }
                catch (Exception) 
                { 

                }
                ids.Add(id);
            }

            return GetMaxId(ids);
        }

        /// <summary>
        /// Returns max ID od retrieved Load objects from XML base
        /// </summary>
        /// <returns></returns>
        private static int XMLload() 
        {
            XmlDocument xmlDoc = new XmlDocument();
            string path = ConfigurationManager.AppSettings["LoadDatoteka"];
            xmlDoc.Load(path);
            List<int> ids = new List<int>();

            foreach (XmlNode node in xmlDoc.DocumentElement)
            {
                int id = 0;
                try
                {
                    if (node.FirstChild != null)
                    {
                        id = int.Parse(node.FirstChild.InnerText);
                    }
                    //id = int.Parse(node.FirstChild.InnerText);
                }
                catch (Exception)
                {

                }
                ids.Add(id);
            }
            return GetMaxId(ids);
        }
    }
}
