

namespace Repo;

public class Order
{
    public int Id { get; set; }
    public int EpguOrderId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ReceiverIdType { get; set; }
    public string ReceiverId { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; }
    public int StatusCode {get;set;}
    public ICollection<Document> Documents { get; set; }
    public User User { get; set; }

}