using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;

namespace SeleniumTest.Browser
{
    public static class BrowserLauncher
    {
        public static IWebDriver LaunchChrome(string chromePath, string driverPath)
        {
            var chromeBinaryPath = Path.Combine(chromePath, "chrome.exe");
            var chromeDriverExecutablePath = Path.Combine(driverPath, "chromedriver.exe");

            var options = new ChromeOptions();
            options.BinaryLocation = chromeBinaryPath;
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            var driver = new ChromeDriver(Path.GetDirectoryName(chromeDriverExecutablePath), options);
            return driver;
        }
    }
}
