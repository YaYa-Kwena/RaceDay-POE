| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/register` | Registers a new Organiser or Participant. | None | `{ firstName, lastName, email, password, role, gender, dateOfBirth, phoneNumber }` | 201 Created; 400 Bad Request; 409 Conflict |
| **POST** | `/api/auth/login` | Authenticates a user and returns an access token. | None | `{ email, password }` | 200 OK; 401 Unauthorized |
| **GET** | `/api/profiles/me` | Retrieves the logged-in user's profile details. | Any | None | 200 OK; 401 Unauthorized |
| **PUT** | `/api/profiles/me` | Updates the logged-in user's profile information. | Any | `{ phoneNumber, emergencyContactName, emergencyContactPhone, medicalConditions }` | 200 OK; 400 Bad Request; 401 Unauthorized |
| **GET** | `/api/events` | Retrieves a list of all upcoming RaceDay events. | Any | None | 200 OK |
| **GET** | `/api/events/{id}` | Returns the details of a specific event using the event ID. | Any | None | 200 OK; 404 Not Found |
| **POST** | `/api/events` | Creates a new RaceDay event. | Organiser | `{ eventName, description, eventType, eventDate, startTime, locationCity, locationProvince, maxCapacity, registrationDeadline }` | 201 Created; 400 Bad Request; 401 Unauthorized; 403 Forbidden |
| **PUT** | `/api/events/{id}` | Updates an existing event's details. | Organiser | `{ eventName, description, maxCapacity, registrationDeadline, isActive }` | 200 OK; 400 Bad Request; 403 Forbidden; 404 Not Found |
| **DELETE** | `/api/events/{id}` | Deletes a specific event. | Organiser | None | 204 No Content; 403 Forbidden; 404 Not Found |