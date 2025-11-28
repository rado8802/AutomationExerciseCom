using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AutomationExerciseTests.Pages
{
    public class CartPage
    {
        private readonly IPage _page;

        public CartPage(IPage page)
        {
            _page = page;
        }

        public async Task OpenAsync()
        {
            await _page.GotoAsync("https://automationexercise.com/view_cart");
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var rows = _page.Locator("tr[id^='product-']");
            return await rows.CountAsync();
        }
    }
}
