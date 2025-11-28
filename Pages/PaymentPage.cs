using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class PaymentPage
    {
        private readonly IPage Page;
        public PaymentPage(IPage page) { Page = page; }

        public ILocator SuccessTitle => Page.Locator("[data-qa='order-placed']");

        public async Task FillPaymentFormAsync()
        {
            await Page.FillAsync("input[name='name_on_card']", "Test User");
            await Page.FillAsync("input[name='card_number']", "4242424242424242");
            await Page.FillAsync("input[name='cvc']", "123");
            await Page.FillAsync("input[name='expiry_month']", "12");
            await Page.FillAsync("input[name='expiry_year']", "2027");
        }

        public async Task SubmitPaymentAsync()
        {
            await Page.Locator("#submit").ClickAsync(new() { Force = true });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task AssertOrderSuccessAsync()
        {
            await SuccessTitle.WaitForAsync(new() { Timeout = 15000 });
        }
    }
}
