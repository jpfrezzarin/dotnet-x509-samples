using System.Security.Cryptography.X509Certificates;
using Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost.ConfigureKestrel(opts => 
{
    opts.ListenAnyIP(5064, listenOpts => 
    {
        var cert = CertUtil.GetCertificate(".NET Test", StoreName.My, StoreLocation.CurrentUser) ??
                   throw new Exception("Certificate \".NET Test\" not found");
        listenOpts.UseHttps(cert);
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
