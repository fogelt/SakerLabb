using SakerLabb.Web.Components;
using SakerLabb.Web.Data;
using SakerLabb.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("SakerLabb", LogLevel.Information);

builder.Services.AddRazorComponents();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ImportService>();

builder.Services.AddSingleton<Db>();
builder.Services.AddScoped<TicketRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<FileService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5080", "https://localhost:5080")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.Services.GetRequiredService<Db>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("Server");
    await next();
});

app.UseCors();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>();

app.Run();