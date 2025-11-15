using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace AutomationExerciseTests.Tests
{
    [TestFixture]
    public class ContactTests: TestBase
    {
        [Test, Category("Contact")]
        public async Task SubmitContactForm_ShouldShowSuccessMessage()
        {
            Console.WriteLine("🌐 Opening Contact Us page...");
            await Page.GotoAsync("https://automationexercise.com/contact_us", new() { WaitUntil = WaitUntilState.NetworkIdle });

            // 🔹 Премахваме всички блокиращи елементи (cookie overlay, popups, ads)
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
            Console.WriteLine("✅ Overlays removed.");

            // 🔹 Попълваме формата
            await Page.FillAsync("[name='name']", "Radoslav");
            await Page.FillAsync("[name='email']", "radoslav@example.com");
            await Page.FillAsync("[name='subject']", "Playwright QA Test");
            await Page.FillAsync("#message", "Това е тестово съобщение от Playwright automation.");

            // 🔹 Проверяваме дали бутонът е блокиран от overlay
            var submitButton = Page.Locator("input[name='submit']");
            if (!await submitButton.IsVisibleAsync())
            {
                Console.WriteLine("⚠️ Submit button not visible — trying to remove overlays again...");
                await Page.EvaluateAsync(@"() => {
                    const overlays = document.querySelectorAll('.fc-dialog-overlay, .fc-consent-root, .modal-backdrop');
                    overlays.forEach(e => e.remove());
                }");
            }

            // 🔹 Прихващаме alert диалога
            Page.Dialog += async (_, dialog) =>
            {
                Console.WriteLine($"⚠️ Alert shown: {dialog.Message}");
                await dialog.AcceptAsync();
            };

            Console.WriteLine("📤 Clicking Submit button...");
            await submitButton.ClickAsync(new() { Force = true });

            // 🔹 Изчакваме съобщението за успех
            var successMessage = Page.Locator(".status:has-text('Success')");
            await Expect(successMessage).ToBeVisibleAsync(new() { Timeout = 20000 });
            Console.WriteLine("✅ Success message is visible!");

            // 🔹 Скриншот след успех
            await Page.ScreenshotAsync(new() { Path = $"screenshots/ContactForm_{DateTime.Now:yyyyMMdd_HHmmss}.png" });

            Page.Dialog -= async (_, dialog) => await dialog.AcceptAsync();
        }
    }
}
