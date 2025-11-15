using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using AutomationExerciseTests.Util; // ✅ за достъп до TestData

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    public class LoginTests: TestBase
    {
        [Test, Category("Login")]
        public async Task LoginWithValidCredentials_ShouldLoginSuccessfully()
        {
            Console.WriteLine("🌐 Opening Login page...");
            await Page.GotoAsync("https://automationexercise.com/login", new() { WaitUntil = WaitUntilState.NetworkIdle });

            await RemoveOverlaysAsync();

            Console.WriteLine($"🧩 Using credentials: {TestData.ValidEmail}");
            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            var loginButton = Page.Locator("button[data-qa='login-button']");
            await loginButton.ClickAsync(new() { Force = true });

            var loggedInUser = Page.Locator("a:has-text('Logged in as')");
            await Expect(loggedInUser).ToBeVisibleAsync(new() { Timeout = 20000 });

            Console.WriteLine("✅ Login successful!");
            await Page.ScreenshotAsync(new() { Path = $"screenshots/Login_Success_{DateTime.Now:yyyyMMdd_HHmmss}.png" });
        }

        [Test, Category("Login")]
        public async Task LoginWithInvalidCredentials_ShouldShowError()
        {
            Console.WriteLine("🌐 Opening Login page...");
            await Page.GotoAsync("https://automationexercise.com/login", new() { WaitUntil = WaitUntilState.NetworkIdle });

            await RemoveOverlaysAsync();

            Console.WriteLine("❌ Filling invalid credentials...");
            await Page.FillAsync("input[data-qa='login-email']", "invalid@example.com");
            await Page.FillAsync("input[data-qa='login-password']", "wrongpass");

            var loginButton = Page.Locator("button[data-qa='login-button']");
            await loginButton.ClickAsync(new() { Force = true });

            var errorMessage = Page.Locator("p:has-text('Your email or password is incorrect!')");
            await Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 15000 });

            Console.WriteLine("✅ Error message displayed as expected!");
            await Page.ScreenshotAsync(new() { Path = $"screenshots/Login_Invalid_{DateTime.Now:yyyyMMdd_HHmmss}.png" });
        }

        // 🧰 Премахва cookie consent и overlay елементи
        private async Task RemoveOverlaysAsync()
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
                selectors.forEach(sel => document.querySelectorAll(sel).forEach(e => e.remove()));
            }");
            Console.WriteLine("✅ Overlays cleared.");
        }
    }
}
