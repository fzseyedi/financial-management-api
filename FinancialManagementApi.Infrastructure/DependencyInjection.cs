using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Infrastructure.Persistence;
using FinancialManagementApi.Infrastructure.Repositories;
using FinancialManagementApi.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialManagementApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}