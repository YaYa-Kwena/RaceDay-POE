| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/register` | Registers a new Organiser or Participant. | None | `{ firstName, lastName, email, password, role, gender, dateOfBirth, phoneNumber }` | 201 Created; 400 Bad Request; 409 Conflict |
| **POST** | `/api/auth/login` | Authenticates a user and returns an access token. | None | `{ email, password }` | 200 OK; 401 Unauthorized |
| **GET** | `/api/profiles/me` | Retrieves the logged-in user's profile details. | Any | None | 200 OK; 401 Unauthorized |
| **PUT** | `/api/profiles/me` | Updates the logged-in user's profile information. | Any | `{ phoneNumber, emergencyContactName, emergencyContactPhone, medicalConditions }` | 200 OK; 400 Bad Request; 401 Unauthorized |