using logistic_web.infrastructure.Models;

namespace logistic_web.infrastructure.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
    }

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(LogisticContext context) : base(context)
        {
        }
    }
}
