# Booking System API

Booking System API is a robust backend system for managing hotel reservations, room bookings, reviews, and payments.  
Built with **ASP.NET Core 8.0** and integrated with **Cloudinary** and **VNPAY**, it provides APIs for hotel management, room management, authentication, and payment processing.  
The project follows a **Code First** approach with Entity Framework Core.

---

## Overview

**Language:** C#  
**Framework:** ASP.NET Core 8.0  
**Purpose:** Manage hotels, rooms, reservations, reviews, and payments  
**Database:** SQL Server  

---

## Features

- User authentication & role-based authorization  
- Hotel and room management  
- Payment processing with **VNPAY**  
- Email notifications  
- Review system for hotels  
- Integration with **Cloudinary** for image storage  
- API documentation with **Swagger**  

---

## Project Requirements

### Environment
- .NET SDK 8.0  
- SQL Server database  
- Cloudinary account credentials  
- VNPAY credentials  

---

### Cloudinary
- Used for image storage and delivery.  
- You can sign up for a free account at:  
  [https://cloudinary.com](https://cloudinary.com)  
- After registration, you'll get:  
  - `CLOUDINARY_CLOUD_NAME`  
  - `CLOUDINARY_API_KEY`  
  - `CLOUDINARY_API_SECRET`  

---

### VNPAY (Payment Gateway)
- Used for handling online payment processing.  
- Register for merchant credentials at:  
  [http://sandbox.vnpayment.vn/devreg/](http://sandbox.vnpayment.vn/devreg/)  
- After registration, you'll get:  
  - `VNP_TMNCODE`  
  - `VNP_HASHSECRET`  
  - `VNP_URL`  

---

## How to Setup and Run the Project

### 1. Clone the Repository
```bash
git clone https://github.com/xbensieve/booking-system-api.git
cd booking-system-api
````

---

### 2. Setup Environment Variables

Create a `.env` file in the `Booking.Api` directory with the following content:

```env
Cloudinary__CloudName=your_cloud_name
Cloudinary__ApiKey=your_api_key
Cloudinary__ApiSecret=your_api_secret

VNP_TMN_CODE=your_vnp_tmn_code
VNP_HASH_SECRET=your_vnp_hash_secret
VNP_COMMAND=pay
VNP_CURRCODE=VND
VNP_VERSION=2.1.0
VNP_LOCALE=vn
VNP_BASE_URL=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
VNP_RETURN_URL=https://localhost:5133/api/payments/handle-payment-response

MailSettings__EMAIL=your_email
MailSettings__PASSWORD=your_email_password
```

---

### 3. Setup the Database (Entity Framework Core - Code First)

If this is the **first time** you run the project, create a new migration:

```bash
dotnet ef migrations add InitialCreate --project Booking.Infrastructure --startup-project Booking.Api
```

Then update the database:

```bash
dotnet ef database update --project Booking.Infrastructure --startup-project Booking.Api
```

For later schema changes, run:

```bash
dotnet ef migrations add MigrationName --project Booking.Infrastructure --startup-project Booking.Api
dotnet ef database update --project Booking.Infrastructure --startup-project Booking.Api
```

---

### 4. Run the Application

#### Using Visual Studio

1. Open `RoomBooking.Backend.sln` in Visual Studio.
2. Set **Booking.Api** as the startup project.
3. Press `F5` to run.

#### Using Command Line

```bash
cd Booking.Api
dotnet run
```

API will be available at:
👉 `http://localhost:5133`

---

## API Documentation

Swagger is enabled for API documentation. Once the application is running, navigate to:
 [http://localhost:5133/swagger](http://localhost:5133/swagger)

---

## Troubleshooting

### Database Errors

* Ensure your connection string in `appsettings.json` points to a valid SQL Server instance.
* Run `dotnet ef database update` after adding migrations.

### Cloudinary Configuration Errors

* Check that your `.env` values for Cloudinary are correct.

### VNPAY Payment Issues

* Ensure sandbox credentials are correct.
* Check `VNP_RETURN_URL` matches your API running port.

---

## License

This project is licensed under the **MIT License**.
See the [LICENSE](LICENSE) file for details.
