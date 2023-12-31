﻿using ApiCaller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", true);
builder.Services.AddHostedService<SenderService>();

var host = builder.Build();
host.Run();
