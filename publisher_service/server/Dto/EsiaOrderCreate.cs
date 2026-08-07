

namespace Dto;

public class EsiaOrderCreate
{
    public string? ReceiverSnils { get; set; }
    public string? ReceiverOid { get; set; }
    public string SignatureType {get;set;}
    public string Description { get; set; }
     public IFormFileCollection? DocumentsPack { get; set; }
   

}