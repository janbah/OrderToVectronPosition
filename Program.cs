using System.Net.Http.Headers;
using Order2VPos.Core.Common;
using OrderToVectronPosition.IOneApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IIoneClient, IoneClient>();
builder.Services.AddHttpClient<IIoneClient>(client =>
{
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Bearer",
        AppSettings.Default.IoneApiToken);
    client.DefaultRequestHeaders.Add("Identifier", AppSettings.Default.IoneApiIdentifier);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    string baseAddress = AppSettings.Default.ApiBaseAddress;
    if (!baseAddress.EndsWith("/"))
        baseAddress += "/";
    client.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
});

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
