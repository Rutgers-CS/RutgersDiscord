using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Discord.WebSocket;
using RutgersDiscord.Handlers;
using Interactivity;

public class RESTHandler
{
    private readonly DiscordSocketClient _client;
    private readonly DatabaseHandler _database;
    private readonly InteractivityService _interactivity;

    public RESTHandler(DiscordSocketClient client, DatabaseHandler database, InteractivityService interactivity)
    {
        _client = client;
        _database = database;
        _interactivity = interactivity;
    }

    public void Listen()
    {
        HttpListener listener = new HttpListener();
#if DEBUG
        listener.Prefixes.Add($"http://localhost:{Environment.GetEnvironmentVariable("port")}/api/");
#else
        listener.Prefixes.Add($"http://*:{Environment.GetEnvironmentVariable("port")}/api/");
#endif
        listener.Start();

        while (true)
        {
            IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            result.AsyncWaitHandle.WaitOne();
        }
    }

    public static void ListenerCallback(IAsyncResult result)
    {
        HttpListener listener = (HttpListener)result.AsyncState;
        // Call EndGetContext to complete the asynchronous operation.
        HttpListenerContext context = listener.EndGetContext(result);
        HttpListenerRequest request = context.Request;
        StreamReader sr = new(request.InputStream, request.ContentEncoding);
        string line = sr.ReadToEnd();
        Console.WriteLine(line);

        var response = context.Response;
        string responseString = "202 Accepted";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
        //TODO: ADD GALIFI STUFF HERE
    }
}