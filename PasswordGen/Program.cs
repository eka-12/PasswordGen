using PasswordGen.Components;
using PasswordGen.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<PasswordGen.Services.LeakCheckerService>();

builder.Services.AddScoped<HistoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Маршрутизация: сначала API, потом Blazor
app.MapControllers();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();