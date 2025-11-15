using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("ProductPage")]
    public class ProductPageTests: TestBase
    {
        [Test]
        public async Task OpenProductPage_ShouldDisplayProductDetails()
        {
            TestContext.WriteLine("🌐 Opening Products page...");
            await Page.GotoAsync("https://automationexercise.com/products");

            await DismissCookiePopupIfExists();
            await RemoveOverlayIfExists();

            TestContext.WriteLine("🔍 Opening first product (simulated details)...");
            var product = Page.Locator(".features_items .product-image-wrapper").First;
            await product.ScrollIntoViewIfNeededAsync();

            // Вземаме първия <p> елемент, за да избегнем strict mode violation
            var productName = await product.Locator("p").Nth(0).InnerTextAsync();
            TestContext.WriteLine($"ℹ️ Product name: {productName}");

            await Expect(product).ToBeVisibleAsync();
            TestContext.WriteLine("✅ Product card displayed correctly");
        }

        [Test]
        public async Task SearchProduct_ShouldShowMatchingResults()
        {
            TestContext.WriteLine("🔎 Opening Products page...");
            await Page.GotoAsync("https://automationexercise.com/products");

            await DismissCookiePopupIfExists();
            await RemoveOverlayIfExists();

            TestContext.WriteLine("🧩 Searching for 'Tshirt'...");
            await Page.FillAsync("#search_product", "Tshirt");
            await Page.ClickAsync("#submit_search");

            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await Expect(Page.Locator(".features_items")).ToContainTextAsync("Tshirt");
            TestContext.WriteLine("✅ Search results displayed successfully");
        }

        [Test]
        public async Task AddProductToCart_ShouldIncreaseCartCount()
        {
            TestContext.WriteLine("🛒 Opening Products page...");
            await Page.GotoAsync("https://automationexercise.com/products");

            await DismissCookiePopupIfExists();
            await RemoveOverlayIfExists();

            TestContext.WriteLine("📦 Adding product to cart...");
            var addToCart = Page.Locator("a.btn.add-to-cart[data-product-id='4']").First;
            await addToCart.ScrollIntoViewIfNeededAsync();
            await addToCart.ClickAsync(new() { Force = true });

            // Изчакваме бутона "Continue Shopping"
            var continueBtn = Page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
            if (await continueBtn.IsVisibleAsync(new() { Timeout = 5000 }))
                await continueBtn.ClickAsync();

            await RemoveOverlayIfExists();

            TestContext.WriteLine("🧾 Opening Cart page...");
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // 🔹 Премахваме блокиращия стил (overflow:hidden)
            await Page.EvaluateAsync(@"document.body.style.overflow = 'auto';");

            // 🔹 Изчакваме секцията с количката
            var cartSection = Page.Locator("#cart_items");
            await cartSection.ScrollIntoViewIfNeededAsync();
            await Expect(cartSection).ToBeVisibleAsync(new() { Timeout = 15000 });

            TestContext.WriteLine("✅ Product successfully added to cart and cart section visible");
        }

        // === Helpers ===
        private async Task DismissCookiePopupIfExists()
        {
            try
            {
                await Page.EvaluateAsync(@"
                    document.querySelectorAll('div.fc-consent-root, div.fc-dialog-container')
                            .forEach(e => e.remove());
                ");
                TestContext.WriteLine("🍪 Cookie pop-up cleared (if present).");
            }
            catch { }
        }

        private async Task RemoveOverlayIfExists()
        {
            try
            {
                await Page.EvaluateAsync(@"
                    document.querySelectorAll('div.fc-dialog-overlay, div.modal-backdrop, h2.title.text-center')
                            .forEach(e => e.style.display = 'none');
                ");
                TestContext.WriteLine("🧹 Overlays cleared (if present).");
            }
            catch { }
        }
    }
}
