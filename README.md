[![](https://img.shields.io/nuget/v/soenneker.maf.pool.mistral.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.maf.pool.mistral/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.maf.pool.mistral/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.maf.pool.mistral/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.maf.pool.mistral.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.maf.pool.mistral/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.maf.pool.mistral/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.maf.pool.mistral/actions/workflows/codeql.yml)

# Soenneker.Maf.Pool.Mistral

Provides Mistral-specific registration extensions for `IMafPool`, enabling integration via Microsoft Agent Framework (OpenAI-compatible API).

## Install

```bash
dotnet add package Soenneker.Maf.Pool.Mistral
```

## Usage

```csharp
using Soenneker.Maf.Pool.Mistral;
using Soenneker.Maf.Pool.Abstract;

await pool.AddMistral(
    poolId: "chat",
    key: "mistral-primary",
    modelId: "mistral-small-latest",
    apiKey: configuration["MISTRAL_API_KEY"]!,
    rpm: 60,
    instructions: "Answer concisely.",
    cancellationToken: cancellationToken);

(AIAgent? agent, IMafPoolEntry? entry) =
    await pool.GetAvailable("chat", cancellationToken);
```

The default endpoint is Mistral's OpenAI-compatible `https://api.mistral.ai/v1`. Supply `endpoint` when using another compatible deployment.

## What you get

- `MafPoolMistralExtension` — Provides Mistral-specific registration extensions for `IMafPool`, enabling integration via Microsoft Agent Framework (OpenAI-compatible API).

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `MafPoolMistralExtension.AddMistral(pool, poolId, key, modelId, apiKey, endpoint, rps, rpm, rpd, tokensPerDay, instructions, cancellationToken)` | Registers a Mistral model in the agent pool with optional rate/token limits. Uses Mistral's OpenAI-compatible API. | A task that completes when the mistral addition is complete. |
| `MafPoolMistralExtension.RemoveMistral(pool, poolId, key, cancellationToken)` | Unregisters a Mistral model from the agent pool and removes the associated cache entry. | True if the entry existed and was removed; false if it was not present. |

## Practical notes

- The agent is created lazily and reused until its entry is removed.
- Store the API key in a secret provider; the pool retains it in the entry options while the entry is registered.
- Omitted instructions default to `You are a helpful assistant.`
- Checkout consumes one request from the configured quota. `tokensPerDay` is not reconciled against actual provider token usage.
