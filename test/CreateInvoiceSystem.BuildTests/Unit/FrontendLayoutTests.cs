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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class FrontendLayoutTests
{
    [Fact]
    public async Task Invoice_list_disables_edit_and_delete_for_locked_invoices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager, TestNavigationManager>();
        services.AddSingleton(Mock.Of<IJSRuntime>());
        services.AddSingleton(new HttpClient(new InvoiceListHandler()) { BaseAddress = new Uri("https://example.test/") });
        services.AddSingleton<InvoiceService>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<CreateInvoiceSystem.Frontend.Pages.Invoices>()).ToHtmlString());
        var rows = Regex.Matches(WebUtility.HtmlDecode(markup), @"<tr\b[^>]*>.*?</tr>", RegexOptions.Singleline)
            .Select(match => match.Value).ToList();
        var accepted = Assert.Single(rows, row => row.Contains("Accepted"));
        var pending = Assert.Single(rows, row => row.Contains("Pending"));

        Assert.Equal(2, Regex.Matches(accepted, @"<button\b[^>]*\bdisabled\b").Count);
        Assert.Equal(2, Regex.Matches(pending, @"<button\b[^>]*\bdisabled\b").Count);
        var rejected = Assert.Single(rows, row => row.Contains("Rejected"));
        Assert.Empty(Regex.Matches(rejected, @"<button\b[^>]*\bdisabled\b"));
        Assert.Contains("Widok", accepted);
    }

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

    private sealed class InvoiceListHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":[{"invoiceId":1,"title":"Accepted","isKsefAccepted":true,"isKsefLocked":true},{"invoiceId":2,"title":"Pending","isKsefLocked":true},{"invoiceId":3,"title":"Rejected","isKsefLocked":false}],"totalCount":3,"success":true}""",
                    Encoding.UTF8, "application/json")
            });
    }
}
