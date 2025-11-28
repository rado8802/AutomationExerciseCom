using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using AutomationExerciseTests.Util;
using System.Text.RegularExpressions;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    [Category("Login")]
    public class LoginTests : TestBase
    {
        // =====================================================
        // FIXED VALID LOGIN TEST
        // =====================================================

        [Test, Category("Login")]
        public async Task LoginWithValidCredentials_ShouldLoginSuccessfully()
        {
            await Page.GotoAsync("https://automationexercise.com/login", new()
            {
                WaitUntil = WaitUntilState.Load   // ✅ FIX: NetworkIdle removed
            });

            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page.Locator("a:has-text('Logged in as')"))
                .ToBeVisibleAsync(new() { Timeout = 20000 });
        }


        // =====================================================
        // ORIGINAL INVALID LOGIN TEST
        // =====================================================

        [Test, Category("Login")]
        public async Task LoginWithInvalidCredentials_ShouldShowError()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", "invalid@example.com");
            await Page.FillAsync("input[data-qa='login-password']", "wrongpass");

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page.Locator("p:has-text('Your email or password is incorrect!')"))
                .ToBeVisibleAsync(new() { Timeout = 15000 });
        }


        // =====================================================
        // FIXED EXTENDED LOGIN TESTS
        // =====================================================

        [Test]
        public async Task Login_03_BlankEmailAndPassword_ShouldStayOnLoginPage()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", "");
            await Page.FillAsync("input[data-qa='login-password']", "");

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_04_BlankPassword_ShouldStayOnLoginPage()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", "");

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_05_BlankEmail_ShouldStayOnLoginPage()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", "");
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_06_InvalidEmailFormat_ShouldStayOnLoginPage()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", "invalid-format");
            await Page.FillAsync("input[data-qa='login-password']", "123456");

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_07_SqlInjection_ShouldNotLogin()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", "' OR '1'='1");
            await Page.FillAsync("input[data-qa='login-password']", "' OR '1'='1");

            await Page.ClickAsync("button[data-qa='login-button']");

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_08_XssInjection_ShouldNotExecute()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            string payload = "<script>alert('XSS')</script>";

            await Page.FillAsync("input[data-qa='login-email']", payload);
            await Page.FillAsync("input[data-qa='login-password']", payload);

            await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_09_CaseInsensitiveEmail_ShouldFail()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            string upperEmail = TestData.ValidEmail.ToUpper();

            await Page.FillAsync("input[data-qa='login-email']", upperEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            await Page.ClickAsync("button[data-qa='login-button']");

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_10_Logout_ShouldRedirectToLoginPage()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            await Page.ClickAsync("button[data-qa='login-button']");
            await Page.Locator("a:has-text('Logout')").ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_11_RememberMe_NotAvailable_ShouldNotBreak()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            bool exists = await Page.Locator("#remember").CountAsync() > 0;
            Assert.IsFalse(exists);
        }

        [Test]
        public async Task Login_12_LoginButtonShouldBeEnabled_WhenFieldsPopulated()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            var btn = Page.Locator("button[data-qa='login-button']");

            await Page.FillAsync("input[data-qa='login-email']", "test@example.com");
            await Page.FillAsync("input[data-qa='login-password']", "123456");

            await Expect(btn).ToBeEnabledAsync();
        }

        [Test]
        public async Task Login_13_PasswordMinLength_NoClientValidation_ShouldStillSubmit()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", "1");

            await Page.ClickAsync("button[data-qa='login-button']");

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_14_MultipleRapidLoginAttempts_ShouldRemainOnLogin()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            for (int i = 0; i < 5; i++)
            {
                await Page.FillAsync("input[data-qa='login-email']", "invalid@example.com");
                await Page.FillAsync("input[data-qa='login-password']", "wrongpass");
                await Page.ClickAsync("button[data-qa='login-button']", new() { Force = true });
            }

            await Expect(Page).ToHaveURLAsync(new Regex(".*login"));
        }

        [Test]
        public async Task Login_15_LoginSessionShouldPersist_AfterNavigation()
        {
            await Page.GotoAsync("https://automationexercise.com/login");
            await RemoveOverlaysAsync();

            await Page.FillAsync("input[data-qa='login-email']", TestData.ValidEmail);
            await Page.FillAsync("input[data-qa='login-password']", TestData.ValidPassword);

            await Page.ClickAsync("button[data-qa='login-button']");

            await Page.GotoAsync("https://automationexercise.com/products");

            await Expect(Page.Locator("a:has-text('Logged in as')")).ToBeVisibleAsync();
        }


        // =====================================================
        // HELPERS
        // =====================================================

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
        }
    }
}
