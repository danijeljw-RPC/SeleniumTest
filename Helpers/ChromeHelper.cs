using System.IO.Compression;

namespace SeleniumTest.Helpers
{
    public class ChromeHelper
    {
        // Download and extract zip files, overwriting existing files if necessary
        public static async Task<string> DownloadAndExtract(string url, string extractPath)
        {
            string zipFile = Guid.NewGuid().ToString() + ".zip";
            string zipPath = Path.Combine(Path.GetTempPath(), zipFile);

            using HttpClient client = new HttpClient();
            using (var response = await client.GetAsync(url))
            {
                using (var fileStream = new FileStream(zipPath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            // Ensure the extraction directory exists
            Directory.CreateDirectory(extractPath);

            // Extract files from the ZIP, overwriting if they already exist
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(extractPath, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!); // Ensure directory exists

                    // Extract each entry with overwrite set to true
                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
            }

            File.Delete(zipPath); // Clean up the downloaded ZIP
            return Directory.GetDirectories(extractPath).FirstOrDefault() ?? extractPath;
        }
    }
}

