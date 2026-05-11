# Avalonia.Controls.Maui.Essentials - Implementation Tracking

The purpose of this document is to track the implementation status of **MAUI Essentials** for the Avalonia .NET MAUI backend. This library provides cross-platform APIs for accessing native device features.

---

## Accelerometer

Monitor the device accelerometer sensor which reports the acceleration of the device along three axes.

### Interface: IAccelerometer

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

---

## AppActions

Create and respond to app shortcuts from the home screen.

### Interface: IAppActions

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| GetAsync() | Method | ⏳ TODO |
| SetAsync(IEnumerable<AppAction>) | Method | ⏳ TODO |
| OnAppAction | Event | ⏳ TODO |

---

## AppInfo

Get information about the application at runtime.

### Interface: IAppInfo

| Member | Type | Status |
|--------|------|--------|
| PackageName | Property | ⏳ TODO |
| Name | Property | ⏳ TODO |
| VersionString | Property | ⏳ TODO |
| Version | Property | ⏳ TODO |
| BuildString | Property | ⏳ TODO |
| RequestedTheme | Property | ⏳ TODO |
| PackagingModel | Property | ⏳ TODO |
| RequestedLayoutDirection | Property | ⏳ TODO |
| ShowSettingsUI() | Method | ⏳ TODO |

---

## Barometer

Monitor the device barometer sensor which measures pressure.

### Interface: IBarometer

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

---

## Battery

Access device battery information and monitor changes.

### Interface: IBattery

| Member | Type | Status |
|--------|------|--------|
| ChargeLevel | Property | ⏳ TODO |
| State | Property | ⏳ TODO |
| PowerSource | Property | ⏳ TODO |
| EnergySaverStatus | Property | ⏳ TODO |
| BatteryInfoChanged | Event | ⏳ TODO |
| EnergySaverStatusChanged | Event | ⏳ TODO |

---

## Browser

Open a web browser to a specific URL.

### Interface: IBrowser

| Member | Type | Status |
|--------|------|--------|
| OpenAsync(Uri, BrowserLaunchOptions) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| BrowserLaunchMode | ⏳ TODO |
| BrowserLaunchOptions | ⏳ TODO |
| BrowserTitleMode | ⏳ TODO |

---

## Clipboard

Copy and paste text to and from the system clipboard.

### Interface: IClipboard

| Member | Type | Status |
|--------|------|--------|
| HasText | Property | ⏳ TODO |
| SetTextAsync(string) | Method | ⏳ TODO |
| GetTextAsync() | Method | ⏳ TODO |
| ClipboardContentChanged | Event | ⏳ TODO |

---

## Compass

Monitor the device compass sensor which reports the device's heading relative to magnetic north.

### Interface: ICompass

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Start(SensorSpeed, bool) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

---

## Connectivity

Monitor network connectivity state and type.

### Interface: IConnectivity

| Member | Type | Status |
|--------|------|--------|
| NetworkAccess | Property | ⏳ TODO |
| ConnectionProfiles | Property | ⏳ TODO |
| ConnectivityChanged | Event | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| NetworkAccess | ⏳ TODO |
| ConnectionProfile | ⏳ TODO |

---

## Contacts

Read contacts from the device.

### Interface: IContacts

| Member | Type | Status |
|--------|------|--------|
| PickContactAsync() | Method | ⏳ TODO |
| GetAllAsync(CancellationToken) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| Contact | ⏳ TODO |
| ContactPhone | ⏳ TODO |
| ContactEmail | ⏳ TODO |

---

## DeviceDisplay

Get the device's screen metrics and request to keep the screen awake.

### Interface: IDeviceDisplay

| Member | Type | Status |
|--------|------|--------|
| MainDisplayInfo | Property | ⏳ TODO |
| KeepScreenOn | Property | ⏳ TODO |
| MainDisplayInfoChanged | Event | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| DisplayInfo | ⏳ TODO |
| DisplayOrientation | ⏳ TODO |
| DisplayRotation | ⏳ TODO |

---

## DeviceInfo

