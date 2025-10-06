# 🎫 Ticket Clearer

A professional Windows Forms application for automated clearing of old tickets from SQL Server databases. Maintain database performance by systematically clearing tickets that meet specific age and status criteria.

![Windows Forms](https://img.shields.io/badge/Windows%20Forms-.NET%204.8-blue)
![C#](https://img.shields.io/badge/C%23-10.0-green)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2016+-red)
![License](https://img.shields.io/badge/License-MIT-yellow)

## ✨ Features

### 🎯 Core Functionality
- **Automated Ticket Clearing**: Bulk clear tickets older than 2 months with specific status criteria
- **Smart Status Management**: Automatically sets ticket status to 1009 (Cleared) with appropriate clearance information
- **Work Order Validation**: Checks for outstanding work orders before clearing tickets
- **Batch Processing**: Process multiple tickets simultaneously with visual progress tracking

### 🛡️ Safety & Reliability
- **Pre-Clearance Validation**: Verifies ticket status and checks for outstanding work
- **Comprehensive Logging**: Detailed operation logs with file-based persistence
- **Error Handling**: Robust exception handling with user-friendly error messages
- **Confirmation Dialogs**: Requires user confirmation before clearing operations

### 🎨 User Experience
- **Modern UI**: Clean, professional interface with dark theme
- **Real-time Progress**: Visual progress bar for batch operations
- **Color-coded Results**: Success/failure indicators with color coding
- **Connection Testing**: Built-in database connectivity verification

## 🚀 Quick Start

### Prerequisites
- **.NET Framework 4.8**
- **SQL Server 2016+** 
- **Windows 7+** operating system

### Installation

1. **Download the latest release** from the [Releases page](https://github.com/MolfTp/TicketClearer/releases)

2. **Configure the database connection** in `App.config`:
```xml
<add key="SQLConn" value="Persist Security Info=True;User ID=WebFUser;Password=YourPassword;Initial Catalog=WF1ActR;Data Source=YourServer;Connect Timeout=40" />
