using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("Smoke")]
    [Parallelizable(ParallelScope.Self)]
    public class SmokeTests
    {
        private IPlaywright _playwright = null!;
        private IBrowser _browser = null!;
        private IBrowserContext _context = null!;
        private IPage Page = null!;

        [SetUp]
        public async Task SetUp()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                SlowMo = 100
            });
            _context = await _browser.NewContextAsync();
            Page = await _context.NewPageAsync();
            TestContext.WriteLine("✅ Browser launched successfully.");
        }

        [TearDown]
        public async Task TearDown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
            TestContext.WriteLine("🧹 Browser closed.");
        }

        // ---------- COOKIE HANDLER ----------
        private async Task HandleCookieOverlayAsync()
        {
            try
            {
                await Page.EvaluateAsync(@"
                    document.querySelectorAll('div.fc-consent-root, div.fc-dialog-container, div.fc-dialog-overlay, iframe[name=""googlefcPresent""]').forEach(e => e.remove());
                    document.body.style.overflow = 'auto';
                    document.body.style.position = 'static';
                ");
                await Page.WaitForTimeoutAsync(800);
            }
            catch { /* Safe to ignore */ }
        }

        // ---------- SMOKE 1 ----------
        [Test]
        public async Task HomePageLoads_ShouldDisplayMainMenu()
        {
            await Page.GotoAsync("https://automationexercise.com");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🌐 Navigated to home page.");

            var isVisible = await Page.GetByRole(AriaRole.Link, new() { Name = "Products" }).IsVisibleAsync();
            Assert.IsTrue(isVisible, "Expected 'Products' link to be visible on the home page.");
            TestContext.WriteLine("✅ Home page loaded and main menu visible.");
        }

        // ---------- SMOKE 2 ----------
        [Test]
        public async Task ProductsPage_ShouldDisplaySearchBox()
        {
            await Page.GotoAsync("https://automationexercise.com/products");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🛒 Opening Products page...");

            var searchBoxVisible = await Page.Locator("#search_product").IsVisibleAsync();
            Assert.IsTrue(searchBoxVisible, "Search input should be visible on Products page.");
            TestContext.WriteLine("✅ Products page loaded with visible search box.");
        }

        // ---------- SMOKE 3 ----------
        [Test]
        public async Task ContactUsPage_ShouldDisplayForm()
        {
            await Page.GotoAsync("https://automationexercise.com/contact_us");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("📨 Navigated to Contact Us page...");

            var formVisible = await Page.Locator("form[action='/contact_us']").IsVisibleAsync();
            Assert.IsTrue(formVisible, "Expected contact form to be visible.");
            TestContext.WriteLine("✅ Contact form is visible.");
        }

        // ---------- SMOKE 4 ----------
        [Test]
        public async Task LoginPage_ShouldDisplayLoginForm()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🔑 Opening Login page...");

            var emailInputVisible = await Page.Locator("input[data-qa='login-email']").IsVisibleAsync();
            var passwordInputVisible = await Page.Locator("input[data-qa='login-password']").IsVisibleAsync();

            Assert.IsTrue(emailInputVisible && passwordInputVisible, "Login form fields should be visible.");
            TestContext.WriteLine("✅ Login form fields visible.");
        }

        // ---------- SMOKE 5 ----------
        [Test]
        public async Task CartPage_ShouldDisplayCartTable()
        {
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🛍️ Opening Cart page...");

            var cartTableVisible = await Page.Locator("#cart_info_table").IsVisibleAsync();
            var emptyCartVisible = await Page.Locator("p:has-text('Cart is empty!')").IsVisibleAsync();

            Assert.IsTrue(cartTableVisible || emptyCartVisible,
                "Expected cart info table or empty cart message to be visible.");
            TestContext.WriteLine("✅ Cart page loaded correctly (table or empty message visible).");
        }

        // ---------- SMOKE 6 ----------
        [Test]
        public async Task Footer_ShouldContainSubscriptionSection()
        {
            await Page.GotoAsync("https://automationexercise.com");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🔎 Checking footer section...");

            var subscriptionVisible = await Page.GetByText("SUBSCRIPTION").IsVisibleAsync();
            Assert.IsTrue(subscriptionVisible, "Expected 'SUBSCRIPTION' section in footer.");
            TestContext.WriteLine("✅ Footer subscription section visible.");
        }

        // ---------- SMOKE 7 ----------
        [Test]
        public async Task NavigationLinks_ShouldWorkProperly()
        {
            await Page.GotoAsync("https://automationexercise.com");
            await HandleCookieOverlayAsync();
            TestContext.WriteLine("🔗 Testing navigation links...");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Products" }).ClickAsync();
            await HandleCookieOverlayAsync();
            Assert.That(Page.Url, Does.Contain("/products"), "Navigation to Products failed.");

            await Page.GoBackAsync();
            await HandleCookieOverlayAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Contact us" }).ClickAsync();
            await HandleCookieOverlayAsync();
            Assert.That(Page.Url, Does.Contain("/contact_us"), "Navigation to Contact Us failed.");

            await Page.GoBackAsync();
            await HandleCookieOverlayAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await HandleCookieOverlayAsync();
            Assert.That(Page.Url, Does.Contain("/login"), "Navigation to Login failed.");

            TestContext.WriteLine("✅ All main navigation links function correctly.");
        }
    }
}
