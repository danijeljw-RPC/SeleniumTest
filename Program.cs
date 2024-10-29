using System;
using System.IO;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await RunAsync();
        }

        static async Task RunAsync()
        {
            // Define path to ChromeDriver (assuming it's in "Drivers" folder in the output directory)
            string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Drivers");

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Pass the driver path to ChromeDriver
            using IWebDriver driver = new ChromeDriver(driverPath, options);

            // Navigate to Google
            driver.Navigate().GoToUrl("https://www.google.com");

            // Find the search box by name attribute and enter "Selenium C#"
            IWebElement searchBox = await FindElementAsync(driver, By.Name("q"));
            searchBox.SendKeys("Selenium C#");

            // Submit the search
            searchBox.Submit();

            // Wait for results to load and get the title of the first result
            IWebElement firstResult = await FindElementAsync(driver, By.CssSelector("h3"));
            Console.WriteLine("First result title: " + firstResult.Text);

            // Close the driver
            driver.Quit();
        }

        // Async helper method to find an element with a wait
        static async Task<IWebElement> FindElementAsync(IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            return await Task.Run(() =>
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            });
        }
    }
}
