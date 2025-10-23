using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;

namespace logistic_web.application.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponse?> GetCustomerByIdAsync(int customerId);
        Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync();
        Task<IEnumerable<CustomerResponse>> SearchCustomerByNameAsync(string name);
        Task<bool> CreateCustomerAsync(CreateCustomerRequest model);
        Task<bool> UpdateCustomerAsync(int customerId, UpdateCustomerRequest model);
        Task<bool> DeleteCustomerAsync(int customerId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IUnitOfWork unitOfWork,
            ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CustomerResponse?> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return null;
                }

                return new CustomerResponse
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    PersonInCharge = customer.PersonInCharge
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer: {CustomerId}", customerId);
                return null;
            }
        }

        public async Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.CustomerRepository.GetAllAsync();
                return customers.Select(c => new CustomerResponse
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    PersonInCharge = c.PersonInCharge
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                return new List<CustomerResponse>();
            }
        }

        public async Task<IEnumerable<CustomerResponse>> SearchCustomerByNameAsync(string name)
        {
            try
            {
                var customers = await _unitOfWork.CustomerRepository.FindAsync(c => c.CustomerName.Contains(name));
                return customers.Select(c => new CustomerResponse
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    PersonInCharge = c.PersonInCharge
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers by name: {Name}", name);
                return new List<CustomerResponse>();
            }
        }

        public async Task<bool> CreateCustomerAsync(CreateCustomerRequest model)
        {
            try
            {
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    PersonInCharge = model.PersonInCharge
                };

                await _unitOfWork.CustomerRepository.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer created successfully: {CustomerName}", customer.CustomerName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerName}", model.CustomerName);
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(int customerId, UpdateCustomerRequest model)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found for update: {CustomerId}", customerId);
                    return false;
                }

                customer.CustomerName = model.CustomerName;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
                customer.PersonInCharge = model.PersonInCharge;

                _unitOfWork.CustomerRepository.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer updated successfully: {CustomerId}", customerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found for deletion: {CustomerId}", customerId);
                    return false;
                }

                _unitOfWork.CustomerRepository.Remove(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Customer deleted successfully: {CustomerId}", customerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
                return false;
            }
        }
    }
}
