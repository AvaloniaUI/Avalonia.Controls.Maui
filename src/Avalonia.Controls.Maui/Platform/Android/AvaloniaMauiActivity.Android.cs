using System;
using System.Runtime.Versioning;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Avalonia.Android;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platforms.Android
{
    /// <summary>
    /// An activity that serves as the main entry point for the Android platform when hosting Avalonia content within a .NET MAUI application. This class inherits from MauiAppCompatActivity and implements the IActivityResultHandler interface to handle activity results and permission requests, allowing developers to create cross-platform applications that utilize Avalonia's UI capabilities while leveraging the native performance and features of Android devices.
    /// </summary>
    public class AvaloniaMauiActivity : MauiAppCompatActivity, IActivityResultHandler
    {
        public Action<int, Result, Intent?>? ActivityResult { get; set; }
        public Action<int, string[], Permission[]>? RequestPermissionsResult { get; set; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            ActivityResult?.Invoke(requestCode, resultCode, data);
        }

        /// <summary>
        /// Handles the result of a permission request. This method is called when the user responds to a permission request dialog, and it is responsible for invoking the RequestPermissionsResult action with the appropriate parameters, allowing the application to react to the user's decision regarding the requested permissions.
        /// </summary>
        /// <param name="requestCode">The request code passed in the permission request.</param>
        /// <param name="permissions">The requested permissions.</param>
        /// <param name="grantResults">The grant results for the corresponding permissions.</param>
        [SupportedOSPlatform("android23.0")]
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            RequestPermissionsResult?.Invoke(requestCode, permissions, grantResults);
        }
    }
}