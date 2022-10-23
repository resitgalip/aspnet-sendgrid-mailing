using aspnet_sendgrid_mailing;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<SendMail>();
    })
    .Build();

await host.RunAsync();
