

namespace Dto;

public class OrderCreate
{
    public string ReceiverIdType { get; set; }
    public string ReceiverId { get; set; }
    public string SignatureType {get;set;}
    public string Description { get; set; }
     public IFormFileCollection? DocumentsPack { get; set; }
   

}