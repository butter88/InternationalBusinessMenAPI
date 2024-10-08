using InternationalBusinessMen.Services;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Inicializar log4net al iniciar la aplicaci�n
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Configuraci�n de Servicios
// Aqu� registramos nuestros servicios personalizados y configuramos el middleware

// A�adir los controladores para manejar los endpoints
builder.Services.AddControllers();

// A�adir el explorador de API y Swagger para documentaci�n autom�tica
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar los servicios personalizados para inyecci�n de dependencias
builder.Services.AddSingleton<IRateService, RateService>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();

var app = builder.Build();

// Configurar el pipeline de la aplicaci�n
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting(); // Usar el enrutamiento para dirigir las solicitudes a los controladores

app.UseAuthorization(); // Configuraci�n de autorizaci�n, aunque no usamos autenticaci�n en este caso

// Mapear los controladores al pipeline
app.MapControllers();

// Ejecutar la aplicaci�n
app.Run();
