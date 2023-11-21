using System.Net;
using NAudio.Wave;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up an HTTP listener to accept WebSocket connections
        var listener = new HttpListener();
        listener.Prefixes.Add("http://*:8080/"); // Listen on all available network interfaces
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