# dotnet-ef

### Update Tools
```bash
dotnet tool install --global dotnet-ef
```

### Add migrations
```
dotnet ef migrations add CreateIdentitySchema --context IdentityDbContext --output-dir Identity
```
```
dotnet ef migrations add CreateNotificationSchema --context NotificationDbContext --output-dir Notifications
```

Run from `src/Migrations/{Sqlite,PostgreSQL,MSSQL}` — pick the provider directory matching your target database; each module gets its own `--output-dir` under it. `--context <ModuleName>DbContext` selects which module's schema to migrate.

### Update database
```
dotnet ef database update --context NotificationDbContext
```

