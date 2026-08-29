using System;
using System.ClientModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Soenneker.Maf.Dtos.Options;
using Soenneker.Maf.Pool.Abstract;

namespace Soenneker.Maf.Pool.Mistral;

/// <summary>
/// Provides Mistral-specific registration extensions for <see cref="IMafPool"/>, enabling integration via Microsoft Agent Framework (OpenAI-compatible API).
/// </summary>
public static class MafPoolMistralExtension
{
    /// <summary>
    /// Default Mistral API endpoint.
    /// </summary>
    public const string DefaultMistralEndpoint = "https://api.mistral.ai";

    /// <summary>
    /// Registers a Mistral model in the agent pool with optional rate/token limits.
    /// Uses Mistral's OpenAI-compatible API.
    /// </summary>
    /// <param name="pool">Pool that supplies the reusable resource.</param>
    /// <param name="poolId">Identifier of the target pool.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="modelId">Identifier of the model to use.</param>
    /// <param name="apiKey">API key used to authenticate the request.</param>
    /// <param name="endpoint">Service endpoint to call.</param>
    /// <param name="rps">Optional requests-per-second limit.</param>
    /// <param name="rpm">Optional requests-per-minute limit.</param>
    /// <param name="rpd">Optional requests-per-day limit.</param>
    /// <param name="tokensPerDay">Optional daily token limit.</param>
    /// <param name="instructions">Instructions supplied to the model or processor.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task that completes when the mistral addition is complete.</returns>
    public static ValueTask AddMistral(this IMafPool pool, string poolId, string key, string modelId, string apiKey, string? endpoint = null,
        int? rps = null, int? rpm = null, int? rpd = null, int? tokensPerDay = null, string? instructions = null,
        CancellationToken cancellationToken = default)
    {
        var options = new MafOptions
        {
            ModelId = modelId,
            Endpoint = endpoint ?? DefaultMistralEndpoint,
            ApiKey = apiKey,
            RequestsPerSecond = rps,
            RequestsPerMinute = rpm,
            RequestsPerDay = rpd,
            TokensPerDay = tokensPerDay,
            AgentFactory = (opts, _) =>
            {
                Uri uri = new Uri(string.IsNullOrEmpty(opts.Endpoint) ? DefaultMistralEndpoint : opts.Endpoint!, UriKind.Absolute);
                var client = new OpenAIClient(new ApiKeyCredential(opts.ApiKey!), new OpenAIClientOptions { Endpoint = uri });
                var chatClient = client.GetChatClient(opts.ModelId!);
                IChatClient ichatClient = chatClient.AsIChatClient();
                AIAgent agent = ichatClient.AsAIAgent(instructions: instructions ?? "You are a helpful assistant.", name: opts.ModelId);
                return new ValueTask<AIAgent>(agent);
            }
        };

        return pool.Add(poolId, key, options, cancellationToken);
    }

    /// <summary>
    /// Unregisters a Mistral model from the agent pool and removes the associated cache entry.
    /// </summary>
    /// <param name="pool">Pool that supplies the reusable resource.</param>
    /// <param name="poolId">Identifier of the target pool.</param>
    /// <param name="key">Key used to locate the target entry.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>True if the entry existed and was removed; false if it was not present.</returns>
    public static ValueTask<bool> RemoveMistral(this IMafPool pool, string poolId, string key, CancellationToken cancellationToken = default)
    {
        return pool.Remove(poolId, key, cancellationToken);
    }
}
