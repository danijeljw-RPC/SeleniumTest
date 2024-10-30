using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace SeleniumTest.Services
{
    public class SeleniumInterpreter
    {
        private readonly IWebDriver driver;

        public SeleniumInterpreter()
        {
            driver = new ChromeDriver();
        }

        public async Task ExecuteScriptAsync(string filePath)
        {
            var commands = await File.ReadAllLinesAsync(filePath);
            foreach (var command in commands)
            {
                await ProcessCommandAsync(command.Trim());
            }
            driver.Quit();
        }

        private async Task ProcessCommandAsync(string command)
        {
            switch (command)
            {
                case string s when s.StartsWith("Open URL"):
                    {
                        var url = command.Replace("Open URL ", "").Trim();
                        await Task.Run(() => driver.Navigate().GoToUrl(url));
                        break;
                    }

                case string s when s.StartsWith("Click element"):
                    {
                        var locator = ParseLocator(command);
                        await Task.Run(() => driver.FindElement(locator).Click());
                        break;
                    }

                case string s when s.StartsWith("Enter text"):
                    {
                        var text = command.Split('"')[1];
                        var locator = ParseLocator(command.Split("into")[1]);
                        await Task.Run(() => driver.FindElement(locator).SendKeys(text));
                        break;
                    }

                case string s when s.StartsWith("Clear element"):
                    {
                        var locator = ParseLocator(command);
                        await Task.Run(() => driver.FindElement(locator).Clear());
                        break;
                    }

                case string s when s.StartsWith("Wait for element"):
                    {
                        var locator = ParseLocator(command.Split("to be")[0]);
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ParseSeconds(command)));
                        await Task.Run(() => wait.Until(driver => driver.FindElement(locator).Displayed));
                        break;
                    }

                case string s when s.StartsWith("Take screenshot"):
                    {
                        var fileName = command.Replace("Take screenshot and save as ", "").Trim();
                        await Task.Run(() => ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(fileName));
                        break;
                    }

                case string s when s.StartsWith("Switch to frame"):
                    {
                        var frameId = command.Replace("Switch to frame by ID ", "").Trim();
                        await Task.Run(() => driver.SwitchTo().Frame(frameId));
                        break;
                    }

                case string s when s.StartsWith("Switch to default content"):
                    {
                        await Task.Run(() => driver.SwitchTo().DefaultContent());
                        break;
                    }

                case string s when s.StartsWith("Go back"):
                    {
                        await Task.Run(() => driver.Navigate().Back());
                        break;
                    }

                case string s when s.StartsWith("Go forward"):
                    {
                        await Task.Run(() => driver.Navigate().Forward());
                        break;
                    }

                case string s when s.StartsWith("Refresh page"):
                    {
                        await Task.Run(() => driver.Navigate().Refresh());
                        break;
                    }

                default:
                    throw new ArgumentException($"Command '{command}' is not recognized.");
            }
        }

        private By ParseLocator(string text)
        {
            if (text.Contains("by ID"))
                return By.Id(text.Split("by ID")[1].Trim());
            if (text.Contains("by Name"))
                return By.Name(text.Split("by Name")[1].Trim());
            if (text.Contains("by Class"))
                return By.ClassName(text.Split("by Class")[1].Trim());
            if (text.Contains("by XPath"))
                return By.XPath(text.Split("by XPath")[1].Trim());
            throw new ArgumentException("Locator type not recognized.");
        }

        private int ParseSeconds(string command)
        {
            var parts = command.Split(" ");
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int seconds))
                    return seconds;
            }
            return 10; // default if not found
        }
    }
}

