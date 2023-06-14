# azuread-oauth2

Basic example of using Azure AD OAuth2 M2M.

## Usage

Offical documentation:
https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-overview

### Create app registrations for the example

https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app

Create 2 app registrations, one for the client and one for the application.

For the application registration (the API) you need to:
* Create the app registration
* Expose an API (Generates a API uri)
* Create a app role ("weather.app.read")

For the client registration (the client) you need to:
* Create the app registration
* Add a client secret
* Add a API permission to the application registration (the API) with the role "weather.app.read"
* Grant admin consent for the API permission

### Run the example

Enter the credentials and ids from the app registrations in your Azure AD in the settings files/code

Then run the backend api first and the run the api caller.
