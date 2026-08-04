using ABC_Inc_Project_CLD7112.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IQueueStorageService, QueueStorageService>();

// External APIs used to source mock data for the Development-only seeding feature.
builder.Services.AddHttpClient("RandomUser", client =>
{
    client.BaseAddress = new Uri("https://randomuser.me/");
});
builder.Services.AddHttpClient("DummyJson", client =>
{
    client.BaseAddress = new Uri("https://dummyjson.com/");
});
builder.Services.AddScoped<ISeedDataService, SeedDataService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
