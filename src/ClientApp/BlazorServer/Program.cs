using Blazored.LocalStorage;
using Light.Blazor;
using Monolith.Blazor;
using Monolith.Blazor.Components;
using Monolith.Blazor.Components.Account;
using Monolith.Blazor.Services;
using Monolith.Blazor.Services.TokenStorage;
using Monolith.HttpApi.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorComponents(builder.Configuration);

//builder.Services.AddScoped<ISignInManager, SignInManager>();
/*
builder.Services
    .AddAuthentication("jwt")
    .AddCookie("jwt", options =>
    {
        //options.Cookie.Name = "mini-session";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(6);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/access-denied";
    });
*/
//builder.Services.AddScoped<IStorageService, TokenCookieStorage>();
//builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<ITokenProvider, JwtAuthenticationStateProvider>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IStorageService, StorageService>();

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<IClientCurrentUser, CurrentUser>();

//builder.Services.AddScoped<ISignInManager, SignInManager>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

//app.UseSession();

//app.UseAuthentication(); // must be before UseAuthorization
//app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
