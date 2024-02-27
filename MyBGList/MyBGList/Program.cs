using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*Add a service that allows cross-origin requests from all origins, headers, and methods
 *builder.Services.AddCors(options =>
    options.AddDefaultPolicy(cfg =>
        {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
}));
*/

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
 *      Requires using Microsoft.AspNetCore.Cors namespace
*/

app.MapGet("/error", [EnableCors("AnyOrigin")] () => Results.Problem());
app.MapGet("/error/test", [EnableCors("AnyOrigin")] () => { throw new Exception("test"); });

app.MapControllers();

app.Run();
