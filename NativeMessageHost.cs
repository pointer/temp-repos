using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms; // Needed for UI dialogs

class Program
{
    static void Main()
    {
        while (true) // Keep running to listen for messages
        {
            try
            {
                // Read message length (first 4 bytes)
                var stdin = Console.OpenStandardInput();
                var lengthBytes = new byte[4];
                stdin.Read(lengthBytes, 0, 4);
                int length = BitConverter.ToInt32(lengthBytes, 0);

                // Read the JSON message
                var messageBytes = new byte[length];
                stdin.Read(messageBytes, 0, length);
                string json = Encoding.UTF8.GetString(messageBytes);

                // Parse the message
                var message = JsonSerializer.Deserialize<BrowserMessage>(json);
                
                if (message?.Action == "showPrintDialog")
                {
                    // Run UI on STA thread (required for dialogs)
                    var thread = new System.Threading.Thread(() =>
                    {
                        // Call your existing Helper DLL to show the dialog
                        var printDialog = new YourPrintDialog(); // Replace with your actual dialog class
                        printDialog.SetContent(message.Content.Html);
                        printDialog.ShowDialog();
                    });
                    thread.SetApartmentState(System.Threading.ApartmentState.STA);
                    thread.Start();
                    thread.Join(); // Wait for dialog to close
                }

                // Send a response back to the browser
                var response = new { success = true };
                SendResponse(response);
            }
            catch (Exception ex)
            {
                File.AppendAllText("error.log", $"{DateTime.Now}: {ex}\n");
            }
        }
    }

    static void SendResponse(object response)
    {
        string json = JsonSerializer.Serialize(response);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        Console.OpenStandardOutput().Write(BitConverter.GetBytes(bytes.Length), 0, 4);
        Console.OpenStandardOutput().Write(bytes, 0, bytes.Length);
        Console.OpenStandardOutput().Flush();
    }
}

public class BrowserMessage
{
    public string Action { get; set; }
    public PrintContent Content { get; set; }
}

public class PrintContent
{
    public string Url { get; set; }
    public string Title { get; set; }
    public string Html { get; set; }
}
