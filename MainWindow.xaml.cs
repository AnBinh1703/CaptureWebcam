using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Win32;
using Paket;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
namespace CaptureWebcam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            this.Close();
        }

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap currentFrame;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame; // Make sure this line is present
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("No camera devides found!!!");
            }
        }
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Dispose the previous frame to avoid memory leaks
            if (currentFrame != null)
            {
                currentFrame.Dispose();
            }

            // Assign the new frame to the currentFrame variable
            currentFrame = (Bitmap)eventArgs.Frame.Clone();

            // Update the webcam control with the new frame
            webcam.Dispatcher.Invoke(() =>
            {
                webcam.Source = ToBitmapImage(eventArgs.Frame);
            });
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void Button_Capture(object sender, RoutedEventArgs e)
        {
            if (currentFrame != null)
            {
                string folderPath = tb_FolderPath.Text;

                // Check if the folder path is valid
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                    string filePath = Path.Combine(folderPath, fileName);

                    try
                    {
                        // Save the current frame as an image file
                        currentFrame.Save(filePath, ImageFormat.Jpeg);

                        // Display a success message
                        MessageBox.Show("Snapshot captured and saved successfully!");
                        // Refresh the list view with the updated file list
                        List<FileData> fileList = ScanFolder(folderPath);
                        FileListView.ItemsSource = null; // Clear the ItemsSource
                        FileListView.ItemsSource = fileList; // Assign the new ItemsSource
                    }
                    catch (Exception ex)
                    {
                        // Display an error message if saving fails
                        MessageBox.Show("Error capturing snapshot: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a valid folder before capturing a snapshot.");
                }
            }
            else
            {
                MessageBox.Show("No video frame available for capture.");
            }
        }
        private void Button_Browser(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            WinForms.DialogResult result = dialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                tb_FolderPath.Text = folderPath;

                List<FileData> fileList = ScanFolder(folderPath);
                FileListView.ItemsSource = null; // Clear the ItemsSource
                FileListView.ItemsSource = fileList; // Assign the new ItemsSource
            }
        }
        private List<FileData> ScanFolder(string folderPath)
        {
            List<FileData> fileList = new List<FileData>();

            string[] files = Directory.GetFiles(folderPath);

            foreach (string filePath in files)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                // Exclude directories from the list
                if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    BitmapImage image = LoadImage(filePath);

                    fileList.Add(new FileData
                    {
                        Name = fileInfo.Name,
                        Image = image
                    });
                }
            }
            // Sort the fileList by the newest creation date
            fileList.Sort((a, b) => DateTime.Compare(
                File.GetCreationTime(Path.Combine(folderPath, b.Name)),
                File.GetCreationTime(Path.Combine(folderPath, a.Name))));
            return fileList;
        }
        private BitmapImage LoadImage(string filePath)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(filePath);
            image.EndInit();
            return image;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
            }
        }
        public class FileData
        {
            public BitmapImage Image { get; set; }
            public string Name { get; set; }
            public string ImagePath { get; set; }
            public void LoadImage()
    {
        if (!string.IsNullOrEmpty(ImagePath))
        {
            Image = new BitmapImage(new Uri(ImagePath));
        }
    }
        }


        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            /*if (FileListView.SelectedItem is FileData selectedFile)
            {
                ImageWindow imageWindow = new ImageWindow(selectedFile.ImagePath);
                imageWindow.Show();
                // Perform any necessary actions after the ImageWindow is closed
            }
        }*/
            // Get the original source of the event
            DependencyObject originalSource = (DependencyObject)e.OriginalSource;

            // Traverse up the visual tree to find the parent ListViewItem
            ListViewItem selectedItem = FindParent<ListViewItem>(originalSource);

            if (selectedItem != null)
            {
                // Get the file data from the selected item's DataContext
                FileData fileData = (FileData)selectedItem.DataContext;

                // Get the file path from the file data
                string imagePath = fileData.ImagePath;

                // Open the ImageWindow and pass the imagePath
                ImageWindow imageWindow = new ImageWindow(imagePath, fileData);
                imageWindow.ImageDeleted += ImageWindow_ImageDeleted; ;
                imageWindow.Show();
            }
        }
        private void ImageWindow_ImageDeleted(object sender, string imagePath)
        {
            // Update the FileListView when an image is deleted
            if (FileListView.ItemsSource is List<FileData> fileList)
            {
                FileData deletedFile = fileList.FirstOrDefault(file => file.ImagePath == imagePath);
                if (deletedFile != null)
                {
                    fileList.Remove(deletedFile);
                    FileListView.Items.Refresh();
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Traverse up the visual tree to find the parent of the specified type
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }

            return child as T;
        }



    }
}


/*private void Button_Capture(object sender, RoutedEventArgs e)
{
    if (videoSource != null && videoSource.IsRunning)
    {
        // Dừng việc chụp hình tạm thời

        // Kiểm tra xem folder đã được chọn chưa
        if (!string.IsNullOrWhiteSpace(tb_FolderPath.Text) && currentFrame != null)
        {
            string fileName = $"snapshot_{DateTime.Now:yyyyMMddHHmmss}.png";
            string filePath = System.IO.Path.Combine(tb_FolderPath.Text, fileName);

            // Add a null check for the currentFrame object
            if (currentFrame != null)
            {
                try
                {
                    // Lưu ảnh vào thư mục đã chọn
                    currentFrame.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    System.Windows.MessageBox.Show($"Snapshot saved to {filePath}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error saving snapshot: {ex.Message}");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Current frame is null. Make sure the VideoSource_NewFrame event is triggered.");
            }
        }
        else
        {
            System.Windows.MessageBox.Show("Please select a folder before capturing a snapshot.");
        }

        // Bắt đầu lại việc chụp hình từ webcam
    }
    else
    {
        System.Windows.MessageBox.Show("Webcam is not running.");
    }
}*/
