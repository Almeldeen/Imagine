using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CustomizationJobRepository : BaseRepository<CustomizationJob>, ICustomizationJobRepository
    {
        public CustomizationJobRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
