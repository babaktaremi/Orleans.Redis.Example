namespace Abstractions;


[GenerateSerializer,Immutable]
public class MessageModel
{
    [Id(0)]
    public string MessageContent { get; set; } = string.Empty;
    [Id(1)]
    public DateTime MessageDate { get; set; }
}