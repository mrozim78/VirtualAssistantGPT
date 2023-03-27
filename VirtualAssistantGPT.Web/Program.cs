using OpenAI.GPT3;
using OpenAI.GPT3.Extensions;
using VirtualAssistantGPT.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenAIService();

 
builder.Services.AddRazorPages();
IConfiguration requiredService = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
builder.Services.Configure<AWSCredentialsOptions>(requiredService.GetSection("AWSCredentialsOptions"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
