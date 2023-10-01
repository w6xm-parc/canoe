using System.Net;
using System.Net.WebSockets;
using NAudio.Wave;

class AudioWebSocket
{
    private readonly WaveFormat waveFormat;
    private WasapiLoopbackCapture loopbackCapture;
    private MemoryStream audioBuffer;

    public AudioWebSocket(WaveFormat waveFormat) => this.waveFormat = waveFormat;

    public async Task StartAsync(HttpListenerContext context)
    {
        if (context.Request.IsWebSocketRequest)
        {
            Console.WriteLine(context.Request.RemoteEndPoint.ToString() + " connected");
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket; // Get the WebSocket instance

            // Initialize audio capture
            loopbackCapture = new WasapiLoopbackCapture();
            loopbackCapture.DataAvailable += OnDataAvailable;
            loopbackCapture.StartRecording();

            audioBuffer = new MemoryStream();

            while (webSocket.State == WebSocketState.Open)
            {
                // Send audio data to the WebSocket client
                if (audioBuffer.Length > 0)
                {
                    byte[] audioData = audioBuffer.ToArray();
                    await webSocket.SendAsync(new ArraySegment<byte>(audioData), WebSocketMessageType.Binary, true, default);
                    audioBuffer.SetLength(0);
                }
                await Task.Delay(10); // Adjust delay as needed
            }

            // Clean up resources when the WebSocket connection is closed
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
            audioBuffer.Dispose();
        }
        else
        {
            Console.WriteLine("no workie");
            context.Response.StatusCode = 400;
            context.Response.Close();
            return;
        }
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        // Write audio data to the buffer
        audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);
        audioBuffer.Flush();
    }
}
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
        var audioWebSocket = new AudioWebSocket(new WaveFormat(48000, 16, 2));
        await audioWebSocket.StartAsync(context);
    }
}
