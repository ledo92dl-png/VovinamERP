using VovinamERP.Application.DependencyInjection;
using VovinamERP.Infrastructure.DependencyInjection;
using VovinamERP.Api.Services;
using VovinamERP.Api.ExceptionHandling;
using VovinamERP.Infrastructure.Persistence.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IQrCodeImageService, QrCodeImageService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRepositories();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevelopment", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

await DatabaseBootstrapper.BootstrapAsync(app.Services);

app.UseHttpsRedirection();
app.UseCors("LocalDevelopment");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "VovinamERP API",
    status = "Running",
    version = "0.44.0"
}));

app.Run();