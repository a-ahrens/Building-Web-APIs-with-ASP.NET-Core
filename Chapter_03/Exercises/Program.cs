using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

    options.AddPolicy(name: "AnyOrigin_GetOnly", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.WithMethods("GET");
    });
});

/* API versioning example:
 * 
*/

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
    options.SwaggerDoc(
        "v3",
        new OpenApiInfo
        {
            Title = "MyBGList",
            Version = "v3.0"
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            $"/swagger/v1/swagger.json",
            $"MyBGList v1");
        options.SwaggerEndpoint(
            $"/swagger/v2/swagger.json",
            $"MyBGList v2");
        options.SwaggerEndpoint(
            $"/swagger/v3/swagger.json",
            $"MyBGList v3");
    });
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


/* Minimal API using [EnableCors] attribute
 *      Microsoft's recommended way to implement CORS on a per-endpoint basis
 *          Requires using Microsoft.AspNetCore.Cors namespace
 *      Response cache set to NoStore to prevent anyone from caching errors
 *          Requires using Microsoft.AspNetCore.Mvc namespace
*/

app.MapGet("/v{version:ApiVersion}/error",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]         
    () => Results.Problem());

app.MapGet("/v{version:ApiVersion}/error/test",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin")]
    [ResponseCache(NoStore = true)]
    () => { 
        throw new Exception("test");
    });

//Example of how to use Code on Demand (COD) REST constraint
app.MapGet("/v{version:ApiVersion}/cod/test",
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [EnableCors("AnyOrigin_GetOnly")]
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

app.MapControllers().RequireCors("AnyOrigin");

app.Run();
