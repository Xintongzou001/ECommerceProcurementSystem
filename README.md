# ECommerceProcurementSystem

## Project Overview
This project is a modern ASP.NET Core MVC web application for managing procurement data, integrating real-world datasets from the City of Austin's Socrata Open Data API. It supports offline CRUD operations, interactive data visualization, and a responsive, user-friendly interface. The system is designed for both technical and non-technical users, with a focus on maintainability, extensibility, and clarity.

---

## API Endpoints Used
### External (Socrata Open Data API)
- **Base Endpoint:**
  - `https://data.austintexas.gov/resource/3ebq-e9iz.json`
- **Purpose:**
  - Used to import purchase order and annual report data into the local database, especially on first run or when the database is empty.
- **Authentication:**
  - Uses an AppToken (API key) for higher rate limits, stored securely in `appsettings.json` or user secrets.
- **Documentation:**
  - [Socrata API Docs](https://dev.socrata.com/foundry/data.austintexas.gov/3ebq-e9iz)

### Internal (Application Endpoints)
- **/Home/GetAnnualSalesData**
  - Returns annual sales data as JSON for use in Chart.js visualizations on the Data Exploration page.
- **/Home/GetVendorSalesData**
  - Returns total sales by vendor (including vendor code and name) as JSON for use in Chart.js visualizations.
- **/Home/GetCitySalesData**
  - Returns total sales by city (including city name) as JSON for use in Chart.js visualizations.
- **/AnnualReport/**
  - Full CRUD endpoints for managing annual reports (Create, Read, Update, Delete).
- **/PurchaseOrders/**
  - Paginated list and details for purchase orders, using data imported from the API and/or stored locally.

---

## Data Model (ERD)
The system uses a normalized relational data model, designed for clarity and extensibility. The main entities and their relationships are:

- **AnnualReport**: Links to `City` and `Vendor`, contains year and total sale amount.
- **PurchaseOrder**: Links to `Vendor` and `MasterAgreement`, and has many `PurchaseOrderLine` entries.
- **PurchaseOrderLine**: Represents line items in a purchase order, links to `Commodity`.
- **City, Vendor, Commodity, MasterAgreement**: Reference tables for normalization and data integrity.

**See the updated ERD diagram:**
- `wwwroot/images/data-model.png`

**Example (AnnualReport):**
```csharp
public class AnnualReport {
    public int ID { get; set; }
    public int CityID { get; set; }
    public string Vendor_Code { get; set; }
    public int Year { get; set; }
    public decimal SaleAmount { get; set; }
    public City? City { get; set; }
    public Vendor? Vendor { get; set; }
}
```

---

## Overview of CRUD Implementation
- **Annual Reports:**
  - Full Create, Read, Update, Delete operations via MVC controller and strongly-typed Razor views.
  - Dropdowns for City and Vendor are dynamically populated from the database for data integrity and usability.
  - On first access, if the database is empty, up to 20 records are imported from the Socrata API and mapped to the local schema.
  - All forms and tables are styled with Bootstrap and custom CSS for a consistent, accessible experience.

- **Purchase Orders:**
  - Read-only, paginated list and details view.
  - Data is fetched from the Socrata API and/or local database, with navigation to detailed line items.

- **Data Exploration:**
  - Interactive Chart.js visualizations (e.g., annual sales by year) using data from the local database.
  - Data is delivered via a dedicated JSON endpoint for frontend flexibility and performance.

---

## Notable Technical Challenges and Solutions
### 1. API Data Mapping
- **Challenge:** The Socrata API data structure does not match the local database schema (e.g., grouping, aggregation, and normalization required).
- **Solution:** Custom mapping logic in `SocrataService` groups and transforms API data into the normalized local model, ensuring referential integrity and efficient queries.

### 2. Null Handling in LINQ
- **Challenge:** Entity Framework Core does not support the null-propagating operator (`?.`) in LINQ-to-Entities queries, causing runtime errors.
- **Solution:** All null checks are performed outside of LINQ queries, and only safe, supported expressions are used in database queries.

### 3. Dependency Injection for API Services
- **Challenge:** Controllers require access to external API services with configuration (base URL, API key).
- **Solution:** `ISocrataService` and `SocrataService` are registered using `AddHttpClient` in `Program.cs`, with configuration values injected from `appsettings.json` for seamless, testable service consumption.

### 4. Responsive and Consistent UI
- **Challenge:** Ensuring the application is visually consistent and usable across devices and screen sizes.
- **Solution:** Bootstrap and custom CSS are used for layout, navigation, tables, and forms. All pages are tested for responsiveness and accessibility.

### 5. Data Visualization
- **Challenge:** Presenting complex data in an accessible, interactive way for non-technical users.
- **Solution:** Chart.js is integrated for dynamic, responsive charts. Data is provided via a backend JSON endpoint, allowing for easy extension to more chart types or filters in the future.

---

## How to Present and Use This Project
- **Navigation:** Start by showing the navigation bar, which provides access to Home, Annual Reports, Purchase Orders, Data Exploration, and About Us.
- **CRUD Demo:** Demonstrate creating, editing, and deleting an annual report. Show how dropdowns are populated and how validation works.
- **Data Import:** Explain how the app fetches real-world data from the Socrata API if the database is empty, and how this data is mapped and normalized.
- **Data Visualization:** Use the Data Exploration page to highlight trends and insights with interactive charts.
- **Code Quality:** Point out code comments, docstrings, and the organized folder structure (Models, Views, Controllers, Services, Data).
- **Responsiveness:** Resize the browser to show the app works on all devices.

---

## Getting Started
1. Clone the repository and restore NuGet packages.
2. Configure your database connection in `appsettings.json`.
3. (Optional) Set your Socrata AppToken in user secrets or environment variables for higher API limits.
4. Run database migrations if needed (`dotnet ef database update`).
5. Start the app (`dotnet run`) and navigate to Home, Annual Reports, Purchase Orders, or Data Exploration.

---

## Further Information
- For the ERD, see `wwwroot/images/data-model.png`.
- For styling details, see `wwwroot/css/site.css` and `Views/Shared/_Layout.cshtml.css`.

---

*Last updated: May 6, 2025*