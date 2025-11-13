using Eclipse.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eclipse.Controllers
{
    
    // Define a rota-base do controller como /api/chat e ativa convenções de API (validação, binding, etc.)
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        // Serviço que encapsula a chamada HTTP para a OpenAI (injeção de dependência).
        private readonly ChatAiServiceHttp _chat;

        // O container do ASP.NET Core injeta ChatIaServiceHttp automaticamente (registrado no Program.cs).
        public ChatController(ChatAiServiceHttp chat) => _chat = chat;

        // DTO usado para desserializar o JSON de entrada: { "message": "..." }
        public class ChatRequest { public string? Message { get; set; } }

        // Endpoint POST em /api/chat/send que recebe a pergunta e retorna a resposta da IA.
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ChatRequest req, CancellationToken ct)
        {
            // Validação simples: evita chamadas vazias.
            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Mensagem vazia.");

            // Encaminha a pergunta para o serviço de IA; CancellationToken permite cancelar se o cliente abortar.
            var reply = await _chat.PerguntarAsync(req.Message!, ct);

            // Retorna JSON no padrão { reply: "..." }
            Console.WriteLine($"Resposta final enviada ao cliente: {reply}");

            return Ok(new { reply });
        }
    }
}
