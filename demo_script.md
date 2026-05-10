# SaigonRide - Final Project Demo Script

**Estimated Time:** 15-20 minutes
**Language:** English
**Presenter:** [Your Name / Team Representative]

---

## 1. Introduction (2 mins)

**Presenter:**
"Hello Professor and everyone watching. Welcome to our final project presentation for Software Engineering. Today, our team is proud to introduce **SaigonRide**, a smart bike and e-scooter rental network designed specifically for Ho Chi Minh City.

Our goal was to build a green, convenient, and affordable urban mobility platform. The system is built using ASP.NET Core MVC with Entity Framework Core, backed by a PostgreSQL database hosted on Render for cloud deployment. We also integrated advanced features like interactive Leaflet maps, multi-language support, and the VNPay payment gateway. 

In this video, we will walk you through the entire system, demonstrating both the Customer experience and the Admin management dashboard."

---

## 2. Customer Features (8 mins)

### A. Homepage & Localization
**Presenter:**
"Let's start with the Customer view. Upon landing on the homepage, users are greeted with an interactive map of Ho Chi Minh City showing all available rental stations. 
Notice that we have implemented full bilingual support. By clicking the language toggle in the navigation bar, the entire website—including dynamic data like station status—switches seamlessly between English and Vietnamese."

### B. Authentication & User Profile
**Presenter:**
"To rent a vehicle, a user must be logged in. I will quickly log in with an existing customer account. 
Once logged in, users can access their Profile to update personal information and view their completed rental history."

### C. Renting a Vehicle (Core Workflow)
**Presenter:**
"Now, let's simulate the rental process. A user physically at a station would scan a QR code attached to a vehicle. 
* [Action: Click on a station on the map, select a vehicle, or manually navigate to the Create Rental page]*
Here we see the vehicle details. When I confirm the rental, the system updates the vehicle's state to 'Rented', decreases the station's inventory, and begins tracking the time in the 'Active Trip' view. 
During the trip, the system continuously calculates the elapsed time."

### D. Returning and Payment
**Presenter:**
"When the user arrives at their destination, they select the drop-off station to end the trip. 
The system automatically calculates the total fee based on the vehicle's dynamic Unit Price. It also applies any active discounts—for instance, returning a vehicle to a station with low inventory automatically applies a discount to balance the network load.
Finally, the user proceeds to checkout. We have integrated the **VNPay API** to process real-time digital payments. Once the VNPay transaction is successful, the vehicle is marked as 'Available' again, and the rental is completed."

---

## 3. Admin Dashboard & Management (7 mins)

**Presenter:**
"Now, let me switch to an Administrator account to show you the backend management system."

### A. Dashboard Overview
**Presenter:**
"The Admin Dashboard provides real-time analytics. Here, the administrator can see the total number of users, active rentals, total revenue, and system notifications."

### B. Station Management
**Presenter:**
"Under Station Management, admins can perform CRUD operations to manage the network. The system automatically tags stations with badges like 'Low Stock' or 'Almost Full' based on their current capacity, helping staff know where to redistribute vehicles."

### C. Vehicle Management & QR Generation
**Presenter:**
"In the Vehicle section, we can manage bikes and e-scooters. A key feature here is the automatic generation of QR codes for each vehicle. Admins can print these QR codes and stick them on the physical bikes so users can scan them to rent."

### D. Promotions & User Roles
**Presenter:**
"We also have a Promotions module to manage discount codes. Lastly, the User Management section allows the admin to assign roles—Admin, Staff, or Customer—ensuring strict security and access control across the platform."

---

## 4. Testing & Code Quality (2 mins)

**Presenter:**
"Before concluding, I want to highlight our commitment to software quality. We implemented comprehensive Unit Testing using xUnit and Moq. We applied Boundary Value Analysis and Equivalence Partitioning to test our billing logic, discount calculations, and station inventory management. As you can see from our test reports, all 34 tests pass successfully, ensuring the core business logic is highly reliable."

---

## 5. Conclusion (1 min)

**Presenter:**
"That concludes our demonstration of SaigonRide. From a seamless customer rental flow to a robust admin dashboard and secure payment processing, we believe this platform demonstrates a solid application of software engineering principles. 

Thank you, Professor, for your guidance throughout this course, and thank you all for watching. We are now open to any questions!"
