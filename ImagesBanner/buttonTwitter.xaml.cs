namespace ImagesBanner;

public partial class buttonTwitter : ContentPage
{
    bool isPlus = true;

    public buttonTwitter()
    {
        InitializeComponent();
    }
    private async void OnButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        // Crear una tarea de animación
        var growAnimation = button.ScaleTo(1.125, 150);
        var rotateAnimation = button.RotateTo(45, 150);

        // Ejecutar animaciones en paralelo
        await Task.WhenAll(growAnimation, rotateAnimation);

        button.Text = isPlus ? "x" : "+";
        isPlus = !isPlus;

        var shrinkAnimation = button.ScaleTo(1, 150);
        var rotateBackAnimation = button.RotateTo(0, 150);

        await Task.WhenAll(shrinkAnimation, rotateBackAnimation);
    }
}