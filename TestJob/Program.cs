using FluentValidation;
using TestJob;
using TestJob.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IValidator<HtmlAnalyzeData>, HtmlAnalyzeDataValidator>();
builder.Services.AddScoped<DbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TestJob");
        options.RoutePrefix = "api/swagger";
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
