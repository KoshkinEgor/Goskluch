
namespace Repo;

public class SmevOrder
{
    public int Id { get; set; }
    public string? SmevMessageId { get; set; }
    public string? OrderStatusId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
}