Get information about the device.

### Interface: IDeviceInfo

| Member | Type | Status |
|--------|------|--------|
| Model | Property | ⏳ TODO |
| Manufacturer | Property | ⏳ TODO |
| Name | Property | ⏳ TODO |
| VersionString | Property | ⏳ TODO |
| Version | Property | ⏳ TODO |
| Platform | Property | ⏳ TODO |
| Idiom | Property | ⏳ TODO |
| DeviceType | Property | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| DevicePlatform | ⏳ TODO |
| DeviceIdiom | ⏳ TODO |
| DeviceType | ⏳ TODO |

---

## Email

Compose and send an email.

### Interface: IEmail

| Member | Type | Status |
|--------|------|--------|
| IsComposeSupported | Property | ⏳ TODO |
| ComposeAsync(EmailMessage) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| EmailMessage | ⏳ TODO |
| EmailBodyFormat | ⏳ TODO |
| EmailAttachment | ⏳ TODO |

---

## FilePicker

Pick files from the device.

### Interface: IFilePicker

| Member | Type | Status |
|--------|------|--------|
| PickAsync(PickOptions) | Method | ⏳ TODO |
| PickMultipleAsync(PickOptions) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| PickOptions | ⏳ TODO |
| FilePickerFileType | ⏳ TODO |
| FileResult | ⏳ TODO |

---

## FileSystem

Access the app's bundled files and cache/data directories.

### Interface: IFileSystem

| Member | Type | Status |
|--------|------|--------|
| CacheDirectory | Property | ⏳ TODO |
| AppDataDirectory | Property | ⏳ TODO |
| OpenAppPackageFileAsync(string) | Method | ⏳ TODO |
| AppPackageFileExistsAsync(string) | Method | ⏳ TODO |

---

## Flashlight

Turn on/off the device's flashlight.

### Interface: IFlashlight

| Member | Type | Status |
|--------|------|--------|
| IsSupportedAsync() | Method | ⏳ TODO |
| TurnOnAsync() | Method | ⏳ TODO |
| TurnOffAsync() | Method | ⏳ TODO |

---

## Geocoding

Convert addresses to coordinates and vice versa.

### Interface: IGeocoding

| Member | Type | Status |
|--------|------|--------|
| GetLocationsAsync(string) | Method | ⏳ TODO |
| GetPlacemarksAsync(double, double) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| Location | ⏳ TODO |
| Placemark | ⏳ TODO |

---

## Geolocation

Get the device's current GPS location.

### Interface: IGeolocation

| Member | Type | Status |
|--------|------|--------|
| IsListeningForeground | Property | ⏳ TODO |
| GetLastKnownLocationAsync() | Method | ⏳ TODO |
| GetLocationAsync(GeolocationRequest, CancellationToken) | Method | ⏳ TODO |
| StartListeningForegroundAsync(GeolocationListeningRequest) | Method | ⏳ TODO |
| StopListeningForeground() | Method | ⏳ TODO |
| LocationChanged | Event | ⏳ TODO |
| ListeningFailed | Event | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| Location | ⏳ TODO |
| GeolocationRequest | ⏳ TODO |
| GeolocationListeningRequest | ⏳ TODO |
| GeolocationAccuracy | ⏳ TODO |
| GeolocationError | ⏳ TODO |

---

## Gyroscope

Monitor the device gyroscope sensor which measures rotation around the device's three primary axes.

### Interface: IGyroscope

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

---

## HapticFeedback

Trigger haptic feedback on the device.

### Interface: IHapticFeedback

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| Perform(HapticFeedbackType) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| HapticFeedbackType | ⏳ TODO |

---

## Launcher

Open another application by URI.

### Interface: ILauncher

| Member | Type | Status |
|--------|------|--------|
| CanOpenAsync(Uri) | Method | ⏳ TODO |
| OpenAsync(Uri) | Method | ⏳ TODO |
| OpenAsync(OpenFileRequest) | Method | ⏳ TODO |
| TryOpenAsync(Uri) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| OpenFileRequest | ⏳ TODO |

