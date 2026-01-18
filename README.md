# Travel Agency Service – MVC Web Application

A full-featured travel agency management system developed as a course project for  
**Introduction to Computer Communications**.

**Developer:** Yael Levin  
**Course:** Introduction to Computer Communications  
**Instructor:** Pedut Shokron  
**Date:** January 2026

---

## About the Project

**Travel Agency Service** is a web-based booking platform developed using  
**ASP.NET Core MVC**.

The system simulates a real-world travel agency website (similar to Booking.com)  
with an emphasis on booking workflows, shopping cart behavior, waiting list promotion,
secure payment processing, and automated system notifications.

The system supports **role-based access** with separate views and permissions for  
**Admin** and **Users**, including full booking logic, waiting list handling, automated
cancellations, email notifications, and PDF generation.

---

## Main Features

### Admin Features
- Add, edit, and delete travel packages
- Manage trip visibility, categories, and images
- Define destination, country, travel dates, prices, available rooms, age limitations, and descriptions
- Apply **temporary discounts** (up to 7 days) with strikethrough original price
- Manage registered users (view, deactivate, delete under constraints)
- Define booking and cancellation rules (system-wide settings)
- View user booking history
- Manage waiting lists
- Automatic system maintenance logic (discount expiration, auto-cancel, reminders)

---

### User Features
- Search trips by destination, country, package name, or partial keyword
- Filter trips by category, price range, travel dates, and discounts
- Sort trips by price, popularity, or travel date
- Book trips (maximum **3 active upcoming trips**)
- Choose between **Book (shopping cart)** and **Buy Now (direct payment)**
- Join a waiting list for sold-out trips
- Leave the waiting list manually
- Make payments using credit card details (never stored)
- View personal dashboard:
  - Pending bookings (cart)
  - Upcoming trips
  - Past trips
  - Waiting list entries
- Cancel trips within the allowed cancellation period
- Download:
  - **Trip itinerary PDF**
  - **Payment receipt PDF**
- Rate trips and the overall service experience

---

## Booking Logic

- Each booking has one of three statuses:
  - **Booked (0)** – a shopping cart item that does **not** reserve rooms
  - **Paid (1)** – a confirmed booking that **reserves rooms**
  - **Canceled (2)** – a canceled booking that does **not** reserve rooms

- Rooms are reserved **only after successful payment**
- Users promoted from the waiting list have **24 hours** to complete payment
- If payment is not completed within 24 hours:
  - The booking is **automatically cancelled**
  - The next user in the waiting list is promoted

---

## Waiting List Management

- Users can join a waiting list when a trip is fully booked
- Waiting list is handled using **FIFO order**
- When rooms become available:
  - Users are promoted from the waiting list in order, **as long as sufficient rooms are available**
  - Multiple users may be promoted following a single cancellation if enough rooms are freed
  - For each promoted user:
    - A booking is created automatically
    - An **email notification** is sent with a 24-hour payment window
- Users can:
  - Complete payment within 24 hours
  - Cancel or leave the waiting list manually

---

## Trip Gallery

- Displays a list of travel packages including:
  - Destination
  - Country
  - Travel dates
  - Price (original + discounted if active)
  - Available rooms
  - Package type
  - Age limitation
  - Images
- Contains **at least 25 trips** in the database
- Dynamic trip count is displayed on the main page
- Trips can be filtered to show **only discounted packages**
- Some destinations may appear in multiple years
- Each trip has **Add To Cart** and **Buy Now** options

---

## PDF Generation

- **Trip Itinerary PDF**
  - Trip details
  - Travel dates
  - Passenger information
  - Trip image
  - Logo
- **Payment Receipt PDF**
  - Payment details
  - Booking details
  - Customer email
  - Trip image
  - Logo

PDFs are generated using **QuestPDF**.

---

## Email Notifications

Email notifications are implemented using the built-in SMTP functionality provided by  
.NET (`System.Net.Mail`).

HTML-formatted emails are sent for:
- Successful payment confirmation (including attached payment receipt PDF)
- Promotion from the waiting list (24-hour payment window)
- Automatic booking cancellation after 24 hours without payment
- Manual trip cancellation by the user
- Trip reminders before departure

Emails include trip images, booking details, Logo, and branded HTML layout.

---

## Technologies & Libraries

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core (without migrations)
- SQL Server
- Bootstrap 5
- QuestPDF
- System.Net.Mail (built-in .NET SMTP)
- Session-based authentication
- HTTPS enabled

---

## Database

- Database was created **manually in SQL Server Management Studio**
- Entity Framework Core is used **without migrations**
- The database is provided in two formats:
    - `TravelAgencyDB.bak` – Full SQL Server backup (schema + data)
    - `TravelAgencyDB.sql` – SQL script for recreating the database

### Main Tables
- Users
- Trips
- Bookings
- Payments
- WaitingList
- TripReviews
- ServiceReviews
- SystemSettings

---

## Database Setup Options

### Option 1 – Restore from `.bak`
1. Open SQL Server Management Studio
2. Restore the database from the provided `.bak` file
3. Update the connection string in `appsettings.json`

### Option 2 – Run SQL Script
1. Open SQL Server Management Studio
2. Run the provided `.sql` script
3. Update the connection string in `appsettings.json`

---

## Security

- HTTPS enabled
- Role-based authorization
- Credit card numbers are **never stored**
- Passwords are stored using **hashed passwords**
- Password hashing and verification implemented via a dedicated helper
- Admin actions are protected using custom authorization filters

---

## Project Structure (MVC)

- **Controllers** – request handling and business logic
- **Models** – database entities
- **ViewModels** – data transfer objects for views
- **Views** – Razor UI pages
- **Services** – email notifications, PDF generation, system logic
- **Helpers** – password hashing
- **Filters** – authorization and system maintenance
- **Data** – database context
- **wwwroot** – static files (CSS, JS, images)

---

## Course Requirements Compliance

This project fully complies with the course requirements, including:
- Admin User & trip management
- User booking system
- Shopping cart and payment flow
- Waiting list with FIFO promotion and notifications
- Trip catalog with filters and sorting
- Secure payment handling
- User dashboard
- Reviews and ratings
- PDF generation
- Email notifications
- Concurrency and logical constraints handling

---

## Additional Implementations (Beyond Core Requirements)

- Password hashing and verification helper
- Change password functionality
- Splash screen on application entry
- Automatic waiting list promotion and cancellation
- 24-hour payment window for promoted users
- Background system maintenance filter
- Manual removal from waiting lists
- Scroll-to-top navigation button

---

## Notes

This project was developed as part of the requirements for the  
**Introduction to Computer Communications** course.
