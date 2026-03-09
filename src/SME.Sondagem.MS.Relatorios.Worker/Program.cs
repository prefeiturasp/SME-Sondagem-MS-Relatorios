using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.IoC;
using SME.Sondagem.MS.Relatorios.Worker.Menssageria;
using System.Reflection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

ConfigureServices.ConfigurarConexoes(builder.Services, builder.Configuration);
RegistraDependencias.Registrar(builder.Services, builder.Configuration);

builder.Services.AddSingleton<IRabbitMqSetupService, RabbitMqSetupService>();
builder.Services.AddSingleton<IRabbitMqMessageProcessor, RabbitMqMessageProcessor>();

ConfigureServices.ConfigurarServicos(builder.Services, builder.Configuration);

builder.Services.AddHostedService<RabbitMqConsumerService>();

IHost host = builder.Build();
await host.RunAsync();

