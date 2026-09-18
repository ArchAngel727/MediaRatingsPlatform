using System.Net;

namespace MediaRatingsPlatform
{
  internal class Router
  {
    private sealed record Route(
        string Methode,
        string[] Segments,
        Func<HttpListenerContext, Dictionary<string, string>, Task> Handler
    );

    private readonly List<Route> routes = [];

    private static string[] SplitPath(string path)
    {
      string[] segments = path.Split("/");
      return [.. segments.Skip(1)];
    }

    public void AddRoute(string methode, string path, Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
    {
      string[] segments = SplitPath(path);

      routes.Add(new Route(methode, segments, handler));
    }

    private static async Task SendResponse(
        HttpListenerContext context,
        string message
    )
    {
      StreamWriter writer = new(context.Response.OutputStream);
      await writer.WriteAsync(message);
      context.Response.Close();
    }

    public async Task RunAsync(HttpListenerContext context)
    {
      if (context.Request.Url == null)
      {
        return;
      }

      string methode = context.Request.HttpMethod;
      string[] segments = SplitPath(context.Request.Url.AbsolutePath);
      bool matches = true;

      foreach (Route route in routes)
      {
        if (route.Methode != methode)
          return;

        if (route.Segments.Length != segments.Length)
          return;

        var parameters = new Dictionary<string, string>();

        for (int i = 0; i < route.Segments.Length; i++)
        {
          string route_segment = route.Segments[i];
          string request_segment = segments[i];

          if (request_segment.StartsWith(":"))
          {
            string parameter_name = route_segment[1..];

            parameters[parameter_name] = Uri.UnescapeDataString(request_segment);
          }
          else if (request_segment != route_segment)
          {
            matches = false;
            break;
          }
        }

        if (matches)
        {
          await route.Handler(context, parameters);
          return;
        }
      }

      context.Response.StatusCode = 404;
      await SendResponse(context, "Not Found");
    }
  }
}
