var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<QuotationSystem.DbHelper>();
builder.Services.AddSingleton<QuotationSystem.Services.DbInitializer>();
builder.Services.AddScoped<QuotationSystem.Services.ItemSearchService>();

var app = builder.Build();

// Ensure DB tables/triggers are created and validated
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetService<QuotationSystem.Services.DbInitializer>();
    if (dbInit != null)
    {
        try
        {
            dbInit.Initialize();
            Console.WriteLine("[DB INIT] Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB INIT ERROR] {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("[DB INIT ERROR] DbInitializer service not found.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
