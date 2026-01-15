using System.Security.Claims;
using Backend.Services.Chat;
using Backend.Dtos.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Services.AI;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    //not doing websocket here, I try to use SSE later, i dunno how to. Http endpoints is good enough for chatbot.
    public class ChatController : ControllerBase
    {
        private readonly ChatProvider _chatProvider;
        private readonly IAIProviderFactory _aiProviderFactory;

        public ChatController(ChatProvider chatProvider, IAIProviderFactory aiProviderFactory)
        {
            _chatProvider = chatProvider;
            _aiProviderFactory = aiProviderFactory;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            //get list of chat history
            return Ok("Chat endpoint is working");
        }

        [HttpGet("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TestChat([FromQuery] string? prompt = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var aiProvider = _aiProviderFactory.GetDefaultProvider();

            var chatHistory = new List<Message>
            {
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = Guid.Empty,
                    Content = string.IsNullOrWhiteSpace(prompt)
                        ? "Hi! Give me a one-sentence tip for picking courses this term."
                        : prompt,
                    Role = MessageRole.User,
                    Status = MessageStatus.Complete,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var response = await aiProvider.GenerateChatResponseAsync(chatHistory);
            return Ok(new { response });
        }


        //ya just Create a simple room first
        [HttpPost]
        [ProducesResponseType(typeof(CreateRoomResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> NewChat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var roomId = await _chatProvider.CreateRoomAsync(userId);
            return Ok(new { roomId = roomId.ToString() });
        }

        [HttpGet("{roomId}")]
        [ProducesResponseType(typeof(ChatHistoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChatHistory(string roomId)
        {
            if (!Guid.TryParse(roomId, out var guid))
            {
                return BadRequest("Invalid room ID");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var history = await _chatProvider.GetChatHistoryAsync(guid, userId);
            return Ok(history);
        }

        //request the generation and return the generated message id
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(SendMessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage(string id, [FromBody] SendMessageRequestDto request)
        {
            if (!Guid.TryParse(id, out var roomId))
            {
                return BadRequest("Invalid room ID");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var generatedMessageId = await _chatProvider.SendMessageToRoomAsync(roomId, request.Message, userId);
            return Ok(new { generatedId = generatedMessageId.ToString() });
        }

        //SEE later
        //TODO: SEE LATER, POLLING FOR NOW
        [HttpGet("{roomId}/result/{messageId}")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGeneratedResult(string roomId, string messageId)
        {
            if (!Guid.TryParse(roomId, out var roomGuid) || !Guid.TryParse(messageId, out var msgGuid))
            {
                return BadRequest("Invalid room ID or message ID");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                var messageResponse = await _chatProvider.GetMessageResultAsync(roomGuid, msgGuid, userId);
                return Ok(messageResponse);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }

}
