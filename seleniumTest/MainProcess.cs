using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ScraperBase
{
    class MainProcess
    {
        public Form MyForm { get; set; }
        public RichTextBox MyRichTextBox { get; set; }
        public string FolderName { get; set; }
        public string XMLFolderName { get; set; }

        private const string ACCESS_URL = "https://tidesandcurrents.noaa.gov/tide_predictions.html";

        private IWebDriver driver;

        public void Run()
        {
            Log($"Starting scraping job on thread ID {Thread.CurrentThread.ManagedThreadId}");

            var options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", XMLFolderName);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            driver = new ChromeDriver(options);

            Log($"Trying to navigate to NOAA URL on thread ID: {Thread.CurrentThread.ManagedThreadId}");

            FillData();
        }

        private void FillData()
        {
            driver.Url = ACCESS_URL;

            driver.Navigate();

            List<KeyValuePair<string, string>> statesHrefs = new List<KeyValuePair<string, string>>();

            var statesLinks = driver.FindElements(By.XPath("//table[@class='table']/tbody/tr/td/a"));

            foreach (var cityLink in statesLinks)
            {
                statesHrefs.Add(new KeyValuePair<string, string>(cityLink.Text, cityLink.GetAttribute("href")));
                break;
            }

            Log($"Found {statesHrefs.Count} states");

            List<Station> stations = NavigateStations(statesHrefs);

            ProcessStation(stations);

            driver.Quit();
            Log("Process finished on thread ID " + Thread.CurrentThread.ManagedThreadId);
        }

        private List<Station> NavigateStations(List<KeyValuePair<string, string>> statesHrefs)
        {
            List<Station> stations = new List<Station>();

            try
            {
                foreach (var href in statesHrefs)
                {
                    Log($"Navigating to {href.Key} state");

                    driver.Url = href.Value;
                    driver.Navigate();

                    //Get only TDs with info
                    var stationData = driver.FindElements(By.XPath("//table[@class='table']/tbody/tr[descendant::td[@class='stationname']]"));
                    int counter = 1;

                    Log($"Found {stationData.Count} stations in {href.Key}");

                    foreach (var stationTr in stationData)
                    {
                        Station st = new Station();

                        st.Id = stationTr.FindElement(By.ClassName("stationid")).Text;
                        st.State = href.Key;
                        st.Name = stationTr.FindElement(By.ClassName("stationname")).FindElement(By.TagName("a")).Text;
                        st.Lat = stationTr.FindElement(By.ClassName("latitude")).Text;
                        st.Lon = stationTr.FindElement(By.ClassName("longitude")).Text;
                        st.Predictions = stationTr.FindElement(By.ClassName("pred_type")).Text;
                        st.Href = stationTr.FindElement(By.ClassName("stationname")).FindElement(By.TagName("a")).GetAttribute("href");

                        stations.Add(st);

                        Log($"Processed {st.State}'s station {counter++}/{stationData.Count}");

                        if (counter >= 20)
                            break;
                    }


                    FileInfo file = new FileInfo(Path.Combine(FolderName, $"{href.Key}.xlsx"));

                    using (var package = new ExcelPackage(file))
                    {
                        package.Workbook.Worksheets.Add(href.Key);
                        var sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["A1"].Value = "Id";
                        sheet.Cells["B1"].Value = "Name";
                        sheet.Cells["C1"].Value = "Lat";
                        sheet.Cells["D1"].Value = "Lon";
                        sheet.Cells["E1"].Value = "Predictions";

                        int rowCounter = 2;

                        foreach (var station in stations)
                        {
                            int columnCounter = 1;

                            sheet.Cells[rowCounter, columnCounter++].Value = station.Id;
                            sheet.Cells[rowCounter, columnCounter++].Value = station.Name;
                            sheet.Cells[rowCounter, columnCounter++].Value = station.Lat;
                            sheet.Cells[rowCounter, columnCounter++].Value = station.Lon;
                            sheet.Cells[rowCounter, columnCounter++].Value = station.Predictions;

                            rowCounter++;
                        }

                        package.Save();

                    }

                    break;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return stations;
        }

        private void ProcessStation(List<Station> stations)
        {
            try
            {
                foreach (var station in stations)
                {
                    Log($"Navigating to {station.Name} station");

                    driver.Url = station.Href;
                    driver.Navigate();

                    var xml = driver.FindElement(By.XPath("//input[@value='Annual XML']"));
                    xml.Click();

                    //Throttle to wait for download
                    Thread.Sleep(5000);

                    XmlSerializer serializer = new XmlSerializer(typeof(Datainfo));

                    using (var sr = new StreamReader(Path.Combine(XMLFolderName, $"{station.Id}_annual.xml")))
                    {
                        Datainfo data = (Datainfo)serializer.Deserialize(sr);
                        FileInfo file = new FileInfo(Path.Combine(FolderName, $"{station.State} - {station.Name}.xlsx"));

                        using (var package = new ExcelPackage(file))
                        {
                            package.Workbook.Worksheets.Add(station.Name);
                            var sheet = package.Workbook.Worksheets[1];

                            sheet.Cells["A1"].Value = "Origin";
                            sheet.Cells["B1"].Value = "Product Type";
                            sheet.Cells["C1"].Value = "Station name";
                            sheet.Cells["D1"].Value = "State";
                            sheet.Cells["E1"].Value = "Station Id";
                            sheet.Cells["F1"].Value = "Station Type";
                            sheet.Cells["G1"].Value = "Referenced To Station Name";
                            sheet.Cells["H1"].Value = "Referenced To Station Id";
                            sheet.Cells["I1"].Value = "Height Offset Low";
                            sheet.Cells["J1"].Value = "Height Offset High";
                            sheet.Cells["K1"].Value = "Time Offset Low";
                            sheet.Cells["L1"].Value = "Time Offset High";
                            sheet.Cells["M1"].Value = "Begin Date";
                            sheet.Cells["N1"].Value = "End Date";
                            sheet.Cells["O1"].Value = "Data Units";
                            sheet.Cells["P1"].Value = "Time  Zone";
                            sheet.Cells["Q1"].Value = "Datum";
                            sheet.Cells["R1"].Value = "Interval Type";

                            sheet.Cells["A2"].Value = data.Origin;
                            sheet.Cells["B2"].Value = data.Producttype;
                            sheet.Cells["C2"].Value = data.Stationname;
                            sheet.Cells["D2"].Value = data.State;
                            sheet.Cells["E2"].Value = data.Stationid;
                            sheet.Cells["F2"].Value = data.Stationtype;
                            sheet.Cells["G2"].Value = data.ReferencedToStationName;
                            sheet.Cells["H2"].Value = data.ReferencedToStationId;
                            sheet.Cells["I2"].Value = data.HeightOffsetLow;
                            sheet.Cells["J2"].Value = data.HeightOffsetHigh;
                            sheet.Cells["K2"].Value = data.TimeOffsetLow;
                            sheet.Cells["L2"].Value = data.TimeOffsetHigh;
                            sheet.Cells["M2"].Value = data.BeginDate;
                            sheet.Cells["N2"].Value = data.EndDate;
                            sheet.Cells["O2"].Value = data.DataUnits;
                            sheet.Cells["P2"].Value = data.Timezone;
                            sheet.Cells["Q2"].Value = data.Datum;
                            sheet.Cells["R2"].Value = data.IntervalType;

                            sheet.Cells["A4"].Value = "Date";
                            sheet.Cells["B4"].Value = "Day";
                            sheet.Cells["C4"].Value = "Time";
                            sheet.Cells["D4"].Value = "Predictions in ft";
                            sheet.Cells["E4"].Value = "Predictions in cm";
                            sheet.Cells["F4"].Value = "High/Low";

                            int rowCounter = 5;

                            foreach (Item item in data.Data.Item)
                            {
                                int columnCounter = 1;

                                sheet.Cells[rowCounter, columnCounter++].Value = item.Date;
                                sheet.Cells[rowCounter, columnCounter++].Value = item.Day;
                                sheet.Cells[rowCounter, columnCounter++].Value = item.Time;
                                sheet.Cells[rowCounter, columnCounter++].Value = item.Predictions_in_ft;
                                sheet.Cells[rowCounter, columnCounter++].Value = item.Predictions_in_cm;
                                sheet.Cells[rowCounter, columnCounter++].Value = item.Highlow;

                                rowCounter++;
                            }

                            package.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private void LogException(Exception ex)
        {
            MyForm.Invoke(new Action(
                delegate ()
                {
                    MyRichTextBox.Text += ex.Message + Environment.NewLine;
                    MyRichTextBox.Text += ex.StackTrace + Environment.NewLine;
                    MyRichTextBox.SelectionStart = MyRichTextBox.Text.Length;
                    MyRichTextBox.ScrollToCaret();
                }));

            MyForm.Invoke(new Action(
                delegate ()
                {
                    MyForm.Refresh();
                }));
        }

        private void Log(String text)
        {
            MyForm.Invoke(new Action(
                delegate ()
                {
                    MyRichTextBox.Text += text + Environment.NewLine;
                    MyRichTextBox.SelectionStart = MyRichTextBox.Text.Length;
                    MyRichTextBox.ScrollToCaret();
                }));

            MyForm.Invoke(new Action(
                delegate ()
                {
                    MyForm.Refresh();
                }));
        }
    }
}
