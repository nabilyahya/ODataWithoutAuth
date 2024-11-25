using AirVinyl.API.DbContexts;
using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;

using Microsoft.EntityFrameworkCore;
using AirVinyl.EntityDataModels;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AirVinylDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add OData services and enable query options like Select, Expand, Filter, etc.
builder.Services.AddControllers()
    .AddOData(opt =>
        opt.Select().Expand().Filter().OrderBy().Count().SetMaxTop(100)
            .AddRouteComponents("odata", AirVinylEntityDataModel.GetEntityDataModel()) 
    );


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
   
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OData API",
        Version = "v1"
    });

    // This fix Fetch error response status is 500 /swagger/v1/swagger.json
    options.ResolveConflictingActions(apiDescriptions =>
    {
        return apiDescriptions.First();
    });

});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData API V1");

        // if we want to use swagger at root basr url
        //c.RoutePrefix = string.Empty;
    });

}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

