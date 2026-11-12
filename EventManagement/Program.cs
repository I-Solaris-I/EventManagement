using EventManagement.Context;
using EventManagement.Context.DataAccess;
using EventManagement.Context.Interfaces;
using EventManagement.Filters;
using EventManagement.Middlewares;
using EventManagement.Models;
using EventManagement.Services;
using EventManagement.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(options =>
{
    //Включаем проверку подключаемых зависимостей на цикличность, неверное использование жизненного цикла
    if (builder.Environment.IsDevelopment())
    {
        options.ValidateOnBuild = true;
        options.ValidateScopes = true;
    }
});
// Add services to the container.
// Толлько для минимальных API
builder.Services.AddEndpointsApiExplorer();
// Стандартизация ответов
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "EventManagement API", Version = "v1" });
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Event).Assembly.GetName().Name}.xml"));
});

builder.Services.AddControllers(options =>
{
    //Глобально подключаем фильтр бизнес-исключений
    options.Filters.Add<BusinessExceptionFilter>();
})
    .ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = false;
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork>(services => services.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IAsyncEventRepository, EventRepository>();
builder.Services.AddScoped<IValidator<CreateEventDTO>, CreateEventDTOValidation>();
builder.Services.AddScoped<IValidator<UpdateEventDTO>, UpdateEventDTOValidation>();
builder.Services.AddScoped<IValidator<EventFilterDTO>, EventFilterDTOValidation>();
builder.Services.AddScoped<IEventService, EventService>();


builder.Services.AddScoped<IAsyncBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddHostedService<BookingBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseGlobalExceptionHandling();
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
}
app.UseRouting();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
// Не удалять! Без этого тесты не будут работать корректно
/// <summary>
/// Program
/// </summary>
public partial class Program { }

