using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using RedBit.Ai.Models;
using RedBit.Mobile.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RedBit.XamServerless
{
    public class MainPageViewModel : ViewModel
    {
        private ImageUploadResult _result;
        private const string BASE_URL = "https://cba2fa38.ngrok.io/api";

        public MainPageViewModel()
        {
            this.Title = "Xamarin AI";
        }

        private string _Status = string.Empty;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Status
        {
            get => _Status;
            set => SetProperty(ref _Status, value);
        }

        private string _PhotoPath = string.Empty;

        /// <summary>
        /// Sets and gets the PhotoPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PhotoPath
        {
            get => _PhotoPath;
            set => SetProperty(ref _PhotoPath, value);
        }

        private Command _SnapPictureCommand;
        /// <summary>
        /// Gets the SnapPicture.
        /// </summary>
        public Command SnapPictureCommand
        {
            get
            {
                return _SnapPictureCommand
                    ?? (_SnapPictureCommand = new Command(TakePicture));
            }
        }

        private async void TakePicture()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                await CrossMedia.Current.Initialize();

                // attempt to take picture
                if (await InitializeCameraPermissions() && await InitializeStoragePermissions())
                {
                    // check if camera is available
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await DisplayAlert("No Camera", ":( No camera available.");
                        return;
                    }

                    // take a photo
                    var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "Sample",
                        Name = "test.jpg",
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Large,
                        CompressionQuality = 75,
                        AllowCropping = true
                    });

                    if (file != null)
                    {
                        using (file)
                        {
                            // convert image to base 64
                            Status = "Composing Picture ...";
                            
                            // save the path for the photo to show in the UI
                            PhotoPath = file.Path;

                            // upload the image
                            Status = "Uploading Image!";
                            await UploadImage(file.GetStream());
                            Status = "Image Uploaded!";
                        }
                    }
                    else
                    {
                        await DisplayAlert("Information", "No picture available");
                        Status = "No picture available";
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UploadImage(Stream file)
        {
            // create the base64 stream
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, buffer.Length);
            var b64 = Convert.ToBase64String(buffer);

            // upload using HttpClient
            using (HttpClient client = new HttpClient())
            {
                // get the URL
                var url = $"{BASE_URL}/UploadImage";

                // create the object to upload
                var imageObject = new ImageUpload { Imageb64 = b64 };

                // create the request
                using (var msg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    msg.Headers.Add("Accept", "application/json");

                    // set the body for the POST
                    msg.Content = new StringContent(JsonConvert.SerializeObject(imageObject), Encoding.UTF8, "application/json");
                    msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // send the response
                    using (var response = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // parse out the response
                        _result = JsonConvert.DeserializeObject<ImageUploadResult>(responseContent);
                    }
                }
            }
        }

        internal async Task<bool> InitializePermission(Permission permission)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("Requesting permission");
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(permission);
                    Console.WriteLine($"{results[permission].ToString()}");
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(permission))
                        status = results[permission];

                    if (status != PermissionStatus.Granted)
                    {
                        // status is not granted
                        await DisplayAlert("No Permissions", "Permission not granted");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            // we are good to go
            return true;
        }

        protected Task<bool> InitializeStoragePermissions() => InitializePermission(Permission.Storage);

        protected Task<bool> InitializeCameraPermissions() => InitializePermission(Permission.Camera);

    }
}
