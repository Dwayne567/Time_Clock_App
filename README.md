# Timeclock Web Application

## Overview
Timeclock is a comprehensive web application designed for tracking employee time, managing jobs, and handling leave requests. It provides a seamless experience for both employees and administrators to manage daily operations efficiently.

## Features
- **Time Tracking**: Employees can easily clock in and out, tracking their daily duties and hours.
- **Job Management**: Complete job tracking system allowing users to associate time entries with specific jobs.
- **Leave Management**: Submit leave requests directly through the portal.
- **Role-Based Access**:
    - **User Dashboard**: Personalized view for employees to manage their time and tasks.
    - **Admin Dashboard**: Centralized control for administrators to oversee users, jobs, and system settings.

## Technology Stack
- **Backend**: ASP.NET Core 6 Web API
- **Frontend**: Angular 19 SPA
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity

## Project Structure
```
Time_Clock_App/
├── Backend/         # ASP.NET Core Web API
├── Frontend/        # Angular SPA
├── docs/            # Documentation and screenshots
└── README.md
```

## Dashboards

### User Dashboard
The user dashboard provides quick access to daily tasks, current job status, and time entry history.
![User Dashboard](docs/user_dashboard.png)

### Admin Dashboard
The admin dashboard offers powerful tools for managing the workforce, approving requests, and generating reports.
![Admin Dashboard](docs/admin_dashboard.png)