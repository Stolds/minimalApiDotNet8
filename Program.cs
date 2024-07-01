using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Context;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PizzaDb>
(
    options => options.UseMySql(
        "server=localhost;initial catalog=pizzastore;uid=root;pwd=password",
        ServerVersion.Parse("8.0.36-mysql")
));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "PizzaStore API",
            Description = "Making the Pizzas you love",
            Version = "v1"
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI(c =>
   {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
   });
}

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

//for configure the post I need to pass the context, the class, he without any more info
//identifies already what is the type of body that he will need to receive
//then I add the object that I receive to the database save and give the return
//this post don't have a handle for badrequest

app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) => 
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatepizza, int id) => 
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound("we didn't found your pizza :( sorry");
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) => 
{
    var pizza = await db.Pizzas.FindAsync(id);
    if(pizza is null)
    {
        return Results.NotFound("don't have any pizza with that id here ;>");
    }
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});


app.Run();
