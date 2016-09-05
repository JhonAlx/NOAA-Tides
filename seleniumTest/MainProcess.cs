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
        public String FolderName { get; set; }
        
        private const String ACCESS_URL = "https://tidesandcurrents.noaa.gov/tide_predictions.html";
        
        private IWebDriver driver;

        public void Run()
        {
            Log($"Starting scraping job on thread ID {Thread.CurrentThread.ManagedThreadId}");

            var options = new ChromeOptions();
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
                statesHrefs.Add(new KeyValuePair<string, string>(cityLink.Text, cityLink.GetAttribute("href")));

            Log($"Found {statesHrefs.Count} states");

            NavigateStations(statesHrefs);

            driver.Quit();
            Log("Process finished on thread ID " + Thread.CurrentThread.ManagedThreadId);
        }

        private void NavigateStations(List<KeyValuePair<string, string>> statesHrefs)
        {
            foreach (var href in statesHrefs)
            {
                Log($"Navigating to {href.Key} state");

                driver.Url = href.Value;
                driver.Navigate();

                List<KeyValuePair<string, string>> stationHrefs = new List<KeyValuePair<string, string>>();

                //Get only TDs with info
                var stationData = driver.FindElements(By.XPath("//table[@class='table']/tbody/tr[descendant::td[@class='stationname']]"));

                foreach (var stationLink in stationData)
                {
                    //Process data here
                }
            }
        }

        private void ProcessStations(List<KeyValuePair<string, string>> stationHrefs)
        {
            foreach (var href in stationHrefs)
            {
                Log($"Navigating to {href.Key} station");

                driver.Url = href.Value;
                driver.Navigate();
            }
        }

        private void LogException(Exception ex)
        {
            MyForm.Invoke(new Action(
                delegate()
                {
                    MyRichTextBox.Text += ex.Message + Environment.NewLine;
                    MyRichTextBox.Text += ex.StackTrace + Environment.NewLine;
                    MyRichTextBox.SelectionStart = MyRichTextBox.Text.Length;
                    MyRichTextBox.ScrollToCaret();
                }));

            MyForm.Invoke(new Action(
                delegate()
                {
                    MyForm.Refresh();
                }));
        }

        private void Log(String text)
        {
            MyForm.Invoke(new Action(
                delegate()
                {
                    MyRichTextBox.Text += text + Environment.NewLine;
                    MyRichTextBox.SelectionStart = MyRichTextBox.Text.Length;
                    MyRichTextBox.ScrollToCaret();
                }));

            MyForm.Invoke(new Action(
                delegate()
                {
                    MyForm.Refresh();
                }));
        }
    }
}
