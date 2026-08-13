using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.RailLogisticsApi;
using RailLinkBackEnd.Supabase;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCors";

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseConnection")));
builder.Services.AddHttpClient<RailLogisticsApiService>(); //철도api

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
    app.MapOpenApi();

//app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();
//
app.Run();
