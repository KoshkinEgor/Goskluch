

using Microsoft.Net.Http.Headers;

namespace Repo;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Role { get; set; } = "user";
    public string Login { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public ICollection<Order> Orders { get; set; }


}

