using Core.Entities;
using Core.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
