# Case 3 - Web API

## Issues found
- The route `[Route("v1/users/")]` does not clearly expose `id` in the path (`GET /v1/users/{id}`), making the contract ambiguous.
- The method does not validate invalid input (`id <= 0`), so it may process invalid requests without returning `400 BadRequest`.
- There is no explicit handling for user not found to return `404 NotFound`.
- Direct instantiation with `new UserService()` couples the controller to a concrete implementation and makes testing harder.
- There is no async/cancellation pattern for I/O operations (expected in a more robust modern API version).

## Suggested fix
- Adjust route to `GET /v1/users/{id}` and keep signature compatible when using the test-style version.
- Validate `id` at method start (`id > 0`), returning `BadRequest` when invalid.
- Return `NotFound` when the service cannot find the user.
- Return `Ok(user)` on success (`200`).
- Split into two approaches:
  - Test-adherent version: classic Web API with `IHttpActionResult`, `[FromUri]`, and `new UserService()`.
  - Best-practices version: ASP.NET Core with DI (`IUserService`), async, `CancellationToken`, and `ActionResult<UserDto>`.

## Objective conclusion
The original endpoint has route contract, validation, and HTTP status handling flaws. The minimum fix is to adjust route/validation/returns while preserving the requested legacy style. The complete fix applies modern best practices (DI, async, and typed response) without breaking compatibility.
