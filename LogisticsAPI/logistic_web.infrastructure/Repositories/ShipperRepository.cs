using logistic_web.infrastructure.Models;

namespace logistic_web.infrastructure.Repositories
{
    public interface IShipperRepository : IRepository<Shipper>
    {
    }

    public class ShipperRepository : Repository<Shipper>, IShipperRepository
    {
        public ShipperRepository(LogisticContext context) : base(context)
        {
        }
    }
}

