using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Util
{
    public static class Ui
    {
        /// <summary>
        /// Затваря най-често срещаните cookie/consent банери (FC/Sourcepoint).
        /// Без грешка, ако не съществуват.
        /// </summary>
        public static async Task DismissConsentAsync(IPage page)
        {
            // Дай шанс на overlay-а да се изрисува
            await page.WaitForTimeoutAsync(300);

            var candidates = new[]
            {
                "button:has-text('Accept All')",
                "button:has-text('Allow All')",
                "button:has-text('I Accept')",
                "button:has-text('AGREE')",
                ".fc-button.fc-cta-consent",
                ".fc-cta-consent",
                "#ez-accept-all"
            };

            foreach (var sel in candidates)
            {
                var b = page.Locator(sel).First;
                if (await b.IsVisibleAsync())
                {
                    try { await b.ClickAsync(new LocatorClickOptions { Trial = false }); }
                    catch { /* ignore */ }
                }
            }

            // Понякога остава overlay див — опитай да го затвориш
            var overlay = page.Locator(".fc-dialog-overlay, .sp_veil").First;
            if (await overlay.IsVisibleAsync())
            {
                var closeBtn = page.Locator("button[aria-label='Close'], .fc-close, .sp_choice_type_11").First;
                if (await closeBtn.IsVisibleAsync())
                {
                    try { await closeBtn.ClickAsync(); } catch { /* ignore */ }
                }
            }
        }
    }
}
