# MomsdeklarationAPI Integration Tests

This project contains integration tests for the MomsdeklarationAPI.

## Test Structure

- **TestWebApplicationFactory**: Custom factory for creating test instances of the API
- **TestAuthHandler**: Test authentication handler that bypasses real authentication
- **Controllers/**: Integration tests for each controller

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Test Coverage

The tests cover:

- ✅ **PingController**: Health check endpoint
- ✅ **UtkastController**: Draft management (create, read, update, delete, get multiple)
- ✅ **BeslutatController**: Decided declarations (get single, get multiple)
- ✅ **InlamnatController**: Submitted declarations (get single, get multiple)
- ✅ **KontrolleraController**: Draft validation
- ✅ **LasController**: Draft locking/unlocking

## Test Patterns

Each controller test suite includes:

1. **Happy path tests**: Verify successful responses with valid data
2. **Validation tests**: Verify error handling for invalid inputs
3. **Content type tests**: Verify proper JSON responses
4. **Status code tests**: Verify correct HTTP status codes

## Mocking

The tests use Moq to mock the `ISkatteverketApiClient` dependency, allowing isolated testing of controller logic without external API calls.
