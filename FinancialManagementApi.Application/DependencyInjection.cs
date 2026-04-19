using FinancialManagementApi.Application.Customers.Commands.ActivateCustomer;
using FinancialManagementApi.Application.Customers.Commands.CreateCustomer;
using FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;
using FinancialManagementApi.Application.Customers.Commands.DeleteCustomer;
using FinancialManagementApi.Application.Customers.Commands.UpdateCustomer;
using FinancialManagementApi.Application.Customers.Queries;
using FinancialManagementApi.Application.Invoices.Commands;
using FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;
using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FinancialManagementApi.Application.Invoices.Queries;
using FinancialManagementApi.Application.Payments.Commands.RecordPayment;
using FinancialManagementApi.Application.Products.Commands.ActivateProduct;
using FinancialManagementApi.Application.Products.Commands.CreateProdcut;
using FinancialManagementApi.Application.Products.Commands.DeactivateProduct;
using FinancialManagementApi.Application.Products.Commands.UpdateProduct;
using FinancialManagementApi.Application.Products.Queries;
using FinancialManagementApi.Application.Reports.Queries;
using FinancialManagementApi.Application.Reports.Queries.GetUnpaidInvoices;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FinancialManagementApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<UpdateCustomerHandler>();
        services.AddScoped<DeactivateCustomerHandler>();
        services.AddScoped<ActivateCustomerHandler>();
        services.AddScoped<DeleteCustomerHandler>();
        services.AddScoped<GetCustomerByIdHandler>();
        services.AddScoped<GetAllCustomersHandler>();

        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<DeactivateProductHandler>();
        services.AddScoped<ActivateProductHandler>();
        services.AddScoped<GetProductByIdHandler>();
        services.AddScoped<GetAllProductsHandler>();

        services.AddScoped<CreateInvoiceHandler>();
        services.AddScoped<UpdateInvoiceHandler>();
        services.AddScoped<IssueInvoiceHandler>();
        services.AddScoped<GetInvoiceByIdHandler>();
        services.AddScoped<GetAllInvoicesHandler>();

        services.AddScoped<RecordPaymentHandler>();
        services.AddScoped<GetCustomerBalanceHandler>();
        services.AddScoped<GetUnpaidInvoicesHandler>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}