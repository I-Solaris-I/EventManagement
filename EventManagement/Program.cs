using EventManagement.Context;
using EventManagement.Context.Interfaces;
using EventManagement.Filters;
using EventManagement.Models;
using EventManagement.Services;
using EventManagement.Services.Interfaces;
using FluentValidation;
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
    //Глобально подклбчаем фильтр 
    options.Filters.Add<BusinessExceptionFilter>();
})
    .ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = false;
});



#warning Для данных в памяти используем Singleton(для многопоточного доступа в InMemoryEvents используется System/Threading.Lock, однако для других реализаций будем использовать Scoped
builder.Services.AddSingleton<IRepository<Event>, InMemoryEvents>();
builder.Services.AddTransient<IValidator<CreateUpdateEventDTO>, CreateUpdateEventDTOValidation>();
builder.Services.AddScoped<IEventService, EventService>();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
}
app.UseRouting();
app.MapControllers();

app.Run();
// Не удалять! Без этого тесты не будут работать корректно
/// <summary>
/// Program
/// </summary>
public partial class Program { }

