using System.Net;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up an HTTP listener to accept WebSocket on all available network interfaces
        var listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/");

        try
        {
            listener.Start();
            Console.WriteLine("WebSocket server is running");
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        while (true)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            _ = ProcessWebSocketRequest(context);
        }
    }
    static async Task ProcessWebSocketRequest(HttpListenerContext context)
    {
        var canoe = new Canoe();
        await canoe.StartAsync(context);
    }
}