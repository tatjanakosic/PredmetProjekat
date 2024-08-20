using Common;
using Common.Interfaces;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public class CSVData : ICSVManipulation, IDisposable
    {
        public static string pathCSV = ConfigurationManager.AppSettings["CsvDatoteka"] + DateTime.Now.Ticks + ".csv";
        private string fileName = Path.GetFileName(pathCSV);
        private bool disposedValue;
        
        public static string PathCSV { get => pathCSV; set => pathCSV = value; }
        public string FileName { get => fileName; set => fileName = value; }  
        
        private FileStream stream = new FileStream(PathCSV, FileMode.Create);

        public string GetPathFullName()
        {
            return Path.GetFullPath(pathCSV);
        }

        public void LogCSV(List<Load> load)
        {
            using(StreamWriter streamWriter = new StreamWriter(stream))
            {
                if(load.Count > 0)
                {
                    streamWriter.WriteLine("TIME_STAMP,FORECAST_VALUE,MEASURED_VALUE");
                    foreach (var l in load)
                    {
                        streamWriter.WriteLine(l.CSVString());        
                    }
                }
            }
        }

        public List<Load> ReadCSV(string filePath)
        {
            var loadDataList = new List<Load>();
            using (var reader = new StreamReader(filePath))
            {
                string line;
                // Skip header line
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    var loadData = new Load
                    {
                        TimeStamp = DateTime.Parse(values[0]),
                        ForecastValue = double.Parse(values[1]),
                        MeasuredValue = double.Parse(values[2])
                    };
                    loadDataList.Add(loadData);
                }
            }
            return loadDataList;
        }

        // Shows the chart of forecast and measured values 
        public void ShowChart(List<Load> loadData)
        {
            var plotModel = new PlotModel { Title = "Load Data" };

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy-MM-dd HH:mm",
                Title = "Time",
                IntervalType = DateTimeIntervalType.Hours,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Values",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            plotModel.Axes.Add(dateAxis);
            plotModel.Axes.Add(valueAxis);

            var forecastSeries = new LineSeries { Title = "Forecast Value", MarkerType = MarkerType.Circle };
            var measuredSeries = new LineSeries { Title = "Measured Value", MarkerType = MarkerType.Square };

            foreach (var data in loadData)
            {
                forecastSeries.Points.Add(DateTimeAxis.CreateDataPoint(data.TimeStamp, data.ForecastValue));
                measuredSeries.Points.Add(DateTimeAxis.CreateDataPoint(data.TimeStamp, data.MeasuredValue));
            }

            plotModel.Series.Add(forecastSeries);
            plotModel.Series.Add(measuredSeries);

            var plotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill
            };

            var form = new Form
            {
                Text = "Load Data Chart",
                Width = 800,
                Height = 600
            };

            form.Controls.Add(plotView);
            Application.Run(form);
        }

        public void GenerateChart()
        {
            var loadData = ReadCSV(GetPathFullName());
            ShowChart(loadData);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stream.Dispose();
                }

                disposedValue = true;
            }
        }

        ~CSVData()
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
