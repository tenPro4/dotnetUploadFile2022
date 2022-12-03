using DotnetUploadFile2022.BackgroundTasks;
using DotnetUploadFile2022.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<FileManager>();

builder.Services.AddSingleton<IBackgroundQueue, BackgroundQueue>();

builder.Services.AddHostedService<QueueService>();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(o =>
{
    o.MapDefaultControllerRoute();
});

app.Run();
