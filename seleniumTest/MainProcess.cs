using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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
                }
            }

            return stations;
        }

        private void ProcessStation(List<Station> stations)
        {
            foreach (var station in stations)
            {
                Log($"Navigating to {station.Name} station");

                driver.Url = station.Href;
                driver.Navigate();

                var xml = driver.FindElement(By.XPath("//input[@value='Annual XML']"));
                xml.Click();
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
