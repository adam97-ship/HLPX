# GitHub Actions - Automatic Build Setup

## Quick Start

### Step 1: Create GitHub Account
1. Go to https://github.com
2. Sign up for a free account
3. Verify your email

### Step 2: Create New Repository
1. Click **+** icon → **New repository**
2. Name: `ChaosLauncher`
3. Description: `Professional Gaming Optimizer`
4. Choose **Public** (for free CI/CD)
5. Click **Create repository**

### Step 3: Upload Project Files
Option A - Using Git Command Line:
```bash
cd ChaosLauncher
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/ChaosLauncher.git
git push -u origin main
```

Option B - Using GitHub Web Interface:
1. Click **Add file** → **Upload files**
2. Drag and drop all files from ChaosLauncher folder
3. Click **Commit changes**

### Step 4: GitHub Actions Automatic Build
1. Go to your repository
2. Click **Actions** tab
3. The workflow will automatically start building
4. Wait for build to complete (5-10 minutes)

### Step 5: Download EXE
1. Go to **Actions** tab
2. Click the latest build (green checkmark)
3. Scroll down to **Artifacts**
4. Click **ChaosLauncher-EXE** to download
5. Extract and run `ChaosLauncher.exe`

## What Gets Built

- **ChaosLauncher.exe** - Single executable file (~150-200MB)
- **Self-contained** - No .NET SDK required
- **Admin privileges** - Automatically requested on startup
- **Portable** - Works on any Windows 10/11 64-bit system

## Troubleshooting

### Build Failed
- Check **Actions** tab for error messages
- Ensure all files were uploaded correctly
- Try uploading again

### EXE Won't Run
- Right-click → **Run as Administrator**
- Check Windows Defender isn't blocking it
- Ensure Windows 10/11 64-bit

### File Too Large
- This is normal (150-200MB includes .NET runtime)
- Can be reduced with `PublishTrimmed=true`

## Automatic Releases

To create automatic releases:

1. Create a git tag:
```bash
git tag v1.0.0
git push origin v1.0.0
```

2. GitHub Actions will automatically:
   - Build the project
   - Create a Release
   - Upload EXE as release asset

3. Download from **Releases** page

## Continuous Updates

Every time you push to `main` branch:
1. GitHub Actions automatically builds
2. New EXE available in Artifacts
3. No manual build needed!

---

**CHAOS LAUNCHER** - Automated Build System
