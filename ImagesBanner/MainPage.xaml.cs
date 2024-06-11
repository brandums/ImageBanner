using Newtonsoft.Json;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ImagesBanner
{
    public partial class MainPage : ContentPage
    {
        private const string HorizontalImageListFile = "horizontal_imagelist.json";
        private const string VerticalImageListFile = "vertical_imagelist.json";
        private const int MaxWidth = 8000;
        private const int MaxHeight = 8000;
        private const int ImageHeight = 250;
        private const int ImageWidth = 250;

        public ObservableCollection<string> HorizontalImages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> VerticalImages { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();
            LoadImageList(HorizontalImageListFile, HorizontalImages, HorizontalContainer, true);
            LoadImageList(VerticalImageListFile, VerticalImages, VerticalContainer, false);

            BindingContext = this;
        }

        private async void OnLoadImageHorizontalClicked(object sender, EventArgs e)
        {
            await LoadImage(HorizontalImages, HorizontalContainer, HorizontalImageListFile, WarningLabelHorizontal, true);
        }

        private async void OnLoadImageVerticalClicked(object sender, EventArgs e)
        {
            await LoadImage(VerticalImages, VerticalContainer, VerticalImageListFile, WarningLabelVertical, false);
        }

        private async Task LoadImage(ObservableCollection<string> imageList, StackLayout container, string imageListFile, Label warningLabel, bool isHorizontal)
        {
            var file = await FilePicker.PickAsync();

            if (file != null)
            {
                var imageUrl = await SaveImage(file);
                var imageSize = await GetImageSize(imageUrl);

                if (IsContainerFull(container, isHorizontal ? imageSize.Width : imageSize.Height, isHorizontal))
                {
                    warningLabel.IsVisible = true;
                    return;
                }
                warningLabel.IsVisible = false;

                imageList.Add(imageUrl);
                SaveImageList(imageList, imageListFile);
                AddImageToContainer(container, imageUrl, isHorizontal ? CalculateImageWidth(imageSize) : CalculateImageHeight(imageSize), isHorizontal);
            }
        }

        private double CalculateImageWidth(Size imageSize)
        {
            return (ImageHeight * imageSize.Width) / imageSize.Height;
        }

        private double CalculateImageHeight(Size imageSize)
        {
            return (ImageWidth * imageSize.Height) / imageSize.Width;
        }

#pragma warning disable CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        private async Task<Size> GetImageSize(string imageUrl)
#pragma warning restore CS1998 // El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica
        {
            var filePath = Path.Combine("C:\\Users\\brran\\OneDrive\\Escritorio\\ImagesBanner\\ImagesBanner\\Resources\\Images", imageUrl);
            using (var inputStream = File.OpenRead(filePath))
            {
                using (var skBitmap = SKBitmap.Decode(inputStream))
                {
                    return new Size(skBitmap.Width, skBitmap.Height);
                }
            }
        }

        private bool IsContainerFull(StackLayout container, double newImageSize, bool isHorizontal)
        {
            double totalSize = container.Children.Sum(child => isHorizontal ? child.Width : child.Height);
            return (totalSize + newImageSize) > (isHorizontal ? MaxWidth : MaxHeight);
        }

        private async Task<string> SaveImage(FileResult file)
        {
            var filePath = Path.Combine("C:\\Users\\brran\\OneDrive\\Escritorio\\ImagesBanner\\ImagesBanner\\Resources\\Images", file.FileName);

            using (var stream = await file.OpenReadAsync())
            using (var newStream = File.OpenWrite(filePath))
            {
                await stream.CopyToAsync(newStream);
            }

            return file.FileName;
        }

        private void SaveImageList(ObservableCollection<string> imageList, string imageListFile)
        {
            var json = JsonConvert.SerializeObject(imageList);
            var filePath = Path.Combine("C:\\Users\\brran\\OneDrive\\Escritorio\\ImagesBanner\\ImagesBanner", imageListFile);
            File.WriteAllText(filePath, json);
        }

        private void LoadImageList(string imageListFile, ObservableCollection<string> imageList, StackLayout container, bool isHorizontal)
        {
            var filePath = Path.Combine("C:\\Users\\brran\\OneDrive\\Escritorio\\ImagesBanner\\ImagesBanner", imageListFile);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var images = JsonConvert.DeserializeObject<ObservableCollection<string>>(json);
                imageList.Clear();
                container.Children.Clear();
                foreach (var image in images)
                {
                    imageList.Add(image);
                    var imageSize = GetImageSize(image).Result;
                    AddImageToContainer(container, image, isHorizontal ? CalculateImageWidth(imageSize) : CalculateImageHeight(imageSize), isHorizontal);
                }
            }
        }

        private void AddImageToContainer(StackLayout container, string imageUrl, double imageSize, bool isHorizontal)
        {
            var image = new Image
            {
                Source = ImageSource.FromFile(imageUrl),
                HeightRequest = isHorizontal ? 250 : imageSize,
                WidthRequest = isHorizontal ? imageSize : 250,
                Aspect = Aspect.AspectFit
            };

            var deleteButton = new Button { Text = "X" };
            int imageIndex = container.Children.Count;

            deleteButton.Clicked += (sender, args) => DeleteImage(imageIndex, container, isHorizontal);

            var grid = new Grid();
            grid.HeightRequest = isHorizontal ? 250 : imageSize;
            grid.WidthRequest = isHorizontal ? imageSize : 250;

            grid.Children.Add(image);
            grid.Children.Add(deleteButton);

            deleteButton.VerticalOptions = LayoutOptions.Start;
            deleteButton.HorizontalOptions = LayoutOptions.End;

            container.Children.Add(grid);
        }

        private void DeleteImage(int imageIndex, StackLayout container, bool isHorizontal)
        {
            var imageList = isHorizontal ? HorizontalImages : VerticalImages;
            var imageListFile = isHorizontal ? HorizontalImageListFile : VerticalImageListFile;

            if (imageIndex < 0 || imageIndex >= imageList.Count)
            {
                return;
            }

            imageList.RemoveAt(imageIndex);
            SaveImageList(imageList, imageListFile);

            LoadImageList(imageListFile, imageList, container, isHorizontal);
        }
    }
}
