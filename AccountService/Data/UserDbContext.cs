using AccountService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data;

public class UserDbContext(
    DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}