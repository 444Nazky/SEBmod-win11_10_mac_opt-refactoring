# SEBmod — Refactored Safe Exam Browser

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL_2.0-brightgreen.svg?style=for-the-badge)](https://opensource.org/licenses/MPL-2.0)
[![Platform - Windows](https://img.shields.io/badge/Platform-Windows_10_%7C_11-0078D4?style=for-the-badge&logo=windows)](https://microsoft.com)
[![Platform - macOS](https://img.shields.io/badge/Platform-macOS-000000?style=for-the-badge&logo=apple)](https://apple.com)

Refactored and optimized version of **Safe Exam Browser** featuring an integrated Chromium engine and updated multi-platform support.

---

## 📦 Access & Downloads

Modded files and binaries are hosted via Google Drive:

🔗 **[Download Modded Files (Google Drive)](https://drive.google.com/file/d/1DErsvb3QfmOQ-S_Pyr0ypL0W3vTrWSAJ/view?usp=drive_link)**

> **Note:** Contact Nazky on Teams if you require access permissions or technical assistance with the custom builds.

---

## 🛠 System Requirements & Dependencies

### Windows
- **Minimum OS:** Windows 10 (Version 1803) / Windows 11
- **[.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48)**
- **[Visual C++ 2015–2022 Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)**

### macOS
- **Minimum OS:** macOS 11.0 (Big Sur) or newer
- **Xcode 14.3+** (for building from source)

---

## 📂 Repository Layout

```plaintext
.
├── base/                 # Base SEB core components and wrappers
├── imp/                  # Refactored implementation modules
├── mac/                  # macOS native controllers & Swift/Obj-C code
├── moddedfiles.seb       # Custom SEB configuration / mod profile
├── SafeExamBrowser.sln   # Main Visual Studio solution file
├── appveyor-release.yml  # AppVeyor CI/CD release workflow
└── appveyor-test.yml     # AppVeyor CI/CD testing workflow
contributor
