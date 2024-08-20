using Common;
using Common.Exceptions;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Xml;

namespace Service
{
    public class XMLBase : IXMLBase, IDisposable
    {
        private bool disposedValue;
        private List<Load> loadList = new List<Load>();

        private XmlDocument doc = new XmlDocument();
        private XmlDocument xmlDoc = new XmlDocument();

        /// <summary>
        /// Reads Load objects based on datetime from XML base and if they exist returns list of objects
        /// vraca listu tih objekata
        /// </summary>
        /// <param name="dateInput"></param>
        /// <returns></returns>
        public List<Load> ReadData(DateTime dateInput)
        {
            string path = ConfigurationManager.AppSettings["LoadDatoteka"];
            doc.Load(path);

            bool isHere = false;

            foreach (XmlNode node in doc.DocumentElement)
            {
                Load load = null;
                try
                {
                    int id = int.Parse(node.FirstChild.InnerText);
                    DateTime time = DateTime.Parse(node.FirstChild.NextSibling.InnerText);
                    double forecast_value = double.Parse(node.FirstChild.NextSibling.NextSibling.InnerText);
                    double measured_value = double.Parse(node.LastChild.InnerText);
                    load = new Load(id, time, forecast_value, measured_value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError while retrieving Load object from XML base: " + ex.Message);
                }

                if (CheckXMLBase(load, dateInput))
                {
                    loadList.Add(load);
                    try
                    {
                        WriteLoadInMemory(load);
                    }
                    catch (FaultException<InternalCommunicationException> ex)
                    {
                        Console.WriteLine("\nError while writing Load objects in In-Memory base: " + ex.Detail.Message);
                    }
                    isHere = true;
                }
            }
            if (isHere)
            {
                try
                {
                    WriteAuditData(IdCounter.AuditCounter++, AuditType.Info, dateInput);
                }
                catch (FaultException<InternalCommunicationException> ex)
                {
                    Console.WriteLine("\nError while writitng Audit objects: " + ex.Detail.Message);
                }
            }
            else
            {
                try
                {
                    WriteAuditData(IdCounter.AuditCounter++, AuditType.Error, dateInput);
                }
                catch (FaultException<InternalCommunicationException> ex)
                {
                    Console.WriteLine("\nError while writitng Audit objects: " + ex.Detail.Message);
                }
            }

            return loadList;
        }

        /// <summary>
        /// Checks datetime of existing Load object in XML base and compares it with forwarded datetime
        /// i poredi ga sa prosledjenim datumom.
        /// </summary>
        /// <param name="load"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckXMLBase(Load load, DateTime time)
        {
            if (load == null)
                return false;
            return (load.Equals(time)) ? true : false;
        }

        /// <summary>
        /// Writes read Load object from XML base in In-Memory base
        /// </summary>
        /// <param name="load"></param>
        /// <returns></returns>
        /// <exception cref="FaultException{InternalCommunicationException}"></exception>
        public bool WriteLoadInMemory(Load load)
        {
            if (load == null)
            {
                InternalCommunicationException exception =
                    new InternalCommunicationException("\nError while writing Load object in In-Memory base!");

                throw new FaultException<InternalCommunicationException>(exception);
            }
            return InMemoryBase.dbMemory.TryAdd(load.Id, load);
        }

        /// <summary>
        /// Creates new Audit object and writes in both databases
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="dateInput"></param>
        /// <exception cref="FaultException{InternalCommunicationException}"></exception>
        public void WriteAuditData(int id, AuditType type, DateTime dateInput)
        {
            string message = "";
            DateTime time = DateTime.Now;

            if (type == AuditType.Info)
            {
                message = "Data is read and forwarded";
            }
            else if (type == AuditType.Error)
            {
                message = "No data in database with datetime " + dateInput.Date.ToShortDateString();
            }
            else
            {
                message = "Error while executing " + dateInput.Date.ToShortDateString();
                InternalCommunicationException izuzetak =
                    new InternalCommunicationException("Error while handling Audit XML database!");

                throw new FaultException<InternalCommunicationException>(izuzetak);
            }
            var audit = new Audit(id, time, type, message);
            InMemoryBase.dbMemory.TryAdd(id, audit);

            string path = ConfigurationManager.AppSettings["AuditDatoteka"];

            LogAudit(path, audit);

        }

        /// <summary>
        /// Writes Audit object in XML base on forwarded path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="audit"></param>
        private void LogAudit(string path, Audit audit)
        {
            xmlDoc.Load(path);
            XmlElement element = xmlDoc.CreateElement("row");
            try
            {
                XmlElement Id = xmlDoc.CreateElement("ID");
                Id.InnerText = audit.ID.ToString();
                element.AppendChild(Id);

                XmlElement timestamp = xmlDoc.CreateElement("TIME_STAMP");
                timestamp.InnerText = audit.TimeStamp.ToString();
                element.AppendChild(timestamp);

                XmlElement typem = xmlDoc.CreateElement("MESSAGE_TYPE");
                typem.InnerText = audit.Type.ToString();
                element.AppendChild(typem);

                XmlElement msg = xmlDoc.CreateElement("MESSAGE");
                msg.InnerText = audit.Message;
                element.AppendChild(msg);

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError while writing Audit object in XML base: " + ex.Message);
            }
            xmlDoc.DocumentElement.AppendChild(element);
            xmlDoc.Save(path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    doc.RemoveAll();
                    doc = null;
                    xmlDoc.RemoveAll();
                    xmlDoc = null;
                }
                disposedValue = true;
            }
        }
        ~XMLBase()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
