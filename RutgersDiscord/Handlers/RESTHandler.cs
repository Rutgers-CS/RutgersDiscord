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
    private readonly GameServerHandler _gameServerHandler;
    private readonly ConfigHandler _config;

    public RESTHandler(DiscordSocketClient client, DatabaseHandler database, InteractivityService interactivity, GameServerHandler gameServerHandler, ConfigHandler config)
    {
        _client = client;
        _database = database;
        _interactivity = interactivity;
        _gameServerHandler = gameServerHandler;
        _config = config;
    }

    public void Listen()
    {
        HttpListener listener = new HttpListener();
#if DEBUG
        listener.Prefixes.Add($"http://localhost:{_config.settings.ApplicationSettings.Port}/api/");
#else
        listener.Prefixes.Add($"http://*:{_config.settings.application.port}/api/");
#endif
        listener.Start();

        while (true)
        {
            IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallbackAsync), listener);
            result.AsyncWaitHandle.WaitOne();
        }
    }

    public async void ListenerCallbackAsync(IAsyncResult result)
    {
        HttpListener listener = (HttpListener)result.AsyncState;
        // Call EndGetContext to complete the asynchronous operation.
        HttpListenerContext context = listener.EndGetContext(result);
        HttpListenerRequest request = context.Request;
        StreamReader sr = new(request.InputStream, request.ContentEncoding);
        string line = sr.ReadToEnd();

        var response = context.Response;
        string responseString = "202 Accepted";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        Console.WriteLine("Calling webhook parser");
        await _gameServerHandler.UpdateDatabase(line);
    }
}