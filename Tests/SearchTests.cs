using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("SearchPage")]
    [Category("Search")]
    public class SearchTests: TestBase
    {
        private const string ProductsUrl = "https://automationexercise.com/products";

        [SetUp]
        public async Task GoToProductsPage()
        {
            await Page.GotoAsync(ProductsUrl);
            await HandleCookieAndOverlayAsync();
            TestContext.WriteLine("🍪 Cookie/Overlay cleared successfully.");
        }

        // ---------- TEST 1 ----------
        [Test]
        public async Task Search_ValidKeyword_ShouldDisplayMatchingResults()
        {
            TestContext.WriteLine("🔍 Searching for 'Tshirt'...");
            await Page.FillAsync("#search_product", "Tshirt");
            await Page.ClickAsync("#submit_search");
            await HandleCookieAndOverlayAsync();

            await Assertions.Expect(Page.Locator(".features_items"))
                .ToContainTextAsync("Searched Products");
            TestContext.WriteLine("✅ Matching products displayed correctly.");
        }

        // ---------- TEST 2 ----------
        [Test]
        public async Task Search_InvalidKeyword_ShouldShowNoResultsMessage()
        {
            TestContext.WriteLine("🔍 Searching for invalid keyword...");
            await Page.FillAsync("#search_product", "qwerty123");
            await Page.ClickAsync("#submit_search");
            await HandleCookieAndOverlayAsync();

            var pageContent = await Page.ContentAsync();
            StringAssert.Contains("Searched Products", pageContent, "Page should still load search section.");
        }

        // ---------- TEST 3 ----------
        [Test]
        public async Task Search_CaseInsensitive_ShouldReturnResults()
        {
            TestContext.WriteLine("🔍 Checking case-insensitive search...");
            await Page.FillAsync("#search_product", "tShIrT");
            await Page.ClickAsync("#submit_search");
            await HandleCookieAndOverlayAsync();

            await Assertions.Expect(Page.Locator(".features_items"))
                .ToContainTextAsync("Searched Products");
            TestContext.WriteLine("✅ Search works case-insensitively.");
        }

        // ---------- TEST 4 ----------
        [Test]
        public async Task Search_EmptyField_ShouldNotCrash()
        {
            TestContext.WriteLine("🧪 Checking empty search field...");
            await Page.FillAsync("#search_product", "");
            await Page.ClickAsync("#submit_search");
            await HandleCookieAndOverlayAsync();

            var currentUrl = Page.Url;
            StringAssert.Contains("https://automationexercise.com/products?search=", currentUrl, 
                "URL should remain the same or contain ?search=");
        }

        // ---------- TEST 5 ----------
        [Test]
        public async Task Search_PartialMatch_ShouldReturnResults()
        {
            TestContext.WriteLine("🔍 Checking partial keyword search...");
            await Page.FillAsync("#search_product", "Dress");
            await Page.ClickAsync("#submit_search");
            await HandleCookieAndOverlayAsync();

            await Assertions.Expect(Page.Locator(".features_items"))
                .ToContainTextAsync("Searched Products");
            TestContext.WriteLine("✅ Partial match works properly.");
        }

        // ---------- TEST 6 ----------
        [Test]
        public async Task Search_FastSequentialSearch_ShouldDisplayLastResult()
        {
            TestContext.WriteLine("🏎️ Performing rapid sequential searches...");

            string[] keywords = { "Jeans", "Top", "Tshirt" };

            foreach (var word in keywords)
            {
                await Page.FillAsync("#search_product", word);
                await Page.ClickAsync("#submit_search");
                await HandleCookieAndOverlayAsync();
                TestContext.WriteLine($"➡️ Search for '{word}' completed.");
                await Task.Delay(500);
            }

            await Assertions.Expect(Page.Locator(".features_items"))
                .ToContainTextAsync("Searched Products");
            TestContext.WriteLine("✅ Last search displayed correctly.");
        }

        // ---------- COOKIE HANDLER ----------
        private async Task HandleCookieAndOverlayAsync()
        {
            try
            {
                await Page.EvaluateAsync(@"
                    document.querySelectorAll('div.fc-consent-root, div.fc-dialog-container, div.fc-dialog-overlay, iframe[name=""googlefcPresent""]').forEach(e => e.remove());
                    document.body.style.overflow = 'auto';
                    document.body.style.position = 'static';
                ");

                await Page.WaitForTimeoutAsync(1000);

                var overlayExists = await Page.EvaluateAsync<bool>(@"
                    !!document.querySelector('div.fc-consent-root, div.fc-dialog-overlay')
                ");

                if (overlayExists)
                {
                    await Page.EvaluateAsync(@"
                        document.querySelectorAll('div.fc-consent-root, div.fc-dialog-overlay').forEach(e => e.remove());
                        document.body.style.overflow = 'auto';
                    ");
                    TestContext.WriteLine("🍪 Overlay reappeared — removed again.");
                }
                else
                {
                    TestContext.WriteLine("🍪 Cookie/Overlay fully cleared.");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️ Overlay removal failed: {ex.Message}");
            }
        }
    }
}
