using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using AutomationExerciseTests.Util;
using static Microsoft.Playwright.Assertions;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("Checkout")]
    public class CheckoutTests : TestBase
    {
        private async Task ClearOverlays()
        {
            await Page.EvaluateAsync(@"() => {
                const selectors = [
                    '.fc-dialog-overlay',
                    '.fc-consent-root',
                    '.fc-dialog',
                    '.popup',
                    '.modal-backdrop',
                    'iframe',
                    '.adsbygoogle'
                ];
                selectors.forEach(sel =>
                    document.querySelectorAll(sel).forEach(e => e.remove())
                );
            }");

            await Page.WaitForTimeoutAsync(300);
        }

        // =====================================================
        // TEST 01 (валиден, стабилизиран)
        // =====================================================
        [Test, Category("Checkout")]
        public async Task Test_01_LoggedInUser_CanOpenCheckoutPage()
        {
            // Go to login
            await Page.GotoAsync("https://automationexercise.com/login");
            await ClearOverlays();

            // Login
            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);
            await Page.ClickAsync("button[data-qa='login-button']");

            // Wait for logged-in UI
            await Page.WaitForTimeoutAsync(1500);

            var loggedIn = Page.Locator("a:has-text('Logged in as')");
            Assert.IsTrue(await loggedIn.IsVisibleAsync(), "Login failed — user not logged in.");

            // Add product
            await Page.GotoAsync("https://automationexercise.com/products");
            await ClearOverlays();

            await Page.Locator("a.btn.btn-default.add-to-cart").First.ClickAsync();

            // 🔥 FIX: Remove modal fully (unbreakable)
            await Page.EvaluateAsync("document.querySelector('#cartModal')?.remove()");

            // Open cart
            await Page.GotoAsync("https://automationexercise.com/view_cart");
            await ClearOverlays();

            // Proceed to checkout
            await Page.Locator("a:has-text('Proceed To Checkout')").ClickAsync(new() { Force = true });

            // Expect checkout page
            await Page.WaitForURLAsync("**/checkout*", new() { Timeout = 15000 });
            Assert.That(Page.Url, Does.Contain("checkout"));
        }
    }
}
