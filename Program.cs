using AzStorageAccountPrivareEndP;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();

//*****************************************************************


//##################################################################


var azureCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    TenantId = builder.Configuration.GetSection("TenantId").Value,
    ManagedIdentityClientId = builder.Configuration.GetSection("ManagedIdentityClientId").Value,
    ExcludeEnvironmentCredential = true,
    ExcludeWorkloadIdentityCredential = true,
});

//builder.Configuration.AddAzureKeyVault(keyVaultURL, azureCredentials);

var dbcs = builder.Configuration.GetSection("dbcs").Value;
builder.Services.AddDbContext<AppDatabaseContext>(item => item.UseSqlServer(dbcs), ServiceLifetime.Scoped);


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    // Apply pending migrations to the database
    var db = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
    db.Database.Migrate();
}


//##################################################################


//var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=GetFile}");

app.Run();
