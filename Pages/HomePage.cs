using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Pages
{
    public class HomePage : BasePage
    {
        public HomePage(IPage page) : base(page) { }

        public async Task NavigateAsync()
        {
            await Page.GotoAsync("https://automationexercise.com/");
            await BasePage.ForceClearOverlaysAsync();
        }

        public async Task GoToProductsPageAsync()
        {
            await Page.ClickAsync("a[href='/products']");
        }

        public async Task GoToContactPageAsync()
        {
            await Page.ClickAsync("a[href='/contact_us']");
        }

        public async Task GoToCartPageAsync()
        {
            await Page.ClickAsync("a[href='/view_cart']");
        }
    }
}
