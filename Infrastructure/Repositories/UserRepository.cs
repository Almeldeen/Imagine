using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
