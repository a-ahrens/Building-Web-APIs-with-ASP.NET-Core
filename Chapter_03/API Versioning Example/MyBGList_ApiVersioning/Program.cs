using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

/*More Restricted Cors Policy
 *  Creating a default policy to handle requests from a known origin 
 *  Creating an any origin policy to handle requests that don't need restrictions (edge cases)*  
*/
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });

    options.AddPolicy(name: "AnyOrigin", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new UrlSegmentApiVersionReader();    //enables URI versioning
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";                 //Sets the API versioning format
    options.SubstituteApiVersionInUrl = true;           //Replaces the {apiVersion} place-holder with version number
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "MyBGList",
            Version = "v1.0"
        });
    options.SwaggerDoc(
        "v2",
        new OpenApiInfo
        {
            Title = "MyBGList",
            Version = "v2.0"
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}



app.UseHttpsRedirection();

//Cors needs to be established before Controllers and Minimal API routes are. Middleware order matters
app.UseCors();

app.UseAuthorization();

/* Examples of Minimal API routes using endpoint routing. 
 *      Not the suggested approach due to issues with RequireCors() extension method not supporting preflight requests
 
app.MapGet("/error", () => Results.Problem()).RequireCors("AnyOrigin");
app.MapGet("/error/test", () => { throw new Exception("test"); }).RequireCors("AnyOrigin");

 */

/* Minimal API using [EnableCors] attribute
 *      Microsoft's recommended way to implement CORS on a per-endpoint basis
 *          Requires using Microsoft.AspNetCore.Cors namespace
 *      Response cache set to NoStore to prevent anyone from caching errors
 *          Requires using Microsoft.AspNetCore.Mvc namespace
*/

app.MapGet("/error", 
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]         
    () => Results.Problem());
app.MapGet("/error/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => { 
        throw new Exception("test");
    });

//Example of how to use Code on Demand (COD) REST constraint
app.MapGet("/cod/test",
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)] () =>
    Results.Text("<script>" +
       "window.alert('Your client supports JavaScript!" +
       "\\r\\n\\r\\n" +
       $"Server time (UTC): {DateTime.UtcNow.ToString("o")}" +
       "\\r\\n" +
       "Client time (UTC): ' + new Date().toISOString());" +
       "</script>" +
       "<noscript>Your client does not support JavaScript</noscript>",
       "text/html"
    ));

app.MapControllers();

app.Run();
