using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumTest.Browser
{
    public static class BrowserLauncher
    {
        public static async Task LaunchChrome(string chromePath, string driverPath)
        {
            // Build paths to chrome binary and chromedriver executable
            var chromeBinaryPath = Path.Combine(chromePath, "chrome.exe");
            var chromeDriverExecutablePath = Path.Combine(driverPath, "chromedriver.exe");

            var options = new ChromeOptions();
            options.BinaryLocation = chromeBinaryPath;

            // Add arguments to run in headless mode and trace sandbox diagnostics
            options.AddArgument("--headless"); // Enable headless mode
            options.AddArgument("--disable-gpu"); // Prevents some graphics-related errors in headless mode
            options.AddArgument("--trace-startup=-*,disabled-by-default-sandbox");
            options.AddArgument("--no-sandbox"); // Optional: Disable sandbox for debugging

            using IWebDriver driver = new ChromeDriver(Path.GetDirectoryName(chromeDriverExecutablePath), options);

            // Go to Google Australia
            driver.Navigate().GoToUrl("https://www.google.com.au");

            // Accept cookies (if required)
            try
            {
                var acceptButton = driver.FindElement(By.CssSelector("button#L2AGLb")); // Adjust the selector as needed
                acceptButton.Click();
            }
            catch (NoSuchElementException) { /* No accept button found, continue */ }

            // Find the search box, enter search term, and submit
            var searchBox = driver.FindElement(By.Name("q"));
            searchBox.SendKeys("OpenAI ChatGPT");
            searchBox.Submit();

            // Wait for results to load
            await Task.Delay(2000); // Or use WebDriverWait for a more flexible wait

            // Take a screenshot of the results page
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            string screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "screenshot.png");
            screenshot.SaveAsFile(screenshotPath);
            Console.WriteLine($"Screenshot saved to: {screenshotPath}");

            // Find the top 10 search results
            var results = driver.FindElements(By.CssSelector("div.g")).Take(10);
            var topResults = new List<(string Title, string Url)>();

            foreach (var result in results)
            {
                // Extract title
                var title = result.FindElement(By.CssSelector("h3")).Text;

                // Extract URL
                var linkElement = result.FindElement(By.CssSelector("a"));
                var url = linkElement.GetAttribute("href");

                topResults.Add((title, url));
            }

            // Output the top 10 results
            Console.WriteLine("Top 10 Search Results:");
            foreach (var (title, url) in topResults)
            {
                Console.WriteLine($"Title: {title}\nURL: {url}\n");
            }

            driver.Quit();

            Console.WriteLine("Chrome launched with specified version.");
        }
    }
}

