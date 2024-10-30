namespace SeleniumTest.Helpers
{
    public class FolderManagerHelper
    {
        private readonly string baseDir;

        public FolderManagerHelper(string baseDirectory)
        {
            baseDir = baseDirectory;
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
        }

        public string CreateNewFolder()
        {
            int folderNumber = 1;
            string newFolderPath;

            do
            {
                newFolderPath = Path.Combine(baseDir, folderNumber.ToString());
                folderNumber++;
            } while (Directory.Exists(newFolderPath));

            Directory.CreateDirectory(newFolderPath);
            return newFolderPath;
        }
    }
}

