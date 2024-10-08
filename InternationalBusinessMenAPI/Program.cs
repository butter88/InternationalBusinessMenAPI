using InternationalBusinessMen.Services;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Inicializar log4net al iniciar la aplicación
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Configuración de Servicios
// Aquí registramos nuestros servicios personalizados y configuramos el middleware

// Añadir los controladores para manejar los endpoints
builder.Services.AddControllers();

// Añadir el explorador de API y Swagger para documentación automática
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar los servicios personalizados para inyección de dependencias
builder.Services.AddSingleton<IRateService, RateService>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();

var app = builder.Build();

// Configurar el pipeline de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting(); // Usar el enrutamiento para dirigir las solicitudes a los controladores

app.UseAuthorization(); // Configuración de autorización, aunque no usamos autenticación en este caso

// Mapear los controladores al pipeline
app.MapControllers();

// Ejecutar la aplicación
app.Run();