---

## Magnetometer

Monitor the device magnetometer sensor which detects the device's orientation relative to Earth's magnetic field.

### Interface: IMagnetometer

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

---

## MainThread

Run code on the application's main UI thread.

### Static Class: MainThread

| Member | Type | Status |
|--------|------|--------|
| IsMainThread | Property | ⏳ TODO |
| BeginInvokeOnMainThread(Action) | Method | ⏳ TODO |
| InvokeOnMainThreadAsync(Action) | Method | ⏳ TODO |
| InvokeOnMainThreadAsync<T>(Func<T>) | Method | ⏳ TODO |
| InvokeOnMainThreadAsync(Func<Task>) | Method | ⏳ TODO |
| InvokeOnMainThreadAsync<T>(Func<Task<T>>) | Method | ⏳ TODO |
| GetMainThreadSynchronizationContextAsync() | Method | ⏳ TODO |

---

## Map

Open the installed map application to a specific location.

### Interface: IMap

| Member | Type | Status |
|--------|------|--------|
| OpenAsync(double, double, MapLaunchOptions) | Method | ⏳ TODO |
| OpenAsync(Placemark, MapLaunchOptions) | Method | ⏳ TODO |
| TryOpenAsync(double, double, MapLaunchOptions) | Method | ⏳ TODO |
| TryOpenAsync(Placemark, MapLaunchOptions) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| MapLaunchOptions | ⏳ TODO |
| NavigationMode | ⏳ TODO |

---

## MediaPicker

Pick or capture photos and videos from the device.

### Interface: IMediaPicker

| Member | Type | Status |
|--------|------|--------|
| IsCaptureSupported | Property | ⏳ TODO |
| PickPhotoAsync(MediaPickerOptions) | Method | ⏳ TODO |
| PickPhotosAsync(MediaPickerOptions) | Method | ⏳ TODO |
| CapturePhotoAsync(MediaPickerOptions) | Method | ⏳ TODO |
| PickVideoAsync(MediaPickerOptions) | Method | ⏳ TODO |
| PickVideosAsync(MediaPickerOptions) | Method | ⏳ TODO |
| CaptureVideoAsync(MediaPickerOptions) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| MediaPickerOptions | ⏳ TODO |
| FileResult | ⏳ TODO |

---

## OrientationSensor

Monitor the device orientation sensor which reports the orientation in 3D space.

### Interface: IOrientationSensor

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| IsMonitoring | Property | ⏳ TODO |
| Start(SensorSpeed) | Method | ⏳ TODO |
| Stop() | Method | ⏳ TODO |
| ReadingChanged | Event | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| OrientationSensorData | ⏳ TODO |

---

## Permissions

Check and request runtime permissions.

### Static Class: Permissions

| Member | Type | Status |
|--------|------|--------|
| CheckStatusAsync<TPermission>() | Method | ⏳ TODO |
| RequestAsync<TPermission>() | Method | ⏳ TODO |
| ShouldShowRationale<TPermission>() | Method | ⏳ TODO |

### Permission Types

| Permission | Status |
|------------|--------|
| Battery | ⏳ TODO |
| Bluetooth | ⏳ TODO |
| CalendarRead | ⏳ TODO |
| CalendarWrite | ⏳ TODO |
| Camera | ⏳ TODO |
| ContactsRead | ⏳ TODO |
| ContactsWrite | ⏳ TODO |
| Flashlight | ⏳ TODO |
| LaunchApp | ⏳ TODO |
| LocationWhenInUse | ⏳ TODO |
| LocationAlways | ⏳ TODO |
| Maps | ⏳ TODO |
| Media | ⏳ TODO |
| Microphone | ⏳ TODO |
| NearbyWifiDevices | ⏳ TODO |
| NetworkState | ⏳ TODO |
| Phone | ⏳ TODO |
| Photos | ⏳ TODO |
| PhotosAddOnly | ⏳ TODO |
| PostNotifications | ⏳ TODO |
| Reminders | ⏳ TODO |
| Sensors | ⏳ TODO |
| Sms | ⏳ TODO |
| Speech | ⏳ TODO |
| StorageRead | ⏳ TODO |
| StorageWrite | ⏳ TODO |
| Vibrate | ⏳ TODO |

