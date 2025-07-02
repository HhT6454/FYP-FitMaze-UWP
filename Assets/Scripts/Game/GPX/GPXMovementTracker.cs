using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GPXMovementTracker : MonoBehaviour
{
    private double initialLatitude;
    private double initialLongitude;
    private float movementScale = 0.000009f; // Scale for Unity units to lat/lon

    private List<(double latitude, double longitude, float elevation, string timestamp)> trackPoints = new List<(double, double, float, string)>();
    private double currentLatitude;
    private double currentLongitude;
    private Vector3 lastPosition;

    private int stepCount = 0; // For real-life tracking

    public double GetCurrentLatitude() => currentLatitude;
    public double GetCurrentLongitude() => currentLongitude;

    void Start()
    {
        if (BLEManager.Instance != null && BLEManager.Instance.bleDataHandler != null)
        {
            BLEManager.Instance.bleDataHandler.OnStepReceived += HandleStepReceived;
        }

        int lastIndex = PlayerPrefs.GetInt("SelectedCoordinateIndex", 0);
        lastIndex = Mathf.Clamp(lastIndex, 0, GPXCoordinate.GetSavedCoordinates().Count - 1);
        GPXCoordinate.SetInitialFromSaved(lastIndex);
        initialLatitude = GPXCoordinate.InitialLatitude;
        initialLongitude = GPXCoordinate.InitialLongitude;

        //ResetTracking(); // Always reset when the level starts
    }

    private void OnDestroy()
    {
        if (BLEManager.Instance != null && BLEManager.Instance.bleDataHandler != null)
        {
            BLEManager.Instance.bleDataHandler.OnStepReceived -= HandleStepReceived;
        }
    }

    public void ResetTracking()
    {
        Debug.Log("GPXMovementTracker: Resetting tracking data.");
        trackPoints.Clear();
        stepCount = 0; // Reset step count for real-life tracking

        initialLatitude = GPXCoordinate.InitialLatitude;
        initialLongitude = GPXCoordinate.InitialLongitude;
        currentLatitude = initialLatitude;
        currentLongitude = initialLongitude;

        lastPosition = transform.position;
        AddTrackPoint(); // Record starting position again
    }

    private void HandleStepReceived()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.InGame &&
            FindAnyObjectByType<LevelManager>().CurrentLevelState == LevelManager.LevelState.Playing)
        {
            if (GPXCoordinate.CurrentTrackingMode == GPXCoordinate.TrackingMode.CharacterTracking)
            {
                TrackCharacterMovement();
            }
            else
            {
                TrackRealLifeMovement();
            }
        }
    }

    private void TrackCharacterMovement()
    {
        // Calculate movement in Unity space
        Vector3 movement = transform.position - lastPosition;

        // Update latitude and longitude based on movement
        double deltaLatitude = movement.z * movementScale; // Z-axis affects latitude
        double deltaLongitude = movement.x * (movementScale / Math.Cos(currentLatitude * (Math.PI / 180))); // X-axis affects longitude

        currentLatitude += deltaLatitude;
        currentLongitude += deltaLongitude;

        // Update last position
        lastPosition = transform.position;

        // Record the track point with the current timestamp
        AddTrackPoint();
    }

    private void TrackRealLifeMovement()
    {
        stepCount++; // Increment step count

        // Calculate distance for this step
        float distance = GPXCoordinate.StepLength; // Distance in meters

        // Convert distance to degrees (latitude)
        float distanceInDegrees = distance / 111139f;

        // Generate random direction (0 to 360 degrees)
        float randomAngle = UnityEngine.Random.Range(0f, 360f);

        // Calculate latitude and longitude changes
        double deltaLatitude = distanceInDegrees * Math.Cos(randomAngle * (Math.PI / 180));
        double deltaLongitude = distanceInDegrees * Math.Sin(randomAngle * (Math.PI / 180)) / Math.Cos(currentLatitude * (Math.PI / 180));

        // Update current position
        currentLatitude += deltaLatitude;
        currentLongitude += deltaLongitude;

        // Record track point
        AddTrackPoint();
    }

    private void AddTrackPoint()
    {
        string timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); // ISO 8601 format
        float elevation = 0.0f; // Set to 0 as there's no elevation change in this case

        trackPoints.Add((currentLatitude, currentLongitude, elevation, timestamp));
    }

    public string GenerateGPXData()
    {
        // Try to get LevelManager for extra info
        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        int stepCount = 0;
        float timeTaken = 0f;
        int score = 0;

        if (levelManager != null)
        {
            stepCount = levelManager.GetFinalStepCount();
            timeTaken = levelManager.GetFinalTime();
            score = levelManager.GetFinalScore();
        }

        StringBuilder gpxData = new StringBuilder();
        gpxData.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        gpxData.AppendLine("<gpx version=\"1.1\" creator=\"GPXMovementTracker\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:fitmaze=\"fitmaze\">");
        gpxData.AppendLine("<trk>");
        gpxData.AppendLine($"<name>{(GPXCoordinate.CurrentTrackingMode == GPXCoordinate.TrackingMode.CharacterTracking ? "Character Movement" : "Real-Life Movement")}</name>");
        gpxData.AppendLine("<extensions>");
        gpxData.AppendLine($"  <fitmaze:stepCount>{stepCount}</fitmaze:stepCount>");
        gpxData.AppendLine($"  <fitmaze:timeTaken>{timeTaken:F2}</fitmaze:timeTaken>");
        gpxData.AppendLine($"  <fitmaze:score>{score}</fitmaze:score>");
        gpxData.AppendLine("</extensions>");
        gpxData.AppendLine("<trkseg>");

        foreach (var point in trackPoints)
        {
            gpxData.AppendLine($"<trkpt lat=\"{point.latitude}\" lon=\"{point.longitude}\">");
            gpxData.AppendLine($"  <ele>{point.elevation}</ele>");
            gpxData.AppendLine($"  <time>{point.timestamp}</time>");
            gpxData.AppendLine($"</trkpt>");
        }

        gpxData.AppendLine("</trkseg>");
        gpxData.AppendLine("</trk>");
        gpxData.AppendLine("</gpx>");

        return gpxData.ToString();
    }
}
