using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MediaRatingsPlatform
{
  internal class Server
  {
    public Server()
    {
      HttpListener listener = new();
      listener.Prefixes.Add("http://localhost:8080/");
      listener.Start();

      Console.WriteLine("Starting server...");

      while (true)
      {
        HttpListenerContext context = listener.GetContext();

        if (context.Request.Url == null)
        {
          return;
        }

        string[] steps = context.Request.Url.AbsolutePath.Split("/");
        steps = [.. steps.Skip(1)];

        Console.WriteLine($"Steps [{steps.Length}]:");

        foreach (string step in steps)
        {
          Console.WriteLine(step);
        }

        if (context.Request.Url?.AbsolutePath == "/")
        {
          string response_string = "Hello root";
          byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response_string);

          context.Response.ContentLength64 = buffer.Length;
          context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        if (context.Request.Url?.AbsolutePath == "/json")
        {
          string file_data = File.ReadAllText("./test_data.json");
          file_data = Regex.Replace(file_data, @"\s+", " ");
          byte[] buffer = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(file_data));

          context.Response.ContentLength64 = buffer.Length;
          context.Response.ContentType = "Application/Json";
          context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        context.Response.Close();
      }
    }
  }
}
