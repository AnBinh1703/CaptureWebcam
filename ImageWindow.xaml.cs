using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Firebase.Storage;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore;
using static CaptureWebcam.MainWindow;

namespace CaptureWebcam
{
    public partial class ImageWindow : Window
    {
        private FirebaseStorage storage;
        private FileData FileData { get; set; }

        public ImageWindow(string imagePath, FileData fileData)
        {
            InitializeComponent();

            FileData = fileData;
            FileData.ImagePath = imagePath;
            FileData.LoadImage();

            DataContext = FileData;

            // Load the service account key JSON file
            InitializeFirebaseAsync(imagePath);
        }

        private async void InitializeFirebaseAsync(string imagePath)
        {
            try
            {
                // Load the service account key JSON file
                var credential = GoogleCredential
                    .FromFile("D:\\UNIVERSITY\\SE_7_SU_2023\\PRN221\\CaptureWebcam\\capturewebcam-b8665-firebase-adminsdk-l8a6f-5a592ece75.json");

                // Initialize Firebase Admin SDK
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                });

                // Initialize Firebase Storage
                storage = new FirebaseStorage(
                    "capturewebcam-b8665",
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => GetAccessTokenAsync(credential),
                    });
            }
            catch (Exception ex)
            {
                // Handle initialization error
                Console.WriteLine($"Error initializing Firebase: {ex.Message}");
            }
        }

        private async Task<string> GetAccessTokenAsync(GoogleCredential credential)
        {
            return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }
        private async void Button_UploadToFirebase(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Upload button clicked.");

            try
            {
                Console.WriteLine($"ImagePath: {FileData.ImagePath}");

                if (FileData != null && !string.IsNullOrEmpty(FileData.ImagePath) && File.Exists(FileData.ImagePath))
                {
                    Console.WriteLine($"File exists: {File.Exists(FileData.ImagePath)}");

                    string fileName = Path.GetFileName(FileData.ImagePath);
                    Console.WriteLine($"Uploading file: {fileName}");

                    var stream = File.OpenRead(FileData.ImagePath);

                    // Upload the image to Firebase Storage
                    var imageUrl = await storage
                        .Child("images")
                        .Child(fileName)
                        .PutAsync(stream);

                    // Display a success message
                    Console.WriteLine($"Image uploaded to Firebase Storage!\nImage URL: {imageUrl}");
                    MessageBox.Show($"Image uploaded to Firebase Storage!\nImage URL: {imageUrl}");
                }
                else
                {
                    MessageBox.Show("No image data available for upload.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during image upload: {ex}");
                MessageBox.Show($"Error uploading image to Firebase: {ex.Message}");
            }
        }


        /* private async void Button_UploadToFirebase(object sender, RoutedEventArgs e)
         {
             Console.WriteLine($"ImagePath: {FileData.ImagePath}");
             try
             {
                 if (FileData != null && !string.IsNullOrEmpty(FileData.ImagePath) && File.Exists(FileData.ImagePath))
                 {
                     string fileName = Path.GetFileName(FileData.ImagePath);
                     var stream = File.OpenRead(FileData.ImagePath);

                     // Upload the image to Firebase Storage
                     var imageUrl = await storage
                         .Child("images")
                         .Child(fileName)
                         .PutAsync(stream);



                     // Display a success message
                     Console.WriteLine($"File exists: {File.Exists(FileData.ImagePath)}");

                     MessageBox.Show($"Image uploaded to Firebase Storage!\nImage URL: {imageUrl}");
                 }
                 else
                 {
                     MessageBox.Show("No image data available for upload.");
                 }
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Exception during image upload: {ex}");
                 MessageBox.Show($"Error uploading image to Firebase: {ex.Message}");
             }
         }
 */
        public class ImageData
        {
            public BitmapImage Image { get; set; }
            public string ImagePath { get; set; }

            public void LoadImage()
            {
                if (!string.IsNullOrEmpty(ImagePath))
                {
                    Image = new BitmapImage(new Uri(ImagePath));
                }
            }
        }

        public event EventHandler<string> ImageDeleted;

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is FileData fileData)
                {
                    // Delete the image file
                    if (File.Exists(fileData.ImagePath))
                    {
                        File.Delete(fileData.ImagePath);
                    }

                    // Raise the ImageDeleted event and pass the image path
                    ImageDeleted?.Invoke(this, fileData.ImagePath);
                }

                Close();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                Console.WriteLine($"Failed to delete the image: {ex.Message}");
                MessageBox.Show($"Failed to delete the image: {ex.Message}");
            }
        }
    }
}
/*using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Firebase.Storage;
using System.IO;
using System.Windows.Forms;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore;




using static CaptureWebcam.MainWindow;

namespace CaptureWebcam
{
    /// <summary>
    /// Interaction logic for ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        private FirebaseStorage storage;
        private FileData FileData { get; set; }
        public ImageWindow(string imagePath, FileData fileData)
        {
            InitializeComponent();

            FileData = fileData;
            FileData.ImagePath = imagePath;
            FileData.LoadImage();

            DataContext = FileData;
            // Load the service account key JSON file
            InitializeFirebaseAsync(imagePath);
        }
        private async void InitializeFirebaseAsync(string imagePath)
        {

            try
            {
                // Load the service account key JSON file
                var credential = GoogleCredential
                    .FromFile("D:\\UNIVERSITY\\SE_7_SU_2023\\PRN221\\CaptureWebcam\\capturewebcam-b8665-firebase-adminsdk-l8a6f-5a592ece75.json");

                // Initialize Firebase Admin SDK
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                });

                // Initialize Firebase Storage
                storage = new FirebaseStorage(
                    "capturewebcam-b8665",
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => GetAccessTokenAsync(credential),
                    });
            }
            catch (Exception ex)
            {
                // Handle initialization error
                System.Windows.MessageBox.Show($"Error initializing Firebase: {ex.Message}");
            }
        }
        private async Task<string> GetAccessTokenAsync(GoogleCredential credential)
        {
            
            return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }
        private async void Button_UploadToFirebase(object sender, RoutedEventArgs e)
        {

            try
            {
                if (FileData != null && !string.IsNullOrEmpty(FileData.ImagePath) && File.Exists(FileData.ImagePath))
                {
                    string fileName = Path.GetFileName(FileData.ImagePath);
                    var stream = File.OpenRead(FileData.ImagePath);

                    // Upload the image to Firebase Storage
                    var imageUrl = await storage
                        .Child("images")
                        .Child(fileName)
                        .PutAsync(stream);
                    Console.WriteLine($"ImagePath: {FileData.ImagePath}");

                    // Display a success message
                    System.Windows.MessageBox.Show($"Image uploaded to Firebase Storage!\nImage URL: {imageUrl}");
                }
                else
                {
                    System.Windows.MessageBox.Show("No image data available for upload.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during image upload: {ex}");

               
                System.Windows.MessageBox.Show($"Error uploading image to Firebase: {ex.Message}");
            }

        }

        public class ImageData
        {
            public BitmapImage Image { get; set; }
            public string ImagePath { get; set; }

            public void LoadImage()
            {
                if (!string.IsNullOrEmpty(ImagePath))
                {
                    Image = new BitmapImage(new Uri(ImagePath));
                }
            }
        }






        public event EventHandler<string> ImageDeleted;
        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is FileData fileData)
                {
                    // Delete the image file
                    if (File.Exists(fileData.ImagePath))
                    {
                        File.Delete(fileData.ImagePath);
                    }

                    // Raise the ImageDeleted event and pass the image path
                    ImageDeleted?.Invoke(this, fileData.ImagePath);
                }

                Close();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                System.Windows.MessageBox.Show($"Failed to delete the image: {ex.Message}");
            }
        }


    }
}

*/