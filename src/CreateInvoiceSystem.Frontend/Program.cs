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
            
            var apiBaseAddress = builder.Configuration["BaseApiUrl"] ?? "https://localhost:7168/";
            var apiUri = new Uri(apiBaseAddress);

            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddTransient<AuthenticatedAndRefreshedHandler>();
            
            builder.Services.AddHttpClient<InvoiceService>(client =>
            {
                client.BaseAddress = apiUri;
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<ClientService>(client =>
            {
                client.BaseAddress = apiUri;
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<ProductService>(client =>
            {
                client.BaseAddress = apiUri;
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<UserService>(client =>
            {
                client.BaseAddress = apiUri;
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddHttpClient<NbpService>(client =>
            {
                client.BaseAddress = apiUri;
            });

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = apiUri;
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

            builder.Services.AddHttpClient("FullAccessClient", client =>
            {
                client.BaseAddress = apiUri;
            })
            .AddHttpMessageHandler<AuthenticatedAndRefreshedHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("FullAccessClient"));

            await builder.Build().RunAsync();
        }
    }
}