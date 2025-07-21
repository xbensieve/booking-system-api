
# RoomBooking Backend

RoomBooking Backend is a robust and scalable backend system for managing hotel reservations, room bookings, and payments. Built with ASP.NET Core and integrated with Firebase, Cloudinary, and VNPAY, it provides APIs for hotel management, room management, user authentication, and payment processing. This project is designed to support modern web and mobile applications with seamless integration and high performance.


## Overview

**Language:** C#

**Framework:** ASP.NET Core 8.0

**Purpose:** Manage hotel reservations, rooms, reviews, and payments

**Database:**

<img width="1498" height="763" alt="image" src="https://github.com/user-attachments/assets/74a0a722-c9d1-4df8-91e2-ff2172115382" />

## Features:
- User authentication via Firebase
- Hotel and room management
- Payment processing with VNPAY
- Notification via email
- Review system for hotels
- Integration with Cloudinary for image storage

## Project Requirements

### Environment:
  - .NET SDK 8.0
  - Docker
  - Firebase Admin SDK credentials (firebase-adminsdk.json)
  - Cloudinary account credentials
  - VNPAY credentials 
  - SQL Server database

### Cloudinary
- Used for image storage and delivery.
- You can sign up for a free account at:  
  [https://cloudinary.com](https://cloudinary.com)
- After registration, you'll get:
  - `CLOUDINARY_CLOUD_NAME`
  - `CLOUDINARY_API_KEY`
  - `CLOUDINARY_API_SECRET`

### VNPAY (Payment Gateway)
- Used for handling online payment processing.
- Register for merchant credentials at:  
  [http://sandbox.vnpayment.vn/devreg/](http://sandbox.vnpayment.vn/devreg/)
- After registration, you'll get:
  - `VNP_TMNCODE`
  - `VNP_HASHSECRET`
  - `VNP_URL`

### Dependencies:
  - CloudinaryDotNet
  - FirebaseAdmin
  - Microsoft.EntityFrameworkCore

## How to Setup and Run the Project

1. Clone the Repository

```bash
  git clone https://github.com/xbensieve/smart-hotel-booking.git
  cd smart-hotel-booking
```
2. Setup Environment Variables

   - Add firebase-adminsdk.json in the Booking.Api directory
   - Create a .env file in the Booking.Api directory

  `Cloudinary__CloudName=your_cloud_name`

  `Cloudinary__ApiKey=your_api_key`

  `Cloudinary__ApiSecret=your_api_secret`

  `VNP_TMN_CODE=`

  `VNP_HASH_SECRET=`

  `VNP_COMMAND=pay`

  `VNP_CURRCODE=VND`

  `VNP_VERSION=2.1.0`

  `VNP_LOCALE=vn`

  `VNP_BASE_URL=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`

  `VNP_RETURN_URL=https://{your-development-port}/api/payments/handle-payment-response`
  
  `MailSettings__EMAIL=`

  `MailSettings__PASSWORD=`
   
  
3. Run with Docker

   - Build and run the Docker container

```bash
  docker-compose up --build
```

4. Access the API

   - The API will be available at http://localhost:8080/swagger/index.html

## How to Use the Project

### API Endpoints
  - /api/hotels: Manage hotels
  - /api/rooms: Manage rooms
  - /api/reviews: Submit and manage reviews
  - /api/payments: Handle payments

### Swagger Documentation

  - Access Swagger UI at http://localhost:8080/swagger for detailed API documentation

## Troubleshooting

### Docker Build Errors
  - Verify that firebase-adminsdk.json is included in the build context
### Cloudinary Configuration Errors
  - Ensure Cloudinary credentials are correctly set in .env

## License

This project is licensed under the MIT License. See the LICENSE file for details.
