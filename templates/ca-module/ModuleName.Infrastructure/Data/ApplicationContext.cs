﻿namespace ModuleName.Infrastructure.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
}
