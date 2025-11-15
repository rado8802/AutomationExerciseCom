using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class CartPage: BasePage
    {
        public CartPage(IPage page) : base(page) { }

        public ILocator CartHeader => Page.Locator("h2:has-text('Shopping Cart')");
        public ILocator CartTable => Page.Locator("#cart_info_table");
        public ILocator ProceedToCheckoutBtn => Page.GetByRole(AriaRole.Link, new() { Name = "Proceed To Checkout" });

        public async Task OpenAsync()
        {
            await Page.GotoAsync("https://automationexercise.com/view_cart");
        }
    }
}
