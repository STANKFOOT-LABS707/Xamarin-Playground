﻿using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFMediaPluginDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        
        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            var result = await TakePhoto(this);

            if (result != null)
                viewPhotoImage.Source = result;
        }

        private async void PickPhotoButton_Clicked(object sender, EventArgs e)
        {
            var result = await SelectPhoto(this);

            if (result != null)
                viewPhotoImage.Source = result;
        }

        public async Task<ImageSource> TakePhoto(ContentPage pageContext)
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await pageContext.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return null;
            }

            await RequestCameraAndGalleryPermissions();

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "TestPhotoFolder",
                SaveToAlbum = true,
                CompressionQuality = 75,
                CustomPhotoSize = 50,
                PhotoSize = PhotoSize.Medium,
                MaxWidthHeight = 1000,
            });

            if (file == null)
                return null;

            var imageSource = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            return imageSource;
        }

        public async Task<ImageSource> SelectPhoto(ContentPage pageContext)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await pageContext.DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return null;
            }
            
            await RequestCameraAndGalleryPermissions();

            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
            });

            if (file == null)
                return null;

            var imageSource = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            return imageSource;
        }

        private async Task<bool> RequestCameraAndGalleryPermissions() 
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            var mediaLibraryStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.MediaLibrary);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted || mediaLibraryStatus != PermissionStatus.Granted)
            {
                var permissionRequestResult = await CrossPermissions.Current.RequestPermissionsAsync(
                    new Permission[] { Permission.Camera, Permission.Storage, Permission.MediaLibrary });

                var cameraResult = permissionRequestResult[Plugin.Permissions.Abstractions.Permission.Camera];
                var storageResult = permissionRequestResult[Plugin.Permissions.Abstractions.Permission.Storage];
                var mediaLibraryResults = permissionRequestResult[Plugin.Permissions.Abstractions.Permission.MediaLibrary];
            }

            return true;
        }

        private async Task<bool> RequestPermissions(List<Permission> permissionList)
        {
            //List<Permission> permissionList = new List<Permission> { Permission.Camera, Permission.Storage, Permission.MediaLibrary };

            List<PermissionStatus> permissionStatuses = new List<PermissionStatus>();
            foreach (var permission in permissionList)
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
                permissionStatuses.Add(status);
            }

            var requiresRequesst = permissionStatuses.Any(x => x != PermissionStatus.Granted);

            if (requiresRequesst)
            {
                var permissionRequestResult = await CrossPermissions.Current.RequestPermissionsAsync(permissionList.ToArray());
            }

            return true;
        }
    }
}