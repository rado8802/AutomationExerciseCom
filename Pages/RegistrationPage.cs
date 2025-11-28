using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutomationExerciseTests.Pages
{
    public class RegistrationPage : BasePage
    {
        public RegistrationPage(IPage page) : base(page) { }

        // --- Signup form (left side) ---
        public ILocator Name => Page.Locator("input[name='name']");
        public ILocator Email => Page.Locator("input[data-qa='signup-email']");
        public ILocator SignupBtn => Page.GetByRole(AriaRole.Button, new() { Name = "Signup" });

        // --- Account details form ---
        public ILocator TitleMr => Page.Locator("#id_gender1");
        public ILocator Password => Page.Locator("#password");
        public ILocator Firstname => Page.Locator("#first_name");
        public ILocator Lastname => Page.Locator("#last_name");
        public ILocator Address1 => Page.Locator("#address1");
        public ILocator State => Page.Locator("#state");
        public ILocator City => Page.Locator("#city");
        public ILocator Zip => Page.Locator("#zipcode");
        public ILocator Mobile => Page.Locator("#mobile_number");
        public ILocator CreateBtn => Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" });

        public ILocator AccountCreatedHeader => Page.GetByText("Account Created!");

        // -------------------------------------------------------
        // REQUIRED BY TESTS
        // -------------------------------------------------------

        // This is the equivalent of StartSignupAsync in old test suite
        public async Task StartRegistrationAsync(string name, string email)
        {
            await Name.FillAsync(name);
            await Email.FillAsync(email);
            await SignupBtn.ClickAsync();
        }

        // Fill the required fields
        public async Task FillMandatoryAccountFormAsync()
        {
            if (await TitleMr.IsVisibleAsync())
                await TitleMr.CheckAsync();

            await Password.FillAsync("Valid123!");

            await Firstname.FillAsync("QA");
            await Lastname.FillAsync("Automation");

            await Address1.FillAsync("Test Street 123");
            await State.FillAsync("Sofia");
            await City.FillAsync("Sofia");
            await Zip.FillAsync("1000");
            await Mobile.FillAsync("0888123456");

            await CreateBtn.ClickAsync();
        }

        // Validate account creation
        public async Task AssertAccountCreatedAsync()
        {
            await AccountCreatedHeader.WaitForAsync(new() { Timeout = 5000 });
        }
    }
}
