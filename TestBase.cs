using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace AutomationExerciseTests
{
    public class TestBase
    {
        protected IPlaywright _playwright;
        protected IBrowser _browser;
        protected IBrowserContext _context;
        protected IPage Page;

        [SetUp]
        public async Task SetUp()
        {
            // Инициализация на Playwright и браузъра
            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(new()
            {
                Headless = false, // Показва браузъра
                SlowMo = 150       // Забавя стъпките, за да виждаш какво прави тестът
            });

            // Създаваме контекст с видео запис
            _context = await _browser.NewContextAsync(new()
            {
                RecordVideoDir = "videos/",
                RecordVideoSize = new() { Width = 1280, Height = 720 }
            });

            // Създаваме нова страница
            Page = await _context.NewPageAsync();

            Console.WriteLine("✅ Browser launched and test started...");
        }

        [TearDown]
        public async Task TearDown()
        {
            // Заснемаме screenshot при грешка или завършен тест
            var testName = TestContext.CurrentContext.Test.Name;
            var screenshotPath = $"screenshots/{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });

            // Извеждаме пътя до видеото
            var videoPath = await Page.Video?.PathAsync();
            if (videoPath != null)
                Console.WriteLine($"🎥 Video saved: {videoPath}");

            Console.WriteLine($"📸 Screenshot saved: {screenshotPath}");

            // Затваряме контекста и браузъра
            await _context.CloseAsync();
            await _browser.CloseAsync();
            _playwright.Dispose();

            Console.WriteLine("✅ Browser closed and test finished.");
        }
    }
}
