using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class UserColorSuggestionRepository : BaseRepository<UserColorSuggestion>, IUserColorSuggestionRepository
    {
        public UserColorSuggestionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
