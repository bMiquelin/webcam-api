using Serilog;

Emgu.CV.VideoCapture captureStream = null;
DateTime? lastCapture = null;


Task Warm()
{
    return Task.Run(() => {
        Log.Information("Warming up");
        captureStream = new Emgu.CV.VideoCapture(0);
        captureStream.Start();
        Log.Information("Capture started");
    });
}

bool IsWarming() => captureStream == null || !captureStream.IsOpened;


async Task<byte[]> GetFrame(bool small = false)
{
    Log.Information("Capture requested");
    if (IsWarming())
    {
        Log.Warning("Stream is warming up");
        return null;
    }
    var frame = small ? captureStream.QuerySmallFrame() : captureStream.QueryFrame();
    lastCapture = DateTime.Now;
    var tempFile = "temp.jpg";
    frame.Save(tempFile);
    var content = await System.IO.File.ReadAllBytesAsync(tempFile);
    System.IO.File.Delete(tempFile);
    return content;
}


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

var app = builder.Build();
app.UseSerilogRequestLogging();

_ = Warm();
app.Map("/", () => new {
    IsWarming = IsWarming(),
    Ip = System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()).Result.First().ToString(),
    LastCapture = lastCapture
});
app.Map("/frame", async () => {
    var content = await GetFrame();
    return content is null ? Results.NotFound() : Results.File(content, "image/jpeg");
});
app.Map("/framesmall", async () => Results.File(await GetFrame(true), "image/jpeg"));
app.Run("http://*:8080");