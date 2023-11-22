using NAudio.Wave;
using System.Net.WebSockets;
using System.Net;
using System.Text;

class Canoe
{
    // private readonly WaveFormat waveFormat;
    private WaveFormat waveFormat = new WaveFormat(48000, 16, 2);
    private WasapiLoopbackCapture loopbackCapture;
    private MemoryStream audioBuffer;
    public int heading = -1;
    public Boolean sendRawAudio = false;

    public async Task StartAsync(HttpListenerContext context)
    {
        if (context.Request.IsWebSocketRequest)
        {
            Console.WriteLine(context.Request.RemoteEndPoint.ToString() + " connected");
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket; // Get the WebSocket instance

            _ = ReceiveMessagesAsync(webSocket);
            _ = SendMessagesAsync(webSocket);

            while (true)
            {
                await Task.Delay(1000);  
            }
        }
        else
        {
            Console.WriteLine("Not a WebSocket Request");
            context.Response.StatusCode = 400;
            context.Response.Close();
            return;
        }
    }
    private async Task SendMessagesAsync(WebSocket webSocket)
    {
        // Initialize audio capture
        loopbackCapture = new WasapiLoopbackCapture();
        loopbackCapture.DataAvailable += OnDataAvailable;
        loopbackCapture.StartRecording();

        audioBuffer = new MemoryStream();
        int sendRawAudioMessageCount = 0;

        byte[] wsBuffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                // Send audio data to the WebSocket client
                if (audioBuffer.Length > 0)
                {
                    sendRawAudioMessageCount += 1;
                    byte[] audioData = audioBuffer.ToArray();
                    await webSocket.SendAsync(new ArraySegment<byte>(audioData), WebSocketMessageType.Binary, true, default);
                    audioBuffer.SetLength(0);
                    if (sendRawAudioMessageCount % 100 == 0)
                    {
                        long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                        Console.WriteLine($"{unixTime}: {sendRawAudioMessageCount} messages");
                    }
                }

                await Task.Delay(10); // Adjust delay as needed
            }
            // Clean up resources when the WebSocket connection is closed
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
            audioBuffer.Dispose();
            Console.WriteLine("Cleaned up audio resources");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket send error: {ex.Message}");
        }
    }   
    private static async Task ReceiveMessagesAsync(WebSocket webSocket)
    {
        int heading = -1;
        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                { 
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine(receivedMessage);
                    int stringStart = receivedMessage.IndexOf(':', 0);
                    stringStart += 2;
                    int stringStop = receivedMessage.Length;
                    heading = int.Parse(receivedMessage.Substring(stringStart, stringStop - stringStart));
                    Console.WriteLine($"{heading}");

                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket receive error: {ex.Message}");
        }
    }


    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        // Write audio data to the buffer
        audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);
        audioBuffer.Flush();
    }

    /* static void WriteMic(byte[] buffer, int count)
     {
         string outputFile = "my.wav"; 
         var writer = new WaveFileWriter(outputFile, new WaveFormat(48000, 16, 2));
         writer.Write(outputFile, count, count);

     }

     static void SendMic(byte[] buffer, int count)
     {
         // Create a WaveOutEvent to play audio
         using (WaveOutEvent waveOut = new WaveOutEvent())
         {
             // Set the desired audio format (16-bit PCM, 48 kHz, stereo)
             var waveFormat = new NAudio.Wave.WaveFormat(48000, 16, 2);
             waveOut.Init(new NAudio.Wave.RawSourceWaveStream(new System.IO.MemoryStream(buffer, 0, count), waveFormat));

             // Start playing audio
             waveOut.Play();

             // Wait for playback to finish
             while (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
             {
                 Thread.Sleep(100);
             }

             // Stop and dispose of the WaveOutEvent
             waveOut.Stop();
             waveOut.Dispose();
         }
     }
    */
}