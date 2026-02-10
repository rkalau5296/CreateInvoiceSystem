using CreateInvoiceSystem.Frontend.Handler;
using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace CreateInvoiceSystem.Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddTransient<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<InvoiceService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<ClientService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<ProductService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<UserService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<NbpService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            });

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7168/");
            });

            builder.Services.AddScoped<AuthService>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var js = sp.GetRequiredService<IJSRuntime>();
                var nav = sp.GetRequiredService<NavigationManager>();
                var authState = sp.GetRequiredService<AuthenticationStateProvider>();
                var cleanClient = factory.CreateClient("AuthClient");
                return new AuthService(cleanClient, js, nav, authState);
            });

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<CustomAuthStateProvider>());

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7168/")
            });
            await builder.Build().RunAsync();
        }
    }
}