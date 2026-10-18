# Development setup

Instructions for Windows with PowerShell. Use PowerShell 7 or later: it writes UTF-8 files without a byte order mark by default, which matches `.editorconfig`.

## 1. Install the tools

| Tool | How |
| --- | --- |
| .NET 10 SDK | <https://dotnet.microsoft.com/download/dotnet/10.0> |
| IDE | Visual Studio 2026 with the **.NET Multi-platform App UI development** workload, or JetBrains Rider. Command-line builds work with the SDK alone. |
| .NET MAUI Android workload | `dotnet workload install maui-android` (skip if installed through Visual Studio) |
| Microsoft OpenJDK 21 | <https://learn.microsoft.com/java/openjdk/download> |
| Android SDK | Installed by Visual Studio, or with the command in step 3 |
| Node.js 24 LTS | <https://nodejs.org> |
| Git and GitHub CLI | `winget install Git.Git GitHub.cli` |

Check the installation:

```powershell
dotnet --info
dotnet workload list
java -version
node --version
gh auth status
```

## 2. Clone

```powershell
cd C:\Users\janlu\source\repos
git clone https://github.com/janlu89/J-HandsFree.git
cd J-HandsFree
git switch dev
```

## 3. Install Android dependencies

Use absolute paths without spaces. The target installs the Android SDK and a JDK into these folders when they are missing:

```powershell
$installArgs = @('build', 'src\JHandsFree.Mobile', '-t:InstallAndroidDependencies', '-f', 'net10.0-android', '-p:AndroidSdkDirectory=C:\Android\sdk', '-p:JavaSdkDirectory=C:\Android\jdk', '-p:AcceptAndroidSdkLicenses=True')
dotnet @installArgs
```

If OpenJDK 21 is already installed, point `JavaSdkDirectory` to it instead.

## 4. Prepare the phone

1. **Settings → About phone →** tap **Build number** seven times to enable developer options.
2. **Developer options →** enable **USB debugging**.
3. Connect the phone by USB and accept the debugging prompt.
4. Update **Android System WebView** and **Chrome** from the Play Store.
5. Confirm the phone is visible:

```powershell
adb devices
```

## 5. Build, test and run

```powershell
npm --prefix src/player-scripts ci
dotnet build JHandsFree.slnx
dotnet test JHandsFree.slnx
dotnet build src\JHandsFree.Mobile -t:Run -f net10.0-android
```

Building Infrastructure.Android also builds the player scripts: it runs `npm ci` when `package-lock.json` changed and `npm run build` when a script changed, then embeds the bundle. Node.js must therefore be on the `PATH` for every .NET build.

The last command installs and starts the app on the connected phone. If the install fails with `INSTALL_FAILED_USER_RESTRICTED`:

- Unlock the phone and accept the install prompt. Some vendors, such as Xiaomi with **Install via USB**, ask for every new app installed over USB.
- On a phone with more than one Android user, install for the owner only: add `-p:AndroidDeviceUserId=0` to the last command.

## 6. Before opening a pull request

Run the local checks listed in [CONTRIBUTING.md](../CONTRIBUTING.md#local-checks).
