using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using RedBit.Mobile.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RedBit.XamServerless
{
    public class MainPageViewModel : ViewModel
    {
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

        private bool _PictureButtonEnabled = true;

        /// <summary>
        /// Sets and gets the PictureButtonEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool PictureButtonEnabled
        {
            get => _PictureButtonEnabled;
            set => SetProperty(ref _PictureButtonEnabled, value);
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
            PictureButtonEnabled = false;
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
                        Status = "Composing Picture ...";
                        var t = file.GetStream();
                        byte[] buffer = new byte[t.Length];
                        t.Read(buffer, 0, buffer.Length);
                        var b64 = Convert.ToBase64String(buffer);

                        this.PhotoPath = file.Path;
                        file.Dispose();
                        Status = "See your pic!";
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
                PictureButtonEnabled = true;
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