---

## PhoneDialer

Open the phone dialer with a specified number.

### Interface: IPhoneDialer

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| Open(string) | Method | ⏳ TODO |

---

## Preferences

Store application preferences in a key/value store.

### Interface: IPreferences

| Member | Type | Status |
|--------|------|--------|
| ContainsKey(string, string) | Method | ⏳ TODO |
| Remove(string, string) | Method | ⏳ TODO |
| Clear(string) | Method | ⏳ TODO |
| Set<T>(string, T, string) | Method | ⏳ TODO |
| Get<T>(string, T, string) | Method | ⏳ TODO |

---

## Screenshot

Capture a screenshot of the current screen.

### Interface: IScreenshot

| Member | Type | Status |
|--------|------|--------|
| IsCaptureSupported | Property | ✅ Implemented |
| CaptureAsync() | Method | ✅ Implemented |

### Supporting Types

| Type | Status |
|------|--------|
| IScreenshotResult | ⏳ TODO |
| ScreenshotFormat | ⏳ TODO |

---

## SecureStorage

Securely store encrypted key/value pairs.

### Interface: ISecureStorage

| Member | Type | Status |
|--------|------|--------|
| GetAsync(string) | Method | ⏳ TODO |
| SetAsync(string, string) | Method | ⏳ TODO |
| Remove(string) | Method | ⏳ TODO |
| RemoveAll() | Method | ⏳ TODO |

---

## SemanticScreenReader

Announce text through the operating system's screen reader.

### Interface: ISemanticScreenReader

| Member | Type | Status |
|--------|------|--------|
| Announce(string) | Method | ⏳ TODO |

---

## Share

Share data such as text and links with other applications.

### Interface: IShare

| Member | Type | Status |
|--------|------|--------|
| RequestAsync(ShareTextRequest) | Method | ⏳ TODO |
| RequestAsync(ShareFileRequest) | Method | ⏳ TODO |
| RequestAsync(ShareMultipleFilesRequest) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| ShareTextRequest | ⏳ TODO |
| ShareFileRequest | ⏳ TODO |
| ShareMultipleFilesRequest | ⏳ TODO |
| ShareFile | ⏳ TODO |

---

## Sms

Open the default SMS application with a message.

### Interface: ISms

| Member | Type | Status |
|--------|------|--------|
| IsComposeSupported | Property | ⏳ TODO |
| ComposeAsync(SmsMessage) | Method | ⏳ TODO |

### Supporting Types

| Type | Status |
|------|--------|
| SmsMessage | ⏳ TODO |

---

## TextToSpeech

Speak text using the device's text-to-speech engine.

### Interface: ITextToSpeech

| Member | Type | Status |
|--------|------|--------|
| GetLocalesAsync() | Method | ✅ Done |
| SpeakAsync(string, SpeechOptions, CancellationToken) | Method | ✅ Done |

### Supporting Types

| Type | Status |
|------|--------|
| Locale | ✅ Done |
| SpeechOptions | ✅ Done |

---

## VersionTracking

Track application version history on the device.

### Interface: IVersionTracking

| Member | Type | Status |
|--------|------|--------|
| Track() | Method | ⏳ TODO |
| IsFirstLaunchEver | Property | ⏳ TODO |
| IsFirstLaunchForCurrentVersion | Property | ⏳ TODO |
| IsFirstLaunchForCurrentBuild | Property | ⏳ TODO |
| CurrentVersion | Property | ⏳ TODO |
| CurrentBuild | Property | ⏳ TODO |
| PreviousVersion | Property | ⏳ TODO |
| PreviousBuild | Property | ⏳ TODO |
| FirstInstalledVersion | Property | ⏳ TODO |
| FirstInstalledBuild | Property | ⏳ TODO |
| VersionHistory | Property | ⏳ TODO |
| BuildHistory | Property | ⏳ TODO |
| IsFirstLaunchForVersion(string) | Method | ⏳ TODO |
| IsFirstLaunchForBuild(string) | Method | ⏳ TODO |

