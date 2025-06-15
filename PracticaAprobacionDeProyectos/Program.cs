using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.UseCase;
using Infrastructure.Command;
using Infrastructure.Persistence;
using Infrastructure.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PracticaAprobacionDeProyectos.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Custom
var connectionString = builder.Configuration["ConnectionStrings"];
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPorjectProposalService, PorjectProposalService>();
builder.Services.AddScoped<IProjectApprovalStepService, ProjectApprovalStepService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IProjectTypeService, ProjectTypeService>();
builder.Services.AddScoped<IApproverRoleService, ApproverRoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApprovalStatusService, ApprovalStatusService>();
builder.Services.AddScoped<IProjectApprovalStepCommand, ProjectApprovalStepCommand>();
builder.Services.AddScoped<IProjectProposalCommand, ProjectProposalCommand>();
builder.Services.AddScoped<IApprovalRuleQuery, ApprovalRuleQuery>();
builder.Services.AddScoped<IProjectApprovalStepQuery, ProjectApprovalStepQuery>();
builder.Services.AddScoped<IProjectProposalQuery, ProjectProposalQuery>();
builder.Services.AddScoped<IUserQuery, UserQuery>();
builder.Services.AddScoped<IAreaQuery, AreaQuery>();
builder.Services.AddScoped<IProjectTypeQuery, ProjectTypeQuery>();
builder.Services.AddScoped<IApproverRoleQuery, ApproverRoleQuery>();
builder.Services.AddScoped<IApprovalStatusQuery, ApprovalStatusQuery>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(new { message = "Parámetro de consulta inválido" });
    };
});


//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("PermitirTodo");

app.UseMiddleware<ExceptionHandlingMiddleware>();


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
