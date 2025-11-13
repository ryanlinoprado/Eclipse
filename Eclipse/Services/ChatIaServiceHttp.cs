using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Eclipse.Services
{

    /// Serviço responsável por enviar perguntas ao endpoint de Chat Completions da OpenAI
    /// e devolver a resposta como string. Ele esconde detalhes de rede (headers, retries, parse).

    public class ChatAiServiceHttp
    {
        private readonly IHttpClientFactory _factory;           // Fábrica de HttpClient (boa prática em ASP.NET Core)
        private readonly ILogger<ChatAiServiceHttp> _logger;    // Log estruturado para diagnóstico

        public ChatAiServiceHttp(IHttpClientFactory factory, ILogger<ChatAiServiceHttp> logger)
        {
            _factory = factory;
            _logger = logger;
        }


        /// Envia a "pergunta" para o modelo e retorna o texto de resposta.
        /// Implementa retry/backoff para 429/5xx e mensagens de erro amigáveis.

        public async Task<string> PerguntarAsync(string pergunta, CancellationToken ct = default)
        {
            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "system", content = "Você é responsável por auxiliar nos problemas de TI de uma empresa chamada Eclipse, forneça respostas e se necessário, peça ao usuário para que ele abra um chamado. Responda em português, de forma objetiva e sempre auxilie ao máximo. Se perguntarem sobre problemas relacionados a computador, informe sobre prováveis problemas e deixe claro que eles podem abrir um chamado. Se não souber, diga que não tem certeza." },
            new { role = "user", content = pergunta }
        },
                temperature = 0.3
            };

            var http = _factory.CreateClient("OpenAI");
            const int maxRetries = 3;
            var rnd = new Random();

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var resp = await http.PostAsJsonAsync("chat/completions", payload, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                // 🔎 Log completo (útil para depuração)
                _logger.LogInformation("Resposta da OpenAI: {Body}", body);

                if (resp.IsSuccessStatusCode)
                {
                    try
                    {
                        using var json = JsonDocument.Parse(body);
                        var root = json.RootElement;

                        string? content = null;

                        // Detecta automaticamente o formato da resposta
                        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var choice = choices[0];

                            if (choice.TryGetProperty("message", out var message) &&
                                message.TryGetProperty("content", out var msgContent))
                            {
                                content = msgContent.GetString();
                            }
                            else if (choice.TryGetProperty("text", out var textContent))
                            {
                                content = textContent.GetString();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            _logger.LogWarning("Resposta da IA vazia ou inesperada. Corpo: {Body}", body);
                            return "Não consegui gerar uma resposta agora. Tente reformular a pergunta.";
                        }

                        return content.Trim();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao interpretar resposta da OpenAI: {Body}", body);
                        return "Ocorreu um erro ao interpretar a resposta da IA.";
                    }
                }

                // Trata 429 / 5xx com retry/backoff
                if (resp.StatusCode == (HttpStatusCode)429 || (int)resp.StatusCode >= 500)
                {
                    TimeSpan delay;

                    if (resp.Headers.TryGetValues("Retry-After", out var vals) &&
                        int.TryParse(vals.FirstOrDefault(), out var seconds) && seconds >= 0)
                    {
                        delay = TimeSpan.FromSeconds(seconds);
                    }
                    else
                    {
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                        var jitter = TimeSpan.FromMilliseconds(rnd.Next(0, 250));
                        delay = baseDelay + jitter;
                    }

                    _logger.LogWarning(
                        "OpenAI {Status}. Retentando em {Delay}s (tentativa {Attempt}/{Max}). Corpo: {Body}",
                        (int)resp.StatusCode, delay.TotalSeconds, attempt, maxRetries, body
                    );

                    if (attempt == maxRetries)
                    {
                        var msg = ExtrairMensagemErro(body) ?? "Limite de requisições atingido. Tente novamente mais tarde.";
                        return $"Falha (HTTP {(int)resp.StatusCode}): {msg}";
                    }

                    await Task.Delay(delay, ct);
                    continue;
                }

                // Erros não transitórios
                _logger.LogError("OpenAI falhou: {Status} - {Body}", (int)resp.StatusCode, body);
                var erro = ExtrairMensagemErro(body) ?? body;
                return $"Falha (HTTP {(int)resp.StatusCode}): {erro}";
            }

            return "Não consegui gerar uma resposta agora.";
        }


        /// Tenta extrair "error.message" do JSON de erro retornado pela API para exibição amigável.

        private static string? ExtrairMensagemErro(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var e))
                    return e.TryGetProperty("message", out var m) ? m.GetString() : e.ToString();
            }
            catch
            {
                // Ignora falhas de parse — devolve null para o chamador usar fallback.
            }
            return null;
        }
    }
}