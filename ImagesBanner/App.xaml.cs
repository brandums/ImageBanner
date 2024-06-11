namespace ImagesBanner
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new ImageEditor();
        }
    }
}