using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MediaRatingsPlatform
{
  internal class Server
  {
    readonly Router router;
    readonly HttpListener listener;

    public Server()
    {
      listener = new();
      router = new();
      listener.Prefixes.Add("http://localhost:8080/");

      ConfigureRoutes();
    }

    private void ConfigureRoutes()
    {
      router.AddRoute("GET", "/api/users/1/profile", async (context, parameters) =>
      {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";

        StreamWriter writer = new(context.Response.OutputStream);
        await writer.WriteAsync("User profile");
        Console.WriteLine("User profile");

        context.Response.Close();
      });
    }

    public async Task RunAsync()
    {
      listener.Start();

      while (true)
      {
        HttpListenerContext context = await listener.GetContextAsync();

        _ = HandleRequestAsync(context);

        // if (context.Request.Url?.AbsolutePath == "/")
        // {
        //   string response_string = "Hello root";
        //   byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response_string);
        //
        //   context.Response.ContentLength64 = buffer.Length;
        //   context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        // }
        //
        // if (context.Request.Url?.AbsolutePath == "/json")
        // {
        //   string file_data = File.ReadAllText("./test_data.json");
        //   file_data = Regex.Replace(file_data, @"\s+", " ");
        //   byte[] buffer = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(file_data));
        //
        //   context.Response.ContentLength64 = buffer.Length;
        //   context.Response.ContentType = "Application/Json";
        //   context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        // }
      }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
      try
      {
        await router.RunAsync(context);
      }
      catch (Exception e)
      {
        Console.WriteLine(e);

        if (context.Response.OutputStream.CanWrite)
        {
          context.Response.StatusCode = 500;
          context.Response.Close();
        }
      }
    }
  }
}
