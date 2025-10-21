using Microsoft.EntityFrameworkCore;
using TechStoreEll.Core.Entities;
using TechStoreEll.Core.Infrastructure.Data;
using TechStoreEll.Core.Models;

namespace TechStoreEll.Core.Services;

public class AddressService(AppDbContext context)
{
    public async Task<List<Address>> GetUserAddressesAsync(int userId)
    {
        return await context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerAsync(int userId)
    {
        return await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
    }

    public async Task<AddressFormModel?> GetAddressFormAsync(int id, int userId)
    {
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return null;

        return new AddressFormModel
        {
            Id = address.Id,
            Label = address.Label,
            Country = address.Country,
            Region = address.Region,
            City = address.City,
            Street = address.Street,
            House = address.House,
            Apartment = address.Apartment,
            Postcode = address.Postcode
        };
    }

    public async Task SaveAddressAsync(AddressFormModel model, int userId)
    {
        Console.WriteLine(userId);
        await context.SetCurrentUserAsync(userId);
        
        if (model.Id.HasValue)
        {
            var existing = await context.Addresses
                .FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == userId);
            if (existing == null) throw new InvalidOperationException("Address not found");

            existing.Label = model.Label;
            existing.Country = model.Country;
            existing.Region = model.Region;
            existing.City = model.City;
            existing.Street = model.Street;
            existing.House = model.House;
            existing.Apartment = model.Apartment;
            existing.Postcode = model.Postcode;
        }
        else
        {
            var newAddress = new Address
            {
                UserId = userId,
                Label = model.Label,
                Country = model.Country,
                Region = model.Region,
                City = model.City,
                Street = model.Street,
                House = model.House,
                Apartment = model.Apartment,
                Postcode = model.Postcode,
                CreatedAt = DateTime.UtcNow
            };
            context.Addresses.Add(newAddress);
        }

        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAddressAsync(int id, int userId)
    {
        await context.SetCurrentUserAsync(userId);
        
        var address = await context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (address == null) return false;

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
        if (customer?.ShippingAddressId == id || customer?.BillingAddressId == id)
            throw new InvalidOperationException("Address in use by default profile");

        context.Addresses.Remove(address);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task SetShippingAddressAsync(int userId, int addressId)
    {
        await context.SetCurrentUserAsync(userId);
        
        var address = await context.Addresses.FindAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("Invalid address");

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
        if (customer == null)
        {
            customer = new Customer { Id = userId, ShippingAddressId = addressId };
            context.Customers.Add(customer);
        }
        else
        {
            customer.ShippingAddressId = addressId;
        }

        await context.SaveChangesAsync();
    }

    public async Task SetBillingAddressAsync(int userId, int addressId)
    {
        await context.SetCurrentUserAsync(userId);
        
        var address = await context.Addresses.FindAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new InvalidOperationException("Invalid address");

        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == userId);
        if (customer == null)
        {
            customer = new Customer { Id = userId, BillingAddressId = addressId };
            context.Customers.Add(customer);
        }
        else
        {
            customer.BillingAddressId = addressId;
        }

        await context.SaveChangesAsync();
    }
}
