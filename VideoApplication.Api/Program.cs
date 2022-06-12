using VideoApplication.Api;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    })
    .ConfigureHostConfiguration(h =>
    {
        h.AddJsonFile("appsettings.json", false, true);
        h.AddJsonFile("appsettings.Development.json", true, true);
    })
    .Build()
    .Run();
