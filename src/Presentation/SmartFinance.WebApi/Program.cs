using SmartFinance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();      // controller desteği
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();                   // controller route'larını bağla

app.Run();