

namespace Dto;

public class SmevOrderCreate
{
    public string? ReceiverSnils { get; set; }
    public string Description { get; set; }
    public IFormFileCollection? DocumentsPack { get; set; }


}