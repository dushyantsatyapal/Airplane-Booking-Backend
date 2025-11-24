using AirplaneBooking.API.Filters;
using AirplaneBooking.Application;
using AirplaneBooking.Infrastructure;
using Microsoft.OpenApi.Models; // Required for OpenApiInfo, OpenApiContact


var builder = WebApplication.CreateBuilder(args);

// Get the IHostEnvironment instance early if needed for conditional logic
var env = builder.Environment;

// Configuration: Environment variables are typically added by default, but explicitly including doesn't hurt.
builder.Configuration.AddEnvironmentVariables();

// --- Service Registration (Add services to the container) ---
builder.Services.AddControllers();

// Register services from your Clean Architecture layers using extension methods
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer(); // Enables API Explorer for Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Airplane Booking Service",
        Description = "API for an airplane booking service.",
        Contact = new OpenApiContact()
        {
            Name = "Your Name/Organization",
            Url = new Uri("https://github.com/your-repo"),
            Email = "your.email@example.com"
        },
        TermsOfService = new Uri("http://localhost:5000/swagger/index.html") // Adjust port as necessary
    });

    c.CustomSchemaIds(type => type.FullName);

    // Optional: Include XML comments for API documentation in Swagger
    // 1. In your SolutionName.Api project properties -> Build -> Output, check "XML documentation file".
    // 2. Uncomment the following lines:
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // if (File.Exists(xmlPath))
    // {
    //     c.IncludeXmlComments(xmlPath);
    // }

    // Apply custom document and operation filters for Swagger
    // Ensure BasePathFilter and GeneratePathParamsValidationFilter classes exist in SolutionName.Api.Filters
    c.DocumentFilter<BasePathFilter>("/virts/motorcompany/airplane-booking-service/1.0.0");
    c.OperationFilter<GeneratePathParamsValidationFilter>();
});


// --- Build the application ---
var app = builder.Build();


// --- Configure the HTTP request pipeline (Middleware) ---

// Exception Handling: Should be at the very top of the pipeline
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Provides detailed error pages in development
}
else
{
    app.UseExceptionHandler("/Error"); // Redirects to a generic error page/endpoint in production
    app.UseHsts(); // Enforces HTTPS for security in production
}

// Swagger UI: Enables the Swagger UI interface
app.UseSwagger(); // Enables the Swagger JSON endpoint (/swagger/v1/swagger.json or /swagger/1.0.0/swagger.json)
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/1.0.0/swagger.json", "Airplane Booking Service V1"); // Points to your Swagger JSON
    // Optional: If you want Swagger UI to be at the root (e.g., http://localhost:port/)
    // c.RoutePrefix = string.Empty;
});

// HTTPS Redirection: Redirects HTTP requests to HTTPS
app.UseHttpsRedirection();

// Authorization: Enables authorization middleware (e.g., JWT token validation)
app.UseAuthorization();

// Map Controllers: Maps incoming requests to your API controller actions
app.MapControllers();

// Run the application
app.Run();