using CreateInvoiceSystem.Frontend.Layout;
using CreateInvoiceSystem.Frontend.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class FrontendLayoutTests
{
    [Fact]
    public async Task Direct_layout_renders_the_same_not_found_page_as_layout_view()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorizationCore();
        services.AddSingleton<NavigationManager, TestNavigationManager>();
        services.AddSingleton(Mock.Of<IJSRuntime>());
        services.AddSingleton<AuthenticationStateProvider, CustomAuthStateProvider>();
        services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://example.test/") });
        services.AddSingleton<AuthService>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var (oldMarkup, newMarkup) = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var oldLayout = await renderer.RenderComponentAsync<LayoutHarness>(ParameterView.FromDictionary(
                new Dictionary<string, object?> { [nameof(LayoutHarness.UseLayoutView)] = true }));
            var directLayout = await renderer.RenderComponentAsync<LayoutHarness>();
            return (oldLayout.ToHtmlString(), directLayout.ToHtmlString());
        });

        Assert.Equal(oldMarkup, newMarkup);
        Assert.Contains("Sorry, there's nothing at this address.", System.Net.WebUtility.HtmlDecode(newMarkup));
    }

    public sealed class LayoutHarness : ComponentBase
    {
        [Parameter]
        public bool UseLayoutView { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            RenderFragment content = b =>
            {
                b.OpenElement(0, "p");
                b.AddAttribute(1, "role", "alert");
                b.AddContent(2, "Sorry, there's nothing at this address.");
                b.CloseElement();
            };
            RenderFragment layout = b =>
            {
                if (UseLayoutView)
                {
                    b.OpenComponent<LayoutView>(0);
                    b.AddComponentParameter(1, nameof(LayoutView.Layout), typeof(MainLayout));
                    b.AddComponentParameter(2, nameof(LayoutView.ChildContent), content);
                    b.CloseComponent();
                }
                else
                {
                    b.OpenComponent<MainLayout>(3);
                    b.AddComponentParameter(4, nameof(LayoutComponentBase.Body), content);
                    b.CloseComponent();
                }
            };
            builder.OpenComponent<CascadingAuthenticationState>(0);
            builder.AddComponentParameter(1, nameof(CascadingAuthenticationState.ChildContent), layout);
            builder.CloseComponent();
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("https://example.test/", "https://example.test/missing");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
}
