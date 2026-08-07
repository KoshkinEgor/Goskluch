

namespace Repo;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; }
    public string? ReceiverSnils { get; set; }
    public string? ReceiverOid { get; set; }
    public int UserId { get; set; }
    public bool IsDeadRequest {get;set;} = true;
    public string Description { get; set; }
    public ICollection<Document> Documents { get; set; }
    public User User { get; set; }
    public EpguOrder? EpguOrder {get;set;}
    public SmevOrder? SmevOrder {get;set;}

}