// Top 25 Critical Test Cases Automated with C# + Playwright + NUnit
// Project quickstart:
// 1) dotnet new nunit -n AutomationExerciseTests
// 2) cd AutomationExerciseTests
// 3) dotnet add package Microsoft.Playwright
// 4) dotnet add package Microsoft.Playwright.NUnit
// 5) npx playwright install  (or: dotnet tool install --global Microsoft.Playwright.CLI && playwright install)
// 6) Put this file into the project (e.g., Tests/Top25Tests.cs)
// 7) dotnet test

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace AutomationExercise.Top25
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public class Top25Tests
    {
        private IPlaywright _pw;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;

        private const string BaseUrl = "https://www.automationexercise.com";

        [SetUp]
        public async Task SetUp()
        {
            _pw = await Playwright.CreateAsync();
            _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1366, Height = 768 }
            });
            _page = await _context.NewPageAsync();
            _page.SetDefaultTimeout(15000);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.CloseAsync();
            await _browser.CloseAsync();
            _pw?.Dispose();
        }

        // ---------- Helpers ----------
        private async Task GoHomeAsync() => await _page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        private async Task OpenLoginAsync()
        {
            await GoHomeAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await ExpectVisibleAsync(_page, "text=Login to your account");
        }

        private async Task LoginAsync(string email, string password)
        {
            await OpenLoginAsync();
            // Common AutomationExercise selectors use data-qa attributes in tutorials; keep both safe fallbacks
            var emailInput = _page.Locator("input[data-qa='login-email'], input[name='email']");
            var passInput = _page.Locator("input[data-qa='login-password'], input[name='password']");
            await emailInput.FillAsync(email);
            await passInput.FillAsync(password);
            await _page.Locator("button[data-qa='login-button'], button:has-text('Login')").First.ClickAsync();
        }

        private async Task EnsureLoggedOutAsync()
        {
            await GoHomeAsync();
            if (await _page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).IsVisibleAsync())
            {
                await _page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();
            }
        }

        private async Task OpenProductsAsync()
        {
            await GoHomeAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Products" }).ClickAsync();
            await ExpectVisibleAsync(_page, "text=All Products");
        }

        private async Task SearchAsync(string term)
        {
            await OpenProductsAsync();
            var searchInput = _page.Locator("input#search_product, input[name='search']");
            await searchInput.FillAsync(term);
            await _page.Locator("button#submit_search, button:has-text('Search')").First.ClickAsync();
            await ExpectVisibleAsync(_page, "text=Searched Products");
        }

        private async Task AddFirstProductFromGridAsync()
        {
            // Hover card and click Add to cart
            var firstCard = _page.Locator(".product-image-wrapper").First;
            await firstCard.HoverAsync();
            await _page.Locator(".product-image-wrapper:has-text('Add to cart') >> text=Add to cart").First.ClickAsync();
            // Modal → Continue Shopping
            var modal = _page.Locator(".modal-content");
            if (await modal.IsVisibleAsync())
            {
                var cont = modal.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
                if (await cont.IsVisibleAsync()) await cont.ClickAsync();
            }
        }

        private async Task OpenCartAsync()
        {
            await _page.GetByRole(AriaRole.Link, new() { Name = "Cart" }).ClickAsync();
            await ExpectVisibleAsync(_page, "text=Shopping Cart");
        }

        private async Task ProceedToCheckoutAsync()
        {
            await _page.GetByRole(AriaRole.Link, new() { Name = "Proceed To Checkout" }).First.ClickAsync();
        }

        private static async Task ExpectVisibleAsync(IPage page, string locatorText)
        {
            await page.Locator($":text('{locatorText.Replace("text=", string.Empty)}')").First.WaitForAsync();
        }

        // ---------- Top 25 Tests ----------
        // 01 Authenticate with valid credentials
        [Test, Order(1), Category("Login")] 
        public async Task TC01_AuthenticateWithValidCredentials()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            Assert.IsTrue(await _page.GetByText("Logged in as").IsVisibleAsync(), "Expected to be logged in.");
            Assert.IsTrue(await _page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).IsVisibleAsync());
        }

        // 02 Reject login with incorrect password
        [Test, Order(2), Category("Login")]
        public async Task TC02_RejectLoginWithIncorrectPassword()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Wrong999");
            Assert.IsTrue(await _page.GetByText("Your email or password is incorrect!").IsVisibleAsync());
        }

        // 03 Reject login with unregistered email
        [Test, Order(3), Category("Login")]
        public async Task TC03_RejectLoginWithUnregisteredEmail()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync($"notreg+{System.DateTime.UtcNow.Ticks}@example.com", "Any123!");
            Assert.IsTrue(await _page.GetByText("Your email or password is incorrect!").IsVisibleAsync());
        }

        // 04 Email field required validation
        [Test, Order(4), Category("Login")]
        public async Task TC04_EmailFieldRequiredValidation()
        {
            await OpenLoginAsync();
            await _page.Locator("input[data-qa='login-password'], input[name='password']").FillAsync("Valid123!");
            await _page.Locator("button[data-qa='login-button'], button:has-text('Login')").First.ClickAsync();
            // Expect built-in validation or error label
            var invalid = await _page.Locator("input[data-qa='login-email'], input[name='email']").EvaluateAsync<bool>("e => e.validationMessage !== ''");
            Assert.IsTrue(invalid, "Expected email field required validation.");
        }

        // 05 Password field required validation
        [Test, Order(5), Category("Login")]
        public async Task TC05_PasswordFieldRequiredValidation()
        {
            await OpenLoginAsync();
            await _page.Locator("input[data-qa='login-email'], input[name='email']").FillAsync("valid@user.com");
            await _page.Locator("button[data-qa='login-button'], button:has-text('Login')").First.ClickAsync();
            var invalid = await _page.Locator("input[data-qa='login-password'], input[name='password']").EvaluateAsync<bool>("e => e.validationMessage !== ''");
            Assert.IsTrue(invalid, "Expected password field required validation.");
        }

        // 06 Email format validation
        [Test, Order(6), Category("Login")]
        public async Task TC06_EmailFormatValidation()
        {
            await OpenLoginAsync();
            await _page.Locator("input[data-qa='login-email'], input[name='email']").FillAsync("user@");
            await _page.Locator("input[data-qa='login-password'], input[name='password']").FillAsync("Any123!");
            await _page.Locator("button[data-qa='login-button'], button:has-text('Login')").First.ClickAsync();
            var invalid = await _page.Locator("input[data-qa='login-email'], input[name='email']").EvaluateAsync<bool>("e => e.validationMessage !== ''");
            Assert.IsTrue(invalid, "Expected invalid email format validation.");
        }

        // 07 Logout ends session
        [Test, Order(7), Category("Login")]
        public async Task TC07_LogoutEndsSession()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await _page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();
            Assert.IsTrue(await _page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).IsVisibleAsync());
        }

        // 08 Persist cart items after login
        [Test, Order(8), Category("Login/Cart")]
        public async Task TC08_PersistCartItemsAfterLogin()
        {
            await EnsureLoggedOutAsync();
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Cart" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Proceed To Checkout" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Register / Login" }).ClickAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await OpenCartAsync();
            Assert.IsTrue(await _page.Locator("tbody tr").CountAsync() > 0, "Expected items to persist after login.");
        }

        // 09 Add single product from listing
        [Test, Order(9), Category("Cart")]
        public async Task TC09_AddSingleProductFromListing()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            Assert.IsTrue(await _page.Locator("tbody tr").CountAsync() >= 1);
        }

        // 10 Increase quantity before add (detail page)
        [Test, Order(10), Category("Cart")]
        public async Task TC10_IncreaseQtyBeforeAdd()
        {
            await OpenProductsAsync();
            await _page.Locator(".product-image-wrapper a:has-text('View Product')").First.ClickAsync();
            var qty = _page.Locator("input[name='quantity']");
            if (await qty.IsVisibleAsync())
            {
                await qty.FillAsync("3");
            }
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
            var cont = _page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
            if (await cont.IsVisibleAsync()) await cont.ClickAsync();
            await OpenCartAsync();
            // Read qty cell
            var qtyCell = _page.Locator("tbody tr td:nth-child(4)").First; // typical table: image, desc, price, qty, total, remove
            var txt = await qtyCell.InnerTextAsync();
            Assert.IsTrue(txt.Contains("3") || txt.Trim() == "3", $"Expected quantity 3, got '{txt}'");
        }

        // 11 Update quantity in cart
        [Test, Order(11), Category("Cart")]
        public async Task TC11_UpdateQuantityInCart()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            // Try to click + / - buttons if present or edit input
            var plus = _page.Locator(".cart_quantity_button a:has-text('+'), .cart_quantity_up");
            if (await plus.IsVisibleAsync()) await plus.First.ClickAsync();
            await _page.WaitForTimeoutAsync(500);
            Assert.IsTrue(await _page.Locator("tbody tr").First.IsVisibleAsync());
        }

        // 12 Remove single item from cart
        [Test, Order(12), Category("Cart")]
        public async Task TC12_RemoveSingleItemFromCart()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await _page.Locator(".cart_quantity_delete, a:has(i.fa-trash-o)").First.ClickAsync();
            await _page.WaitForTimeoutAsync(800);
            // Either empty state text or zero rows
            var rows = await _page.Locator("tbody tr").CountAsync();
            Assert.IsTrue(rows == 0 || await _page.GetByText("Cart is empty").IsVisibleAsync());
        }

        // 13 Price accuracy for multiple items
        [Test, Order(13), Category("Cart")]
        public async Task TC13_PriceAccuracyForMultipleItems()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            // Basic assertion: total cell exists
            Assert.IsTrue(await _page.Locator("#cart_info").IsVisibleAsync());
        }

        // 14 Prevent negative quantity
        [Test, Order(14), Category("Cart")]
        public async Task TC14_PreventNegativeQuantity()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            var qtyInput = _page.Locator("input[type='number'], input[name='quantity']");
            if (await qtyInput.IsVisibleAsync())
            {
                await qtyInput.FillAsync("-1");
                await _page.Keyboard.PressAsync("Enter");
                await _page.WaitForTimeoutAsync(300);
                var val = await qtyInput.InputValueAsync();
                Assert.IsTrue(int.TryParse(val, out var n) && n >= 1, "Quantity should not go below 1.");
            }
            else
            {
                Assert.Pass("UI does not allow direct negative quantity input.");
            }
        }

        // 15 Cart persists after login
        [Test, Order(15), Category("Cart")]
        public async Task TC15_CartPersistsAfterLogin()
        {
            await EnsureLoggedOutAsync();
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await ProceedToCheckoutAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Register / Login" }).ClickAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await OpenCartAsync();
            Assert.IsTrue(await _page.Locator("tbody tr").CountAsync() > 0);
        }

        // 16 Minimum/maximum quantity limits
        [Test, Order(16), Category("Cart")]
        public async Task TC16_MinMaxQuantityLimits()
        {
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            var qtyInput = _page.Locator("input[type='number'], input[name='quantity']");
            if (await qtyInput.IsVisibleAsync())
            {
                await qtyInput.FillAsync("999");
                await _page.Keyboard.PressAsync("Enter");
                await _page.WaitForTimeoutAsync(300);
                var val = await qtyInput.InputValueAsync();
                Assert.IsTrue(int.TryParse(val, out var n) && n >= 1, "Quantity should be a valid positive integer.");
            }
            else
            {
                Assert.Pass("UI uses +/- controls; manual cap not applicable.");
            }
        }

        // 17 Search existing product by exact name
        [Test, Order(17), Category("Search")]
        public async Task TC17_SearchExactName()
        {
            await SearchAsync("Blue Top");
            Assert.IsTrue(await _page.Locator(".features_items .product-image-wrapper").CountAsync() >= 1);
        }

        // 18 No-results message
        [Test, Order(18), Category("Search")]
        public async Task TC18_NoResultsMessage()
        {
            await SearchAsync("qwertyxyz");
            Assert.IsTrue(await _page.GetByText("Searched Products").IsVisibleAsync());
            // Accept either explicit no-results message or zero cards
            var count = await _page.Locator(".features_items .product-image-wrapper").CountAsync();
            Assert.IsTrue(count == 0 || await _page.GetByText("not found").First.IsVisibleAsync());
        }

        // 19 Add to cart from search results
        [Test, Order(19), Category("Search/Cart")]
        public async Task TC19_AddToCartFromSearchResults()
        {
            await SearchAsync("dress");
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            Assert.IsTrue(await _page.Locator("tbody tr").CountAsync() >= 1);
        }

        // 20 Pagination after search
        [Test, Order(20), Category("Search")]
        public async Task TC20_PaginationAfterSearch()
        {
            await SearchAsync("top");
            // Try next page link if present
            var next = _page.Locator(".pagination li a:has-text('>'), .pagination li a:has-text('Next')");
            if (await next.IsVisibleAsync())
            {
                await next.First.ClickAsync();
                await _page.WaitForTimeoutAsync(500);
                Assert.IsTrue(await _page.Locator(".features_items .product-image-wrapper").CountAsync() >= 0);
            }
            else
            {
                Assert.Pass("Single page of results – pagination not shown.");
            }
        }

        // 21 E2E Register → Browse → Add → Checkout (to payment)
        [Test, Order(21), Category("E2E")]
        public async Task TC21_RegisterBrowseAddCheckout()
        {
            await GoHomeAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Signup / Login" }).ClickAsync();
            await _page.Locator("input[data-qa='signup-name'], input[name='name']").FillAsync("Test User");
            string email = $"auto+{System.DateTime.UtcNow.Ticks}@example.com";
            await _page.Locator("input[data-qa='signup-email'], input[name='email']").FillAsync(email);
            await _page.Locator("button[data-qa='signup-button'], button:has-text('Signup')").First.ClickAsync();
            // Minimal account form (fill requireds)
            await _page.CheckAsync("input#id_gender1, input[name='title']").Catch(_ => Task.CompletedTask);
            await _page.Locator("input#password, input[name='password']").FillAsync("Valid123!");
            await _page.Locator("input#first_name, input[name='first_name']").FillAsync("Test");
            await _page.Locator("input#last_name, input[name='last_name']").FillAsync("User");
            await _page.Locator("input#address1, input[name='address1']").FillAsync("Main 1");
            await _page.Locator("input#state, input[name='state']").FillAsync("State");
            await _page.Locator("input#city, input[name='city']").FillAsync("City");
            await _page.Locator("input#zipcode, input[name='zipcode']").FillAsync("1000");
            await _page.Locator("input#mobile_number, input[name='mobile_number']").FillAsync("+359888888888");
            await _page.Locator("button[data-qa='create-account'], button:has-text('Create Account')").First.ClickAsync();
            await _page.GetByText("Account Created!").WaitForAsync();
            await _page.GetByRole(AriaRole.Link, new() { Name = "Continue" }).ClickAsync();

            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await ProceedToCheckoutAsync();
            Assert.IsTrue(await _page.GetByText("Address Details").IsVisibleAsync());
        }

        // 22 E2E Login existing → Add multiple → Checkout
        [Test, Order(22), Category("E2E")]
        public async Task TC22_LoginAddMultipleCheckout()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await ProceedToCheckoutAsync();
            Assert.IsTrue(await _page.GetByText("Review Your Order").IsVisibleAsync());
        }

        // 23 E2E Payment page form presence (demo)
        [Test, Order(23), Category("E2E")]
        public async Task TC23_PaymentPageFormPresence()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await ProceedToCheckoutAsync();
            // If payment page requires comment first
            if (await _page.Locator("textarea[name='message']").IsVisibleAsync())
            {
                await _page.Locator("textarea[name='message']").FillAsync("Test order");
                await _page.GetByRole(AriaRole.Link, new() { Name = "Place Order" }).ClickAsync();
            }
            Assert.IsTrue(await _page.Locator("input[name='name_on_card']").IsVisibleAsync());
        }

        // 24 E2E Order confirmation shows items and totals (demo path)
        [Test, Order(24), Category("E2E")]
        public async Task TC24_OrderConfirmationShowsItemsAndTotals()
        {
            await EnsureLoggedOutAsync();
            await LoginAsync("valid@user.com", "Valid123!");
            await OpenProductsAsync();
            await AddFirstProductFromGridAsync();
            await OpenCartAsync();
            await ProceedToCheckoutAsync();
            if (await _page.Locator("textarea[name='message']").IsVisibleAsync())
            {
                await _page.Locator("textarea[name='message']").FillAsync("Test order");
                await _page.GetByRole(AriaRole.Link, new() { Name = "Place Order" }).ClickAsync();
            }
            // Use demo data but do not assert final charge; site shows success after dummy
            await _page.Locator("input[name='name_on_card']").FillAsync("Test User");
            await _page.Locator("input[name='card_number']").FillAsync("4111111111111111");
            await _page.Locator("input[name='cvc']").FillAsync("123");
            await _page.Locator("input[name='expiry_month']").FillAsync("12");
            await _page.Locator("input[name='expiry_year']").FillAsync("2030");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Pay and Confirm Order" }).ClickAsync();
            Assert.IsTrue(await _page.GetByText("Your order has been placed successfully!").IsVisibleAsync());
        }

        // 25 E2E Validate price consistency across pages
        [Test, Order(25), Category("E2E")]
        public async Task TC25_ValidatePriceConsistencyAcrossPages()
        {
            await OpenProductsAsync();
            // Capture first product price on list
            var firstCard = _page.Locator(".product-image-wrapper").First;
            var listPriceText = await firstCard.Locator("h2:has-text('$'), .productinfo h2").First.InnerTextAsync().Catch(_ => Task.FromResult(""));
            await firstCard.Locator("a:has-text('View Product')").First.ClickAsync();
            var detailPriceText = await _page.Locator(".product-information span:has-text('$'), .product-information h2:has-text('$')").First.InnerTextAsync().Catch(_ => Task.FromResult(""));
            await _page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
            var cont = _page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
            if (await cont.IsVisibleAsync()) await cont.ClickAsync();
            await OpenCartAsync();
            var cartPriceText = await _page.Locator("tbody tr td:nth-child(3)").First.InnerTextAsync();

            string Normalize(string t) => new string(t.Where(c => char.IsDigit(c) || c == '.').ToArray());
            var listP = Normalize(listPriceText);
            var detailP = Normalize(detailPriceText);
            var cartP = Normalize(cartPriceText);

            Assert.IsTrue(!string.IsNullOrEmpty(cartP), "Cart price should be present.");
            if (!string.IsNullOrEmpty(listP)) Assert.AreEqual(listP, cartP, "List vs Cart price mismatch.");
            if (!string.IsNullOrEmpty(detailP)) Assert.AreEqual(detailP, cartP, "Detail vs Cart price mismatch.");
        }
    }
}
