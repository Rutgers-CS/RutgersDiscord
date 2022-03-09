using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IServiceProvider _services;


    public InteractionHandler(DiscordSocketClient client, IServiceProvider services, InteractionService interaction)
    {
        _client = client;
        _services = services;
        _interaction = interaction;
    }

    public async Task InstallAsync()
    {
        await _interaction.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), _services);
        _client.InteractionCreated += HandleInteraction;

    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            await _interaction.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}
