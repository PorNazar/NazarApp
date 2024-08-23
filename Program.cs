using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ItemsDb>(opt => opt.UseInMemoryDatabase("ItemsList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument( config => {
    config.DocumentName = "ItemsAPI";
    config.Title = "ItemsAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

app.UseOpenApi();
    app.UseSwaggerUi(config => {
        config.DocumentTitle = "ItemsAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

app.MapGet("/", ()=> {
return Results.Redirect("/swagger/index.html");
})
.ExcludeFromDescription();


app.MapGet("/items", async (ItemsDb db) => 
{
    var result = await db.Items.ToListAsync();
    return Results.Ok(result);
});

app.MapPost("/items", async (Item item, ItemsDb db) => {
    await db.Items.AddAsync(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapGet("/items/{id}", async (int id, ItemsDb db)=> {
    return await db.Items.FindAsync(id)
    is Item item 
    ? Results.Ok(item)
    : Results.NotFound();
});

app.MapDelete("/items/{id}", async (int id, ItemsDb db) => {
    if(await db.Items.FindAsync(id) is Item item){
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
