using backend.Database;
using backend.Models;
using backend.Utils;

namespace backend.Services;

public class UserService
{
    private ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateUserAsync(string username, string password)
    {
        User user = new User
        {
            Username = username,
            Password = PasswordHelper.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}