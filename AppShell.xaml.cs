namespace HerrJogging
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
        }
        public async Task NavigateTo(string route)
        {
            // Optional: Hier kannst du globales Logging, Checks oder ähnliches machen.
            await GoToAsync(route);
        }
    }
}
 