using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;

namespace logistic_web.infrastructure.Unitofwork
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        ICargolistRepository CargolistRepository { get; }
        IUserRepository UserRepository { get; }
        IRoleRepository RoleRepository { get; }
        IUserRoleRepository UserRoleRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IShipperRepository ShipperRepository { get; }
        ITrackingRepository TrackingRepository { get; }
        
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task<int> SaveChangesAsync();
        Task RollBackTransactionAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly LogisticContext _context;
        private ICargolistRepository? _cargolistRepository;
        private IUserRepository? _userRepository;
        private IRoleRepository? _roleRepository;
        private IUserRoleRepository? _userRoleRepository;
        private ICustomerRepository? _customerRepository;
        private IShipperRepository? _shipperRepository;
        private ITrackingRepository? _trackingRepository;

        public UnitOfWork(LogisticContext context)
        {
            _context = context;
        }

        public ICargolistRepository CargolistRepository => 
            _cargolistRepository ??= new CargolistRepository(_context);

        public IUserRepository UserRepository => 
            _userRepository ??= new UserRepository(_context);

        public IRoleRepository RoleRepository => 
            _roleRepository ??= new RoleRepository(_context);

        public IUserRoleRepository UserRoleRepository => 
            _userRoleRepository ??= new UserRoleRepository(_context);

        public ICustomerRepository CustomerRepository => 
            _customerRepository ??= new CustomerRepository(_context);

        public IShipperRepository ShipperRepository => 
            _shipperRepository ??= new ShipperRepository(_context);

        public ITrackingRepository TrackingRepository => 
            _trackingRepository ??= new TrackingRepository(_context);
    //2 phương thức sử dụng cho LinQ
    public Task<int> SaveChanges()
    {
        return _context.SaveChangesAsync();
    }
      public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    public void Dispose()
    {
        _context?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }


    //3 phương thức bên dưới sử dụng cho SQLRaw
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();

    }

        public async Task RollBackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
    }
}