---

## Vibration

Make the device vibrate.

### Interface: IVibration

| Member | Type | Status |
|--------|------|--------|
| IsSupported | Property | ⏳ TODO |
| Vibrate() | Method | ⏳ TODO |
| Vibrate(TimeSpan) | Method | ⏳ TODO |
| Cancel() | Method | ⏳ TODO |

---

## WebAuthenticator

Handle OAuth-style authentication flows.

### Interface: IWebAuthenticator

| Member | Type | Status |
|--------|------|--------|
| AuthenticateAsync(WebAuthenticatorOptions) | Method | ✅ Done |
| AuthenticateAsync(WebAuthenticatorOptions, CancellationToken) | Method | ✅ Done |

### Supporting Types

| Type | Status |
|------|--------|
| WebAuthenticatorOptions | ✅ Done |
| WebAuthenticatorResult | ✅ Done |
| IWebAuthenticatorResponseDecoder | ✅ Done |

---

## Common Types

These types are used across multiple Essentials APIs.

### Sensor Types

| Type | Status |
|------|--------|
| SensorSpeed | ⏳ TODO |

### Location Types

| Type | Status |
|------|--------|
| Location | ⏳ TODO |
| Placemark | ⏳ TODO |

### File Types

| Type | Status |
|------|--------|
| FileBase | ⏳ TODO |
| FileResult | ⏳ TODO |
| ReadOnlyFile | ⏳ TODO |

---

## Implementation Priority Notes

### High Priority (Core Platform APIs)

These APIs are commonly needed and should be prioritized:

1. **Preferences** - Key/value storage (use Avalonia Preferences or file-based storage)
2. **SecureStorage** - Encrypted storage (use platform keychain/keystore equivalents)
3. **FileSystem** - App directories and bundled files
4. **Clipboard** - Text copy/paste (Avalonia has native support)
5. **MainThread** - UI thread dispatching (Avalonia has Dispatcher)
6. **DeviceInfo** - Basic device information
7. **DeviceDisplay** - Screen metrics
8. **AppInfo** - Application information
9. **VersionTracking** - Version history tracking

### Medium Priority (Useful Desktop APIs)

These APIs have good desktop platform support:

1. **Browser** - Open URLs (use Process.Start or platform launchers)
2. **Launcher** - Open files/URIs
3. **FilePicker** - File selection dialogs (Avalonia has StorageProvider)
4. **Share** - Share content (limited desktop support)
5. **Screenshot** - Screen capture (Avalonia has RenderTargetBitmap)
6. **Connectivity** - Network status

### Low Priority (Mobile-Centric APIs)

These APIs are primarily for mobile devices and may have limited desktop applicability:

1. **Sensors** (Accelerometer, Barometer, Compass, Gyroscope, Magnetometer, OrientationSensor)
2. **Battery** - Battery status
3. **Flashlight** - Camera flash
4. **Geolocation** - GPS location
5. **Geocoding** - Address conversion
6. **HapticFeedback** - Tactile feedback
7. **MediaPicker** - Photo/video capture
8. **PhoneDialer** - Make calls
9. **Sms** - Send SMS
10. **Contacts** - Read contacts
11. **Vibration** - Device vibration

### Platform-Specific Considerations

When implementing for Avalonia:

- **Desktop (Windows/Linux/macOS)**: Most storage, file, and UI-related APIs can be implemented
- **Browser (WebAssembly)**: Limited to web-safe APIs (clipboard, storage via IndexedDB/localStorage)
- **Mobile (if supported)**: Would need platform-specific implementations

---

## Legend

- **⏳ TODO**: Feature is pending implementation
- **🔧 In Progress**: Feature is currently being implemented
- **✅ Implemented**: Feature is fully implemented and ready to use
- **❌ Not Applicable**: Feature cannot be implemented on the target platform(s)

---
