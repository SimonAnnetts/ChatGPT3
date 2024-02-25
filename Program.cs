using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var configFile = "ChatGPT3.json";
        var dir = Path.GetDirectoryName(AppContext.BaseDirectory)??"".Replace("\\", "/");
        bool found = false;
        while (dir.Length > 3)
        {
            if(File.Exists(Path.GetFullPath(Path.Combine(dir, configFile))))
            {
                found = true;
                break;
            }
            dir = Path.GetFullPath(Path.Combine(dir, "../"));
        }
        if (!found)
        {
            Console.WriteLine($"{configFile} not found. Please create a ChatGPT3.json file.");
            return 1;
        }


        HttpClient client = new HttpClient();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(dir, configFile), optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var openaiApiKey = config.GetSection("OpenAI").GetValue<string>("ApiKey");
        if (string.IsNullOrEmpty(openaiApiKey))
        {
            Console.WriteLine($"OpenAI API key not found. Please set the OpenAI:ApiKey property in {configFile}");
            return 1;
        }

        client.DefaultRequestHeaders.Add("authorization", $"Bearer {openaiApiKey}");

        Console.WriteLine("Welcome to the OpenAI GPT-3.5 Turbo Instruct Console App!");
        Console.WriteLine("Type 'exit' to quit the program.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Ask a question: ");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (input == "exit")
            {
                break;
            }
            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            //openai completions API
            var content = new StringContent(
                "{\"model\": \"gpt-3.5-turbo-instruct\", \"prompt\": \"" + input + "\", \"temperature\": 1, \"max_tokens\": 100}",
                Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine(responseString);
                continue;
            }

            try
            {
                dynamic json = JsonConvert.DeserializeObject(responseString) ?? throw new Exception("Failed to parse JSON response");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(json.choices[0].text);
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine();
        }

        return 0;
    }
}