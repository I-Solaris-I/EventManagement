using EventManagement.Context;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using FluentValidation;

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
// Толлько для минимальных API
builder.Services.AddEndpointsApiExplorer();
// Стандартизация ответов
builder.Services.AddProblemDetails();

// Add services to the container.

builder.Services.AddControllers()
.ConfigureApiBehaviorOptions(options =>
{
    //Отключаем стандартную проверку валидации модели
    options.SuppressModelStateInvalidFilter = true;
});



#warning Для данных в памяти используем Singleton(для многопоточного доступа в InMemoryEvents используется System/Threading.Lock, однако для других реализаций будем использовать Scoped
builder.Services.AddSingleton<IRepository<Event>, InMemoryEvents>();
builder.Services.AddTransient<IValidator<CreateUpdateEventDTO>, CreateUpdateEventDTOValidation>();


var app = builder.Build();

// Configure the HTTP request pipeline.


if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
}
app.UseRouting();
app.MapControllers();

app.Run();
// Не удалять! Без этого тесты не будут работать корректно
public partial class Program { }

