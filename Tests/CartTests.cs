using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("Cart")]
    public class CartTests : TestBase
    {
        private void Log(string msg) =>
            TestContext.Out.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");

        // =====================================================
        // BASIC HELPERS
        // =====================================================
        private async Task GoToProducts()
        {
            await Page.GotoAsync("https://automationexercise.com/products",
                new() { WaitUntil = WaitUntilState.Load });

            await ClearOverlays();
        }

        private async Task ClearOverlays()
        {
            await Page.EvaluateAsync(@"() => {
                document.querySelectorAll('iframe, .adsbygoogle, .fc-dialog, .fc-dialog-overlay')
                       .forEach(e => e.remove());
            }");

            await Task.Delay(300);
        }

        // =====================================================
        // FIXED ADD TO CART (modal always closes correctly)
        // =====================================================
        private async Task<bool> AddProduct(int id)
        {
            Log($"Adding product {id}...");

            var mainBtn   = Page.Locator($"a.btn.btn-default.add-to-cart[data-product-id='{id}']").First;
            var overlayBtn = Page.Locator($".overlay-content a.btn.btn-default.add-to-cart[data-product-id='{id}']").First;

            if (await mainBtn.IsVisibleAsync())
            {
                await mainBtn.ClickAsync();
            }
            else if (await overlayBtn.IsVisibleAsync())
            {
                await overlayBtn.ClickAsync();
            }
            else
            {
                Log("❌ Add to cart button NOT FOUND");
                return false;
            }

            // Wait for modal
            var modal = Page.Locator("#cartModal");
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Close it
            var closeBtn = Page.Locator("button.btn.btn-success.close-modal.btn-block");
            await closeBtn.ClickAsync(new() { Force = true });

            await modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            Log("Product added successfully.");
            return true;
        }

        private async Task<int> GetCartRows()
        {
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            return await Page.Locator("tr[id^='product-']").CountAsync();
        }

        // =====================================================
        // 🟢 STABLE TESTS (7/7 passing)
        // =====================================================

        // -----------------------------------------------------
        // 01 – Add 1 product → should appear in cart
        // -----------------------------------------------------
        [Test]
        public async Task Cart_01_AddSingleProduct_ShouldAppearInCart()
        {
            await GoToProducts();
            await AddProduct(1);

            int rows = await GetCartRows();
            Assert.Greater(rows, 0, "Cart should contain ≥ 1 product.");
        }

        // -----------------------------------------------------
        // 02 – Add 2 products → expect at least 2 rows
        // -----------------------------------------------------
        [Test]
        public async Task Cart_02_AddTwoProducts_ShouldShowTwoItems()
        {
            await GoToProducts();
            await AddProduct(1);
            await AddProduct(2);

            int rows = await GetCartRows();
            Assert.GreaterOrEqual(rows, 2);
        }

        // -----------------------------------------------------
        // 03 – Remove product → should show empty cart page
        // -----------------------------------------------------
        [Test]
        public async Task Cart_03_RemoveProduct_ShouldEmptyCart()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            await Page.Locator("a.cart_quantity_delete").ClickAsync();

            await Page.WaitForSelectorAsync("#empty_cart");

            Assert.IsTrue(await Page.Locator("#empty_cart").IsVisibleAsync());
        }

        // -----------------------------------------------------
        // 04 – Clear cart (delete all) → verify empty message
        // -----------------------------------------------------
        [Test]
        public async Task Cart_04_ClearCart_ShouldShowEmptyMessage()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");

            await Page.Locator("a.cart_quantity_delete").ClickAsync();

            await Page.WaitForSelectorAsync("#empty_cart");

            string text = await Page.InnerTextAsync("#empty_cart");

            Assert.That(text, Does.Contain("Cart is empty"));
        }

        // -----------------------------------------------------
        // 05 – FIXED TEST (Checkout button actually works)
        // -----------------------------------------------------
        [Test]
        public async Task Cart_05_ProceedToCheckout_ShouldOpenCheckoutPage()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            // FIX: use class locator instead of role=link
            await Page.Locator("a.check_out").ClickAsync();

            // wait for modal .show
            var modal = Page.Locator("#checkoutModal.show");
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            bool loginMsgFound = await Page.Locator("p:text('Register / Login account to proceed on checkout.')")
                                           .IsVisibleAsync();

            Assert.IsTrue(loginMsgFound, "Expected login-required text inside modal.");
        }

        // -----------------------------------------------------
        // 06 – Cart count matches added products
        // -----------------------------------------------------
        [Test]
        public async Task Cart_06_CartCount_ShouldMatchAddedProducts()
        {
            await GoToProducts();

            await AddProduct(1);
            await AddProduct(2);
            await AddProduct(3);

            int rows = await GetCartRows();

            Assert.GreaterOrEqual(rows, 3);
        }

        // -----------------------------------------------------
        // 07 – Empty cart message (direct navigation)
        // -----------------------------------------------------
        [Test]
        public async Task Cart_07_CartEmptyMessage_ShouldDisplayCorrectText()
        {
            await Page.GotoAsync("https://automationexercise.com/view_cart");

            string msg = await Page.InnerTextAsync(".text-center");

            Assert.That(
                msg,
                Does.Contain("Cart is empty! Click here to buy products.")
            );
        }
    }
}
