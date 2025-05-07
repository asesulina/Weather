# Weather API Tests

This repository contains automated tests for the **Weather API**. It is built using **NUnit** for testing and **ExtentReports** for generating HTML reports.

## Prerequisites

Before running the tests, make sure you have the following:

- .NET 8.0 SDK (or higher) installed on your machine.
- NUnit test runner (can be installed via Visual Studio or .NET CLI).
- **API Key** for OpenWeather API.

## Optional Test Data setup

- The current `city.list.json` file located at `{YourBaseFolder}\Weather.API.Test\TestData\city.list.json` contains a small subset of cities for testing purposes.
- For **more comprehensive test coverage**, you can optionally download the **full city list** from OpenWeather's bulk data repository. The complete list is available in a compressed `.gz` format at [OpenWeather Bulk Data](https://bulk.openweathermap.org/sample/).
- After downloading, extract the `city.list.json` file from the `.gz` archive, and place it in the `..\Weather.API.Test\TestData` folder. Replacing the current subset with the full list will allow for better test coverage, as it includes a larger variety of cities.

## Steps to Run the Tests

### 1. Clone the Repository
- Clone this repository to your local machine

### 2. Set ApiKey value
- Set ApiKey value in chosen .runSettings file

### 3. Run The Tests
- Open a terminal/command prompt in the root directory of the project.
- Execute the tests using the dotnet test command
`dotnet test --settings TestSettings/your-selected-settings.runsettings`

### 4. Review Test Report
- The test report will be generated in the root folder of the solution.
- The report will be named 'TestReport.html'.
- Open the 'TestReport.html' file in a web browser to review the test results.
