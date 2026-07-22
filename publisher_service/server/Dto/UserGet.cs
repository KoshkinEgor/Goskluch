
namespace Dto;

public class UserGet
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Role { get; set; } = "user";
    public string Login { get; set; } = String.Empty;
    // public DateTime? DeletedAt { get; set; }

    public UserGet(Repo.User user){

        this.Id = user.Id;
        this.Login = user.Login;
        this.Role = user.Role;
        this.Name = user.Name;
        // this.DeletedAt = user.DeletedAt;

    }

}

