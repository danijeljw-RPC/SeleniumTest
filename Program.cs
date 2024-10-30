using SeleniumTest.Browser;
using SeleniumTest.Configuration;
using SeleniumTest.Helpers;
using SeleniumTest.Services;

namespace SeleniumTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Extract version and browser from command-line arguments
            string versionArg = args.FirstOrDefault(a => a.StartsWith("--version="))?.Split('=')[1] ?? "132.0.6804.0";
            string browser = args.FirstOrDefault(a => a.StartsWith("--browser="))?.Split('=')[1] ?? "google-chrome";

            // Detect platform (win64, win32, etc.)
            string platform = PlatformHelper.GetPlatform();

            // Determine JSON URL based on selected browser
            var jsonUrl = browser switch
            {
                "google-chrome" => "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json",
                _ => throw new ArgumentException("Unsupported browser")
            };

            // Retrieve the selected version information from the JSON data
            var selectedVersion = await VersionService.GetSelectedVersion(jsonUrl, versionArg, platform);

            if (selectedVersion == null)
            {
                Console.WriteLine("The required version or downloads are unavailable.");
                return;
            }

            // Set base paths for Chrome and ChromeDriver directories, keeping browser and version structure
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string chromeBasePath = Path.Combine(baseDir, "Browsers", browser, selectedVersion.Version!);
            string driverBasePath = Path.Combine(baseDir, "Drivers", browser, selectedVersion.Version!);

            // Download and extract Chrome and ChromeDriver to the appropriate directories
            string chromePath = await ChromeHelper.DownloadAndExtract(selectedVersion.ChromeUrl!, chromeBasePath);
            string driverPath = await ChromeHelper.DownloadAndExtract(selectedVersion.ChromeDriverUrl!, driverBasePath);

            // Launch Chrome with the specified paths
            await BrowserLauncher.LaunchChrome(chromePath, driverPath);
        }
    }
}
