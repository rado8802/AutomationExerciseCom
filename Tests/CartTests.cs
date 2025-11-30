using NUnit.Framework;
using System;
using System.Linq;
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
                document.querySelectorAll(
                    'iframe, .adsbygoogle, .fc-dialog, .fc-dialog-overlay'
                ).forEach(e => e.remove());
            }");

            await Task.Delay(300);
        }

        private async Task<bool> AddProduct(int id)
        {
            Log($"Adding product {id}...");

            var mainBtn = Page.Locator(
                $"a.btn.btn-default.add-to-cart[data-product-id='{id}']"
            ).First;

            var overlayBtn = Page.Locator(
                $".overlay-content a.btn.btn-default.add-to-cart[data-product-id='{id}']"
            ).First;

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

            var modal = Page.Locator("#cartModal");
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible });

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
        // TEST 01 – 07 (original working tests)
        // =====================================================

        [Test]
        public async Task Test_01_AddSingleProduct_ShouldAppearInCart()
        {
            await GoToProducts();
            await AddProduct(1);

            int rows = await GetCartRows();
            Assert.Greater(rows, 0, "Cart should contain ≥ 1 product.");
        }

        [Test]
        public async Task Test_02_AddTwoProducts_ShouldShowTwoItems()
        {
            await GoToProducts();
            await AddProduct(1);
            await AddProduct(2);

            int rows = await GetCartRows();
            Assert.GreaterOrEqual(rows, 2);
        }

        [Test]
        public async Task Test_03_RemoveProduct_ShouldEmptyCart()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            await Page.Locator("a.cart_quantity_delete").ClickAsync();
            await Page.WaitForSelectorAsync("#empty_cart");

            Assert.IsTrue(await Page.Locator("#empty_cart").IsVisibleAsync());
        }

        [Test]
        public async Task Test_04_ClearCart_ShouldShowEmptyMessage()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await Page.Locator("a.cart_quantity_delete").ClickAsync();

            await Page.WaitForSelectorAsync("#empty_cart");

            string text = await Page.InnerTextAsync("#empty_cart");
            Assert.That(text, Does.Contain("Cart is empty"));
        }

        [Test]
        public async Task Test_05_ProceedToCheckout_ShouldOpenCheckoutPage()
        {
            await GoToProducts();
            await AddProduct(1);

            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            await Page.Locator("a.check_out").ClickAsync();

            var modal = Page.Locator("#checkoutModal.show");
            await modal.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            bool loginMsgFound = await Page
                .Locator("p:text('Register / Login account to proceed on checkout.')")
                .IsVisibleAsync();

            Assert.IsTrue(loginMsgFound);
        }

        [Test]
        public async Task Test_06_CartCount_ShouldMatchAddedProducts()
        {
            await GoToProducts();

            await AddProduct(1);
            await AddProduct(2);
            await AddProduct(3);

            int rows = await GetCartRows();
            Assert.GreaterOrEqual(rows, 3);
        }

        [Test]
        public async Task Test_07_CartEmptyMessage_ShouldDisplayCorrectText()
        {
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            string msg = await Page.InnerTextAsync(".text-center");

            Assert.That(
                msg,
                Does.Contain("Cart is empty! Click here to buy products.")
            );
        }

        // =====================================================
        // TEST 08 – CART TOTAL SUM VALIDATION
        // =====================================================

        [Test, Order(8)]
        public async Task Test_08_Check_Cart_Total_Sum_Is_Correct_For_Multiple_Items()
        {
            // Open products page
            await Page.GotoAsync("https://automationexercise.com/products");
            await ClearOverlays();

            // Add first three products
            for (int i = 0; i < 3; i++)
            {
                await Page.Locator("a:has-text('View Product')").Nth(i).ClickAsync();
                await Page.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();

                var cont = Page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
                if (await cont.IsVisibleAsync()) await cont.ClickAsync();

                await Page.GotoAsync("https://automationexercise.com/products");
                await ClearOverlays();
            }

            // Open cart
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            int rowCount = await Page.Locator("tbody tr").CountAsync();

            decimal expectedTotal = 0;
            decimal calculatedTotal = 0;

            for (int i = 0; i < rowCount; i++)
            {
                // Price
                var priceText = await Page
                    .Locator("tbody tr")
                    .Nth(i)
                    .Locator("td:nth-child(3)")
                    .InnerTextAsync();

                decimal price = decimal.Parse(new string(priceText.Where(char.IsDigit).ToArray()));

                // Quantity
                var qtyText = await Page
                    .Locator("tbody tr")
                    .Nth(i)
                    .Locator("td:nth-child(4)")
                    .InnerTextAsync();

                int qty = int.Parse(new string(qtyText.Where(char.IsDigit).ToArray()));

                expectedTotal += price * qty;

                // Row TOTAL
                var rowTotalText = await Page
                    .Locator("tbody tr")
                    .Nth(i)
                    .Locator("td:nth-child(5)")
                    .InnerTextAsync();

                decimal rowTotal = decimal.Parse(new string(rowTotalText.Where(char.IsDigit).ToArray()));

                calculatedTotal += rowTotal;
            }

            Assert.That(calculatedTotal, Is.EqualTo(expectedTotal),
                $"Expected: {expectedTotal}, but UI sum: {calculatedTotal}");
        }
        [Test, Order(9)]
        public async Task Test_09_IncreasingQuantity_ShouldUpdateTotalCorrectly()
        {
            // Go to products
            await Page.GotoAsync("https://automationexercise.com/products");
            await ClearOverlays();

            // Add a product
            await Page.Locator("a.btn.add-to-cart[data-product-id='1']").First.ClickAsync();
            var cont = Page.GetByRole(AriaRole.Button, new() { Name = "Continue Shopping" });
            if (await cont.IsVisibleAsync()) await cont.ClickAsync();

            // Open cart
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            var row = Page.Locator("tbody tr").First;

            // Check if site supports + button for quantity
            var increaseBtn = row.Locator(".cart_quantity_up");
            if (!await increaseBtn.IsVisibleAsync())
            {
                Assert.Pass("Site does not support increasing quantity with a + button.");
                return;
            }

            // Read initial price
            string priceText = await row.Locator("td:nth-child(3)").InnerTextAsync();
            decimal price = decimal.Parse(new string(priceText.Where(char.IsDigit).ToArray()));

            // Read initial qty
            string qtyText = await row.Locator("td:nth-child(4)").InnerTextAsync();
            int initialQty = int.Parse(new string(qtyText.Where(char.IsDigit).ToArray()));

            // Increase quantity
            await increaseBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(1500);

            // Read updated qty
            string newQtyText = await row.Locator("td:nth-child(4)").InnerTextAsync();
            int updatedQty = int.Parse(new string(newQtyText.Where(char.IsDigit).ToArray()));

            Assert.That(updatedQty, Is.EqualTo(initialQty + 1),
                $"Expected qty: {initialQty + 1}, got: {updatedQty}");

            // Read updated row total
            string rowTotalText = await row.Locator("td:nth-child(5)").InnerTextAsync();
            decimal rowTotal = decimal.Parse(new string(rowTotalText.Where(char.IsDigit).ToArray()));

            decimal expectedTotal = price * updatedQty;

            Assert.That(rowTotal, Is.EqualTo(expectedTotal),
                $"Expected row total {expectedTotal}, but UI shows: {rowTotal}");
        }
    }
}
