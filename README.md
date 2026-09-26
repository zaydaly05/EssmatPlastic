# EssmatPlastic

## Firebase setup

The API keeps desktop operations on local SQLite and synchronizes records with the `essmat-plastic` Firestore project every three seconds. The mobile app uses the API only to sign in; its dashboard reads Firestore directly with a Firebase ID token.

1. Create a Firestore Native database in Firebase project `essmat-plastic` and enable Firebase Authentication.
2. Deploy [`firestore.rules`](firestore.rules). The current mobile rules allow dashboard reads only; client writes remain denied until mobile workflows and their validation rules are implemented.
3. Configure these environment variables for the API process:

 ```text
 Firebase__ProjectId=essmat-plastic
 Firebase__WebApiKey=<Firebase Web API key>
 GOOGLE_APPLICATION_CREDENTIALS=<path to service-account JSON outside this repository>
 ```

 The Web API key is public client configuration; restrict it to the Firebase Authentication API. Never commit or share the service-account JSON.
4. Grant the service account Firestore read/write access. The API uses Application Default Credentials for Firestore synchronization and Firebase custom-token creation.
5. Restart the API and check `GET /api/health/status`. `isFirestoreOnline` becomes `true` after the initial sync succeeds; SQLite remains usable when it is `false`.
