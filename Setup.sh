mkdir FinancialManagementApi
cd FinancialManagementApi
dotnet new sln -n FinancialManagementApi
-----------------------------------
dotnet new webapi -n FinancialManagementApi.Api
dotnet new classlib -n FinancialManagementApi.Application
dotnet new classlib -n FinancialManagementApi.Domain
dotnet new classlib -n FinancialManagementApi.Infrastructure
dotnet new xunit -n FinancialManagementApi.Tests
-----------------------------------
dotnet sln add FinancialManagementApi.Api/FinancialManagementApi.Api.csproj
dotnet sln add FinancialManagementApi.Application/FinancialManagementApi.Application.csproj
dotnet sln add FinancialManagementApi.Domain/FinancialManagementApi.Domain.csproj
dotnet sln add FinancialManagementApi.Infrastructure/FinancialManagementApi.Infrastructure.csproj
dotnet sln add FinancialManagementApi.Tests/FinancialManagementApi.Tests.csproj
-----------------------------------
dotnet add FinancialManagementApi.Application/FinancialManagementApi.Application.csproj reference FinancialManagementApi.Domain/FinancialManagementApi.Domain.csproj

dotnet add FinancialManagementApi.Infrastructure/FinancialManagementApi.Infrastructure.csproj reference FinancialManagementApi.Application/FinancialManagementApi.Application.csproj
dotnet add FinancialManagementApi.Infrastructure/FinancialManagementApi.Infrastructure.csproj reference FinancialManagementApi.Domain/FinancialManagementApi.Domain.csproj

dotnet add FinancialManagementApi.Api/FinancialManagementApi.Api.csproj reference FinancialManagementApi.Application/FinancialManagementApi.Application.csproj
dotnet add FinancialManagementApi.Api/FinancialManagementApi.Api.csproj reference FinancialManagementApi.Infrastructure/FinancialManagementApi.Infrastructure.csproj

dotnet add FinancialManagementApi.Tests/FinancialManagementApi.Tests.csproj reference FinancialManagementApi.Application/FinancialManagementApi.Application.csproj
dotnet add FinancialManagementApi.Tests/FinancialManagementApi.Tests.csproj reference FinancialManagementApi.Domain/FinancialManagementApi.Domain.csproj
-----------------------------------
dotnet add FinancialManagementApi.Api package Swashbuckle.AspNetCore