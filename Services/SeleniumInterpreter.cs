using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SeleniumTest.Services
{
    public class SeleniumInterpreter
    {
        private readonly IWebDriver driver;
        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        // Accept an existing IWebDriver instance
        public SeleniumInterpreter(IWebDriver webDriver)
        {
            driver = webDriver;
        }

        public async Task ExecuteScriptAsync(string filePath)
        {
            var commands = await File.ReadAllLinesAsync(filePath);
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i].Trim();
                if (command.StartsWith("For each element"))
                {
                    var selector = ParseLocator(command);
                    var elements = driver.FindElements(selector);
                    foreach (var element in elements.Take(10)) // Example: taking only top 10
                    {
                        i = await ExecuteLoopCommandsAsync(commands, i + 1, element);
                    }
                }
                else
                {
                    await ProcessCommandAsync(command);
                }
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

                case string s when s.StartsWith("Submit element"):
                    {
                        var locator = ParseLocator(command);
                        await Task.Run(() => driver.FindElement(locator).Submit());
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

                case string s when s.StartsWith("Find element by"):
                    {
                        var parts = command.Split("and get");
                        var locator = ParseLocator(parts[0]);
                        var element = driver.FindElement(locator);

                        if (parts[1].Contains("text as"))
                        {
                            var variableName = parts[1].Split("text as")[1].Trim();
                            variables[variableName] = element.Text;
                        }
                        else if (parts[1].Contains("attribute"))
                        {
                            var attribute = parts[1].Split("attribute")[1].Split("as")[0].Trim();
                            var variableName = parts[1].Split("as")[1].Trim();
                            variables[variableName] = element.GetAttribute(attribute);
                        }
                        break;
                    }

                case string s when s.StartsWith("Print"):
                    {
                        var message = ReplacePlaceholders(command.Replace("Print ", "").Trim());
                        Console.WriteLine(message);
                        break;
                    }

                default:
                    throw new ArgumentException($"Command '{command}' is not recognized.");
            }
        }

        private async Task<int> ExecuteLoopCommandsAsync(string[] commands, int startIndex, IWebElement element)
        {
            for (int i = startIndex; i < commands.Length; i++)
            {
                var command = commands[i].Trim();

                if (command.StartsWith("End loop"))
                {
                    return i; // Exit loop
                }

                if (command.StartsWith("Find element by"))
                {
                    var parts = command.Split("and get");
                    var locator = ParseLocator(parts[0]);
                    var loopElement = element.FindElement(locator);

                    if (parts[1].Contains("text as"))
                    {
                        var variableName = parts[1].Split("text as")[1].Trim();
                        variables[variableName] = loopElement.Text;
                    }
                    else if (parts[1].Contains("attribute"))
                    {
                        var attribute = parts[1].Split("attribute")[1].Split("as")[0].Trim();
                        var variableName = parts[1].Split("as")[1].Trim();
                        variables[variableName] = loopElement.GetAttribute(attribute);
                    }
                }
                else if (command.StartsWith("Print"))
                {
                    var message = ReplacePlaceholders(command.Replace("Print ", "").Trim());
                    Console.WriteLine(message);
                }
                else
                {
                    await ProcessCommandAsync(command);
                }
            }

            return commands.Length; // In case "End loop" is not found
        }

        private string ReplacePlaceholders(string message)
        {
            foreach (var variable in variables)
            {
                message = message.Replace($"{{{variable.Key}}}", variable.Value);
            }
            return message;
        }

        private By ParseLocator(string text)
        {
            return text switch
            {
                var t when t.Contains("by ID") => By.Id(text.Split("by ID")[1].Trim()),
                var t when t.Contains("by Name") => By.Name(text.Split("by Name")[1].Trim()),
                var t when t.Contains("by Class") => By.ClassName(text.Split("by Class")[1].Trim()),
                var t when t.Contains("by XPath") => By.XPath(text.Split("by XPath")[1].Trim()),
                var t when t.Contains("by CSS selector") => By.CssSelector(text.Split("by CSS selector")[1].Trim()),
                _ => throw new ArgumentException("Locator type not recognized.")
            };
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
