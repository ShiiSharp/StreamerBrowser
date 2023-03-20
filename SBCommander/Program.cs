using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            MessageBox.Show("Usage: <appname> <string_parameter>", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        string argument = args[0];
        string url = $"http://localhost:9801/{argument}";

        int statusCode;
        try
        {
            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);
            statusCode = (int)response.StatusCode;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusCode = -1;
        }

        Environment.Exit(statusCode);
    }
}

