using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Orthologist.Bussiness.Services.Database;
using Orthologist.Bussiness.Services.DataModifiers;
using Orthologist.Bussiness.Services.DataProviders;
using Orthologist.Bussiness.Services.Ebi;
using Orthologist.Bussiness.Services.Helpers;
using Orthologist.Bussiness.Services.OrthoDb;
using Orthologist.Bussiness.Services.Processes;
using Orthologist.Bussiness.Services.Statistics;
using Orthologist.Web.Classes;
using Orthologist.Web.Components;
using Orthologist.Web.Services.Bussiness;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<IOrthoTreeService, OrthoTreeService>();
builder.Services.AddSingleton<IDataMatrixService, MuscleMatrixService>();
builder.Services.AddSingleton<IOrthoGroupModifierService, OrthoGroupModifierService>();
builder.Services.AddSingleton<IOrthoGeneService, OrthoGeneService>();
builder.Services.AddSingleton<IOrthoGroupService, OrthoGroupService>();
builder.Services.AddSingleton<IMantelTestService, MantelTestService>();
builder.Services.AddSingleton<ILineageService, LineageService>();
builder.Services.AddTransient<IDatabaseProvider, ElasticProvider>();
builder.Services.AddTransient<IOrganismSelectionService, OrganismSelectionService>();
builder.Services.AddTransient<ITreeService, TreeService>();
builder.Services.AddTransient<IOrganismService, OrganismService>();
builder.Services.AddTransient<IOrthoFamilyService, OrthoFamilyService>();
builder.Services.AddTransient<IRStatisticService, RStatisticService>();
builder.Services.Configure<ProfileConfig>(builder.Configuration.GetSection("ProfileConfig"));

builder.Services.AddServerSideBlazor()
        .AddCircuitOptions(options =>
        {
            options.DetailedErrors = true;  // Povolení podrobných chyb
        });

builder.Services.AddBlazorise(options =>
{
    options.Immediate = true;
}).AddBootstrapProviders().AddBootstrap5Providers().AddFontAwesomeIcons();
builder.Services.AddBlazorBootstrap();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
