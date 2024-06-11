using Microsoft.Maui.Layouts;
using SkiaSharp;

namespace ImagesBanner;

public partial class ImageEditor : ContentPage
{
    private double offsetX, offsetY;
    private double rotationAngle = 0;
    private Button rotateButton;

    public ImageEditor()
    {
        InitializeComponent();
    }

    private async void SelectImage(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions();

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
               result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
            {
                var stream = await result.OpenReadAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var skStream = new SKManagedStream(memoryStream))
                using (var bitmap = SKBitmap.Decode(skStream))
                {
                    double originalWidth = bitmap.Width;
                    double originalHeight = bitmap.Height;
                    double aspectRatio = originalHeight / originalWidth;
                    double newWidth = container.Width;
                    double newHeight = newWidth * aspectRatio;

                    Img.WidthRequest = newWidth;
                    Img.HeightRequest = newHeight;

                    memoryStream.Position = 0;
                    var image = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                    Img.Source = image;
                    container.HeightRequest = newHeight;
                    OptionsImg.IsVisible = true;
                }
            }
        }
    }

    //private async void SaveImage_Clicked(object sender, EventArgs e)
    //{
    //    var imageStream = await container.CaptureAsync();

    //    if (imageStream != null)
    //    {
    //        using var memoryStream = new MemoryStream();
    //        await imageStream.CopyToAsync(memoryStream);

    //        await System.IO.File.WriteAllBytesAsync(@"C:\Users\brran\OneDrive\Escritorio\Test.png", memoryStream.ToArray());
    //    }
    //}

    private async void SaveImage_Clicked(object sender, EventArgs e)
    {
        var imageStream = await container.CaptureAsync();

        if (imageStream != null)
        {
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);

            var imageData = memoryStream.ToArray();
            var content = new ByteArrayContent(imageData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            using var client = new HttpClient();
            var response = await client.PostAsync("https://localhost:7121/WeatherForecast", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Imagen subida con éxito.");
            }
            else
            {
                Console.WriteLine("Error al subir la imagen.");
            }
        }
    }

    //private async void SaveImage_Clicked(object sender, EventArgs e)
    //{
    //    var imageStream = await container.CaptureAsync();

    //    if (imageStream != null)
    //    {
    //        using var memoryStream = new MemoryStream();
    //        await imageStream.CopyToAsync(memoryStream);

    //        // Directorio de imágenes
    //        var picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    //        // Si el directorio no existe, créalo
    //        if (!Directory.Exists(picturesDir))
    //        {
    //            Directory.CreateDirectory(picturesDir);
    //        }

    //        // Ruta del archivo
    //        string filePath = Path.Combine(picturesDir, "MyImage.png");

    //        // Guarda los bytes de la imagen como un archivo PNG
    //        await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

    //        // Muestra un mensaje de confirmación
    //        await DisplayAlert("Éxito", "Imagen guardada con éxito", "Aceptar");
    //    }
    //}


    private void OnButtonClicked(object sender, EventArgs e)
    {
        var ovalFrame = new Frame
        {
            WidthRequest = 100,
            HeightRequest = 50,
            BackgroundColor = Colors.Blue,
            CornerRadius = 25,
            Margin = new Thickness(0, 0, 20, 0),
            HasShadow = false,
            BorderColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        rotateButton = new Button
        {
            Text = "?",
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.Black,
            FontSize = 12,
            Padding = 0,
            WidthRequest = 20,
            HeightRequest = 20,
        };

        var canvas = new AbsoluteLayout()
        {
            WidthRequest = 115,
            Padding = new Thickness(0, 0, 0, 0),
            HeightRequest = 50,
            BackgroundColor = Colors.Transparent,
        };
        AbsoluteLayout.SetLayoutBounds(ovalFrame, new Rect(0.5, 0.5, 100, 50));
        AbsoluteLayout.SetLayoutFlags(ovalFrame, AbsoluteLayoutFlags.PositionProportional);
        AbsoluteLayout.SetLayoutBounds(rotateButton, new Rect(0.9, 0.1, 0, 0));
        AbsoluteLayout.SetLayoutFlags(rotateButton, AbsoluteLayoutFlags.PositionProportional);
        canvas.Children.Add(ovalFrame);
        canvas.Children.Add(rotateButton);

        container.Children.Add(canvas);

        var rotatePanGesture = new PanGestureRecognizer();
        rotatePanGesture.PanUpdated += (s, e) => OnRotateButtonPanUpdated(s, e, canvas);
        rotateButton.GestureRecognizers.Add(rotatePanGesture);

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += (s, e) => OnPanUpdated(s, e, canvas);
        ovalFrame.GestureRecognizers.Add(panGesture);

        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += (s, e) => OnPinchUpdated(s, e, canvas);
        ovalFrame.GestureRecognizers.Add(pinchGesture);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e, AbsoluteLayout canvas)
    {
        if (sender is Frame oval)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    offsetX = canvas.TranslationX;
                    offsetY = canvas.TranslationY;
                    break;
                case GestureStatus.Running:
                    double newX = offsetX + e.TotalX;
                    double newY = offsetY + e.TotalY;
                    newX = Math.Max(Math.Min(container.Width - canvas.Width, newX), Math.Min(container.Width - canvas.Width, newX));
                    newY = Math.Max(0, Math.Min(container.Height - canvas.Height, newY));

                    canvas.TranslationX = newX;
                    canvas.TranslationY = newY;
                    break;
            }
        }
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e, AbsoluteLayout canvas)
    {
        if (sender is Frame oval)
        {
            double newWidth = oval.Width * e.Scale;
            double newHeight = oval.Height * e.Scale;

            if (newWidth <= 500 && newHeight <= 500)
            {
                oval.WidthRequest = newWidth;
                oval.HeightRequest = newHeight;
                canvas.WidthRequest = newWidth + 15;
                canvas.HeightRequest = newHeight;

                double cornerRadius = Math.Min(newWidth, newHeight) / 2;
                oval.CornerRadius = (float)cornerRadius;
            }
        }
    }

    private void OnRotateButtonPanUpdated(object sender, PanUpdatedEventArgs e, AbsoluteLayout canvas)
    {
        if (e.StatusType == GestureStatus.Running)
        {
            rotationAngle += e.TotalX / 7;
            canvas.Rotation = rotationAngle;
        }
    }

    private async void CreateText_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Ingrese texto", "Ingrese el texto que desea mostrar:");
        if (!string.IsNullOrEmpty(result))
        {
            CreateTextLabel(result);
        }
    }

    private void CreateTextLabel(string text)
    {
        var label = new Label
        {
            Text = text,
            BackgroundColor = Colors.White,
            Opacity = 0.4,
            TextColor = Colors.Black,
            FontSize = 16,
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };

        container.Children.Add(label);

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdatedForText;
        label.GestureRecognizers.Add(panGesture);

        var pinchGesture = new PinchGestureRecognizer();
        pinchGesture.PinchUpdated += OnPinchUpdatedForText;
        label.GestureRecognizers.Add(pinchGesture);
    }

    private void OnPanUpdatedForText(object sender, PanUpdatedEventArgs e)
    {
        if (sender is Label label)
        {
            if (e.StatusType == GestureStatus.Started)
            {
                offsetX = label.TranslationX;
                offsetY = label.TranslationY;
            }
            else if (e.StatusType == GestureStatus.Running)
            {
                double newX = offsetX + e.TotalX;
                double newY = offsetY + e.TotalY;
                newX = Math.Max(Math.Min(container.Width - label.Width, newX), Math.Min(container.Width - label.Width, newX));
                newY = Math.Max(0, Math.Min(container.Height - label.Height, newY));

                label.TranslationX = newX;
                label.TranslationY = newY;
            }
        }
    }

    private void OnPinchUpdatedForText(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (sender is Label label)
        {
            double newSize = label.FontSize * e.Scale;
            label.FontSize = newSize;
        }
    }
}