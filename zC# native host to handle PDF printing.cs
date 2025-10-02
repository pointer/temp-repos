public class PrintContent
{
    public string Type { get; set; }  // "pdf" or "html"
    public string Url { get; set; }
    public string Html { get; set; }
    public string Title { get; set; }
}

static void Main()
{
    // ... [existing setup code] ...
    
    if (message?.Action == "showPrintDialog")
    {
        if (message.Content.Type == "pdf")
        {
            // Handle PDF printing
            string pdfPath = DownloadPdf(message.Content.Url);
            
            // Show your custom dialog with PDF
            var dialog = new CustomPrintDialog(pdfPath);
            Application.Run(dialog);
        }
        else
        {
            // Handle HTML printing (existing logic)
            var dialog = new CustomPrintDialog(message.Content.Html);
            Application.Run(dialog);
        }
    }
}

static string DownloadPdf(string url)
{
    string tempPath = Path.GetTempFileName() + ".pdf";
    using (var client = new WebClient())
    {
        client.DownloadFile(url, tempPath);
    }
    return tempPath;
}
