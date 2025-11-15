using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class CheckoutPage: BasePage
    {
        public CheckoutPage(IPage page) : base(page) { }

        public ILocator PlaceOrderButton => Page.GetByRole(AriaRole.Link, new() { Name = "Place Order" });

        public async Task OpenAsync()
        {
            await Page.GotoAsync("https://automationexercise.com/checkout");
        }
    }
}
