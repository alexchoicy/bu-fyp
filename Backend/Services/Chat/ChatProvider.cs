using Backend.Data;
using Backend.Models;
using Backend.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Chat;

public class ChatProvider
{
    private readonly AppDbContext _dbContext;
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly ILogger<ChatProvider> _logger;

    public ChatProvider(AppDbContext dbContext, IAIProviderFactory aiProviderFactory, ILogger<ChatProvider> logger)
    {
        _dbContext = dbContext;
        _aiProviderFactory = aiProviderFactory;
        _logger = logger;
    }

    public async Task<Guid> CreateRoomAsync(string userId)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        await _dbContext.Conversations.AddAsync(conversation);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created conversation {ConversationId} for user {UserId}", conversation.Id, userId);

        return conversation.Id;
    }

    public async Task<IReadOnlyList<Message>> GetChatHistoryAsync(Guid roomId, string userId)
    {
        var conversation = await _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == roomId && c.UserId == userId);

        if (conversation is null)
        {
            return Array.Empty<Message>();
        }

        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    public async Task<Guid> SendMessageToRoomAsync(Guid roomId, string content, string userId)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty", nameof(content));
        }

        var conversation = await _dbContext.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == roomId && c.UserId == userId);

        if (conversation is null)
        {
            throw new InvalidOperationException("Conversation not found or access denied");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = roomId,
            Content = content,
            Role = MessageRole.User,
            Status = MessageStatus.Complete,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Saved message {MessageId} to conversation {ConversationId}", message.Id, roomId);

        var pendingMessageId = await CreatePendingMessageAsync(roomId);

        _ = Task.Run(async () => await GenerateResponseAsync(pendingMessageId, roomId, userId));

        return pendingMessageId;
    }

    private async Task GenerateResponseAsync(Guid pendingMessageId, Guid roomId, string userId)
    {
        try
        {
            _logger.LogInformation("Starting background generation for message {MessageId}", pendingMessageId);

            var messages = await GetChatHistoryAsync(roomId, userId);
            
            var aiProvider = _aiProviderFactory.GetDefaultProvider();

            var response = await aiProvider.GenerateChatResponseAsync(messages.ToList());

            await UpdateMessageAsync(pendingMessageId, response.Last().Content.ToString(), MessageStatus.Complete);

            _logger.LogInformation("Completed generation for message {MessageId}", pendingMessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating response for message {MessageId}", pendingMessageId);
            await UpdateMessageAsync(pendingMessageId, string.Empty, MessageStatus.Failed);
        }
    }

    private async Task<Guid> CreatePendingMessageAsync(Guid roomId)
    {
        var pendingMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = roomId,
            Content = string.Empty,
            Role = MessageRole.Assistant,
            Status = MessageStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Messages.AddAsync(pendingMessage);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created pending message {MessageId} for conversation {ConversationId}", pendingMessage.Id, roomId);

        return pendingMessage.Id;
    }

    private async Task UpdateMessageAsync(Guid messageId, string content, MessageStatus status)
    {
        var message = await _dbContext.Messages.FindAsync(messageId);
        
        if (message is null)
        {
            throw new InvalidOperationException($"Message {messageId} not found");
        }

        message.Content = content;
        message.Status = status;

        _dbContext.Messages.Update(message);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated message {MessageId} with status {Status}", messageId, status);
    }

    public async Task<(MessageStatus Status, string Content)> GetMessageResultAsync(Guid roomId, Guid messageId, string userId)
    {
        var conversation = await _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == roomId && c.UserId == userId);

        if (conversation is null)
        {
            throw new InvalidOperationException("Conversation not found or access denied");
        }

        var message = conversation.Messages.FirstOrDefault(m => m.Id == messageId);

        if (message is null)
        {
            throw new InvalidOperationException("Message not found in conversation");
        }

        return (message.Status, message.Content);
    }
}