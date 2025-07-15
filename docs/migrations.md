# dotnet-ef

### Add migrations
```
dotnet ef migrations add CreateIdentitySchema --context AppIdentityDbContext --output-dir Identity
```
```
dotnet ef migrations add CreateCatalogSchema --context CatalogContext --output-dir Catalog
```

### Update database
```
dotnet ef database update --context CatalogContext
```

