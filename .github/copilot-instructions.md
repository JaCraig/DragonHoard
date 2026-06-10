# Copilot Instructions

## General C# Guidance

- Follow the repository's existing C# conventions first, then common C# conventions when the repo is silent.
- Check the local `.editorconfig` file before making style assumptions.
- Keep naming consistent with surrounding code, even when it differs from default .NET style guidance.
- Use `var` for local variables when the type is obvious from the right-hand side.
- Use explicit types for public properties and method parameters.
- Use `readonly` for fields that do not change after initialization.
- Use `const` for compile-time constants and `static` for utility members that do not need instance state.
- Prefer `async`/`await` for asynchronous work and avoid blocking calls like `.Result`, `.Wait()`, or `GetAwaiter().GetResult()`.
- Accept and propagate `CancellationToken` on cancellable operations.
- Keep nullable reference types enabled and resolve nullability warnings instead of suppressing them.
- Validate public inputs with clear guard clauses and throw specific exceptions such as `ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException` when appropriate.
- Use `using` or `await using` for disposable resources and dispose timers, streams, and subscriptions deterministically.
- Prefer readable LINQ and avoid multiple enumeration when cost or side effects matter.
- Use xUnit for tests and NSubstitute for mocking unless the repository already uses a different established pattern.
- Prefer `dotnet format` when a formatting pass is needed.

## Commit Messages

- Use [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) message format. Format: `<type>(<scope>): <description>`
- Include scope in the title.
- The subject should be in present tense describing the overall changes as a completed action.
- The next paragraph should be an overall description of the changes.
- Subsequent lines should be a bulleted list of the changes.

Example 1:

```
feat(services): add new user registration endpoint

Added a new API endpoint for user registration that accepts email and password, creates a new user in the database, and returns a JWT token.

- Created `RegisterUserCommand` and corresponding handler in `GreenField.Services`
- Added `POST /api/register` endpoint in `API/Controllers/AuthController.cs`
- Updated database schema with new `Users` table via Inflatable mapping
```

Example 2:

```
fix(frontend): resolve issue with theme variable loading

Fixed a bug where theme CSS variables were not loading correctly on initial page load, causing the default theme to be applied instead of the user's selected theme.

- Updated `ThemeService` to ensure CSS variables are applied before the first render
- Added unit tests for `ThemeService` to verify correct variable application
```
