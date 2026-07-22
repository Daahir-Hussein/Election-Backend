using ElectionManagement.API.Data;

var builder = WebApplication.CreateBuilder(args);

DbConnection.Configure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow frontend URLs
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://election-ms-project.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseRouting();

// CORS must be before authorization and controllers
app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();