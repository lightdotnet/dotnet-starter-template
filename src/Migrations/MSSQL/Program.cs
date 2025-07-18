﻿using Microsoft.Extensions.DependencyInjection;
using Monolith.Identity.Data;
using Monolith.Infrastructure.Data.SeedWork;
using MSSQL;

// set Environment
//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Live");

using var host = Host.CreateHostBuilder(args).Build();

using var scope = host.Services.CreateScope();

var serviceProvider = scope.ServiceProvider;

var identityInitialiser = serviceProvider.GetRequiredService<IdentityContextInitialiser>();

await identityInitialiser.InitialiseAsync();

await identityInitialiser.TrySeedAsync();

var catalogInitialiser = serviceProvider.GetRequiredService<CatalogContextInitialiser>();

await catalogInitialiser.InitialiseAsync();

await catalogInitialiser.TrySeedAsync();