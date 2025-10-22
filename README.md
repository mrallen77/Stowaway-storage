StowawayStorage/
├── Controllers/
│   ├── HomeController.cs
│   ├── UnitsController.cs
│   ├── ReservationsController.cs
│   └── AdminController.cs
├── Models/
│   ├── StorageUnit.cs
│   ├── Reservation.cs
│   └── ApplicationUser.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Views/
│   ├── Shared/
│   ├── Units/
│   ├── Reservations/
│   └── Admin/
├── wwwroot/
│   └── css, js, images
└── README.md


✅ User Registration & Login — Built-in Identity authentication
✅ Unit Management — CRUD for unit size, price, and availability
✅ Reservation System — Users can book units; prevents overlapping dates
✅ Admin Dashboard — Manage users, view bookings, adjust rates
✅ My Reservations Page — Track active and past bookings
✅ Responsive UI — Mobile-friendly Razor + Bootstrap interface
✅ Database Seeding — Auto-create demo units and admin account
✅ API Calls to USPS to calculate shipping rates

🧰 Seed Data

Default admin:
Email: admin@stowaway.com

Password: Pass123!

Demo units: 5–10 pre-created with random sizes and prices.
