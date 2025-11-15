using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Pages
{
    public class BasePage
    {
        protected readonly IPage Page;

        public BasePage(IPage page)
        {
            Page = page;
        }

        // 🧩 Премахва overlay прозорци (например cookie consent или popups)
        public static async Task ForceClearOverlaysAsync()
        {
            // Изчакваме, ако има overlay с класове, които блокират кликове
            var context = PlaywrightSingleton.CurrentContext;
            if (context == null)
                return;

            foreach (var page in context.Pages)
            {
                try
                {
                    // Примери за блокиращи елементи (можеш да добавяш още)
                    string[] overlays = {
                        ".fc-dialog-overlay",
                        ".popup",
                        "#adblock-popup",
                        ".modal-backdrop",
                        "#cookieConsent",
                        ".newsletter-popup"
                    };

                    foreach (var selector in overlays)
                    {
                        var elements = await page.QuerySelectorAllAsync(selector);
                        foreach (var element in elements)
                        {
                            await element.EvaluateAsync("el => el.remove()");
                        }
                    }
                }
                catch
                {
                    // Игнорирай грешки от несъществуващи елементи
                }
            }
        }
    }

    // 🔹 Singleton за достъп до текущия BrowserContext
    public static class PlaywrightSingleton
    {
        public static IBrowserContext? CurrentContext { get; set; }
    }
}
