#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Storage.Pickers;
using System;
#endif
using UnityEngine;

public class GPXFileSaver : MonoBehaviour
{
    private GPXMovementTracker tracker;

    void Start()
    {
        FindTracker();
    }

    private void FindTracker()
    {
        tracker = FindAnyObjectByType<GPXMovementTracker>(); // Find the tracker dynamically

        if (tracker == null)
        {
            Debug.LogError("GPXFileSaver: No GPXMovementTracker found in the scene.");
        }
    }

    public void SaveGPXFileUWP()
    {
#if ENABLE_WINMD_SUPPORT
        Debug.Log("UWP File Save initiated...");

        // Generate GPX data on the Unity main thread
        if (tracker == null)
        {
            tracker = FindAnyObjectByType<GPXMovementTracker>();
            if (tracker == null)
            {
                Debug.LogError("GPXFileSaver: No GPXMovementTracker found in the scene.");
                return;
            }
        }
        string gpxData = tracker.GenerateGPXData();

        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            try
            {
                // Create a file picker
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("GPX File", new[] { ".gpx" });
                string timestamp = System.DateTime.Now.ToString("ddMMyy_HHmmss"); // Format: DDMMYY_HHMMSS
                savePicker.SuggestedFileName = $"fitmaze{timestamp}";

                Debug.Log("Displaying file picker...");
                // Show the save picker
                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    Debug.Log($"File selected: {file.Path}");

                    // Write the GPX data to the selected file
                    Debug.Log("GPX data generated.");

                    await FileIO.WriteTextAsync(file, gpxData);
                    Debug.Log("GPX file successfully saved.");
                }
                else
                {
                    Debug.Log("Save operation was canceled by the user.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred: {ex.Message}\n{ex.StackTrace}");
            }

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }, false);
        }, false);
#else
        Debug.LogError("This file save method only works on UWP.");
#endif
    }
}
