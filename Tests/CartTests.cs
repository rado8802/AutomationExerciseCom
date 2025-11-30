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
            public async Task Test_09_SameProductTwice_ShouldMergeIntoSingleRow() 
            {
                // Open products page
                await GoToProducts();

                // Add same product twice
                await AddProduct(1);
                await AddProduct(1);

                // Open cart
                await Page.GotoAsync("https://automationexercise.com/view_cart");
                await ClearOverlays();

                // Count cart rows
                int rows = await Page.Locator("tr[id^='product-']").CountAsync();

                // Read quantity of the first row
                var qtyText = await Page.Locator("tbody tr").First
                    .Locator("td:nth-child(4)").InnerTextAsync();
                int qty = int.Parse(new string(qtyText.Where(char.IsDigit).ToArray()));

                // Assert: the cart should have only 1 row
                Assert.That(rows, Is.EqualTo(1), "Same product should not create multiple cart rows.");

                // Assert: quantity should be 2
                Assert.That(qty, Is.GreaterThanOrEqualTo(2),
                    "Quantity should increase when adding the same product twice.");
            }
            [Test, Order(10)]
            public async Task Test_10_RemovingOneProduct_ShouldUpdateCartTotalCorrectly()
            {
                // Go to products
                await GoToProducts();

                // Add two different products
                await AddProduct(1);
                await AddProduct(2);

                // Open cart
                await Page.GotoAsync("https://automationexercise.com/view_cart");
                await ClearOverlays();

                var rows = Page.Locator("tbody tr");
                int rowCountBefore = await rows.CountAsync();
                Assert.That(rowCountBefore, Is.GreaterThanOrEqualTo(2), "Expected at least 2 rows before deletion.");

                // Calculate expected remaining total AFTER removing first product
                // 1) Read second row price
                string priceText2 = await rows.Nth(1).Locator("td:nth-child(3)").InnerTextAsync();
                decimal price2 = decimal.Parse(new string(priceText2.Where(char.IsDigit).ToArray()));

                // 2) Read second row qty
                string qtyText2 = await rows.Nth(1).Locator("td:nth-child(4)").InnerTextAsync();
                int qty2 = int.Parse(new string(qtyText2.Where(char.IsDigit).ToArray()));

                decimal expectedRemainingTotal = price2 * qty2;

                // Remove FIRST product
                await rows.First.Locator("a.cart_quantity_delete").ClickAsync();
                await Page.WaitForTimeoutAsync(1500);

                // Read displayed total from UI (remaining row)
                string remainingTotalText = await rows.First.Locator("td:nth-child(5)").InnerTextAsync();
                decimal remainingTotal = decimal.Parse(new string(remainingTotalText.Where(char.IsDigit).ToArray()));

                Assert.That(remainingTotal, Is.EqualTo(expectedRemainingTotal),
                    $"Expected total after deletion: {expectedRemainingTotal}, but UI shows {remainingTotal}");
            }
            [Test, Order(11)]
            public async Task Test_11_ProductNamesInCart_ShouldMatchAddedProducts()
            {
                await GoToProducts();

                // --- Add Product 1 ---
                var product1Name = await Page.Locator(".productinfo.text-center").Nth(0)
                    .Locator("p").InnerTextAsync();
                await AddProduct(1);

                // --- Add Product 2 ---
                var product2Name = await Page.Locator(".productinfo.text-center").Nth(1)
                    .Locator("p").InnerTextAsync();
                await AddProduct(2);

                // Open cart
                await Page.GotoAsync("https://automationexercise.com/view_cart");
                await ClearOverlays();

                // Read product names in cart table
                var cartProduct1 = await Page.Locator("tbody tr").Nth(0)
                    .Locator("td:nth-child(2)").InnerTextAsync();

                var cartProduct2 = await Page.Locator("tbody tr").Nth(1)
                    .Locator("td:nth-child(2)").InnerTextAsync();

                // Assertions
                Assert.That(cartProduct1.Trim(), Does.Contain(product1Name.Trim()),
                    "Product 1 name in cart does not match the one added.");

                Assert.That(cartProduct2.Trim(), Does.Contain(product2Name.Trim()),
                    "Product 2 name in cart does not match the one added.");
            }
    }
}
