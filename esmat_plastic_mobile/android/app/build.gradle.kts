import java.net.URL
import java.net.HttpURLConnection
import java.io.File

plugins {
    id("com.android.application")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

android {
    namespace = "com.esmatplastic.esmat_plastic_mobile"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = flutter.ndkVersion

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    defaultConfig {
        // TODO: Specify your own unique Application ID (https://developer.android.com/studio/build/application-id.html).
        applicationId = "com.esmatplastic.esmat_plastic_mobile"
        // You can update the following values to match your application needs.
        // For more information, see: https://flutter.dev/to/review-gradle-config.
        minSdk = flutter.minSdkVersion
        targetSdk = flutter.targetSdkVersion
        // Uses the version code from pubspec.yaml. When using split APKs, 1000 * ABI_VERSION
        // is added automatically by Flutter. (https://developer.android.com/studio/build/configure-apk-splits#configure-APK-versions)
        // You can force using the value of versionCode by specifying the `-P force-version-code-ignoring-abi=true`
        // flag during build.
        versionCode = flutter.versionCode
        versionName = flutter.versionName
    }

    buildTypes {
        release {
            // TODO: Add your own signing config for the release build.
            // Signing with the debug keys for now, so `flutter run --release` works.
            signingConfig = signingConfigs.getByName("debug")
        }
    }
}

kotlin {
    compilerOptions {
        jvmTarget = org.jetbrains.kotlin.gradle.dsl.JvmTarget.JVM_17
    }
}

tasks.register("ensureApiRunning") {
    doFirst {
        try {
            val url = URL("http://localhost:5023/api/Health/status")
            val connection = url.openConnection() as HttpURLConnection
            connection.connectTimeout = 2000
            connection.readTimeout = 2000
            connection.requestMethod = "GET"
            if (connection.responseCode == 200) {
                println("EsmatPlastic API is already running.")
                return@doFirst
            }
        } catch (e: Exception) {
            // API not running
        }

        println("Starting EsmatPlastic.API automatically...")
        val rootDir = file("../..").absolutePath
        val processBuilder = ProcessBuilder(
            "cmd.exe", "/c", "start", "EsmatPlastic.API", "dotnet", "run", "--project", "EsmatPlastic.API/EsmatPlastic.API.csproj", "--urls", "http://localhost:5023"
        )
        processBuilder.directory(File(rootDir))
        processBuilder.start()

        var attempts = 0
        while (attempts < 10) {
            Thread.sleep(1000)
            attempts++
            try {
                val url = URL("http://localhost:5023/api/Health/status")
                val connection = url.openConnection() as HttpURLConnection
                connection.connectTimeout = 1000
                connection.readTimeout = 1000
                if (connection.responseCode == 200) {
                    println("EsmatPlastic API started successfully!")
                    return@doFirst
                }
            } catch (e: Exception) {}
        }
        println("Warning: Could not verify EsmatPlastic API startup.")
    }
}

tasks.named("preBuild").configure {
    dependsOn("ensureApiRunning")
}

flutter {
    source = "../.."
}
