

using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace AirplaneBooking.Infrastructure.Persistence;

public class FirebaseDbContext
{
    public FirestoreDb FirestoreDb { get; }

    public FirebaseDbContext(IConfiguration configuration)
    {
        // Initialize Firebase Admin SDK
        // This typically involves setting the GOOGLE_APPLICATION_CREDENTIALS environment variable
        // pointing to your service account key JSON file, or loading it directly.
        // For local development, setting the env variable is convenient.
        // For deployment, consider secure ways to provide credentials (e.g., Kubernetes secrets, Azure Key Vault).

        var projectId = configuration["Firebase:ProjectId"];
        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Firebase ProjectId not found in configuration.");
        }

        FirestoreDb = FirestoreDb.Create(projectId);
    }
}
