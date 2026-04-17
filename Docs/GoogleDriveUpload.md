# Google Drive APK Upload – Setup Guide

The `upload-to-gdrive.yml` workflow automatically uploads the Android APK to a Google Drive folder every time the **Build project** workflow completes successfully.

Two repository secrets must be configured before the workflow can run:

| Secret name | Description |
|---|---|
| `GDRIVE_CREDENTIALS_DATA` | Base64-encoded Google Cloud service account key (JSON) |
| `GDRIVE_FOLDER_ID` | ID of the Google Drive folder to upload into |

---

## 1. Create a Google Cloud Service Account

1. Open the [Google Cloud Console](https://console.cloud.google.com/) and select (or create) a project.
2. Go to **APIs & Services → Library** and enable the **Google Drive API**.
3. Go to **APIs & Services → Credentials** and click **Create Credentials → Service Account**.
4. Give the service account a name (e.g. `github-apk-uploader`) and click **Done**.
5. Click the newly created service account, then open the **Keys** tab.
6. Click **Add Key → Create new key**, choose **JSON**, and download the file.

---

## 2. Get the `GDRIVE_CREDENTIALS_DATA` value

The workflow expects the service account JSON to be **base64-encoded**.

Run the following command in your terminal (replace the filename with your downloaded key):

```bash
base64 -w 0 your-service-account-key.json
```

Copy the entire output string – that is the value for `GDRIVE_CREDENTIALS_DATA`.

> **Windows (PowerShell):**
> ```powershell
> [Convert]::ToBase64String([IO.File]::ReadAllBytes("your-service-account-key.json"))
> ```

---

## 3. Share the target Google Drive folder with the service account

1. Open [Google Drive](https://drive.google.com/) and navigate to the folder where APKs should be uploaded (create one if needed).
2. Right-click the folder → **Share**.
3. Paste the service account's email address (visible on the Credentials page; looks like `name@project-id.iam.gserviceaccount.com`).
4. Set the permission to **Editor** and click **Send**.

---

## 4. Get the `GDRIVE_FOLDER_ID` value

Open the target folder in Google Drive. The folder ID is the last part of the URL:

```
https://drive.google.com/drive/folders/<FOLDER_ID>
```

Copy everything after `/folders/` — that is the value for `GDRIVE_FOLDER_ID`.

---

## 5. Add the secrets to GitHub

1. In the repository, go to **Settings → Secrets and variables → Actions**.
2. Click **New repository secret** and add each secret:

| Name | Value |
|---|---|
| `GDRIVE_CREDENTIALS_DATA` | The base64 string from Step 2 |
| `GDRIVE_FOLDER_ID` | The folder ID from Step 4 |

---

## How the workflow runs

1. Push code or open a pull request to trigger the **Build project** workflow.
2. Once that build finishes successfully, **Upload APK to Google Drive** starts automatically.
3. It downloads the `Build-Android` artifact, locates the `.apk` file, and uploads it to your Drive folder, overwriting any previous upload with the same name.
