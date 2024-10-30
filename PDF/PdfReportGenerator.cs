using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace SeleniumTest.PDF
{
    public class PdfReportGenerator
    {
        private readonly string title;
        private readonly List<string> folderPaths;

        public PdfReportGenerator(string title, List<string> folders)
        {
            this.title = title;
            folderPaths = folders;
        }

        public void CreatePdf(string outputPath)
        {
            Document document = new Document();
            Section section = document.AddSection();

            // Title Page
            AddTitlePage(document);

            // Table of Contents
            AddTableOfContents(document);

            // Add each test result
            int pageIndex = 3; // Start from page 3 since ToC is on page 2
            foreach (var folderPath in folderPaths)
            {
                AddTestResultPage(document, folderPath, ref pageIndex);
            }

            // Render and save PDF
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();
            renderer.Save(outputPath);
        }

        private void AddTitlePage(Document document)
        {
            var section = document.LastSection;
            section.AddParagraph(title, "Title");
            section.AddParagraph("Date: " + DateTime.Now.ToString("yyyy-MM-dd"), "Subtitle");
        }

        private void AddTableOfContents(Document document)
        {
            var section = document.LastSection;
            section.AddPageBreak();
            section.AddParagraph("Table of Contents", "Heading1");
        }

        private void AddTestResultPage(Document document, string folderPath, ref int pageIndex)
        {
            var section = document.AddSection();
            section.AddPageBreak();

            var metaPath = Path.Combine(folderPath, ".meta");
            var metaContent = File.ReadAllText(metaPath);

            section.AddParagraph($"Test Result {pageIndex}", "Heading2");

            foreach (var file in Directory.GetFiles(folderPath))
            {
                if (file.EndsWith(".png"))
                {
                    section.AddImage(file);
                }
                else if (file.EndsWith(".txt") || file.EndsWith(".meta"))
                {
                    section.AddParagraph(File.ReadAllText(file), "Body");
                }
            }
            pageIndex++;
        }
    }
}

