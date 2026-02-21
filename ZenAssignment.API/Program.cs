using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using ZenAssignment.API.Interface;
using ZenAssignment.API.Repository;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Get key vault URI from config.
string keyVaultUrl = builder.Configuration["AzureKeyVault:Url"];

//Create secretClient with Default AzureCredential (work with Managed Identity or local dev login)
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
//Register SecretClient in DI.
builder.Services.AddSingleton(secretClient);


builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var secretClient = sp.GetRequiredService<SecretClient>();
    var secretName = builder.Configuration["AzureBlobStorage:BlobStorageConnectionString"];
    KeyVaultSecret secret = secretClient.GetSecret(secretName);

    string conn = secret.Value ?? throw new InvalidOperationException($"Secret '{secretName}' has no value.");
    if (!conn.Contains("=")) // simple validation for connection-string format
        throw new FormatException($"Secret '{secretName}' does not contain a storage connection string. Value: '{conn}'");

    return new BlobServiceClient(conn);
});

// get secret name from configuration (which contains the Key Vault secret name)
var aiSecretName = builder.Configuration["ApplicationInsights:ConnectionString"];
KeyVaultSecret aiSecret = secretClient.GetSecret(aiSecretName);
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = aiSecret.Value;
});
builder.Services.AddScoped<IImageRepo, ImageRepo>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
