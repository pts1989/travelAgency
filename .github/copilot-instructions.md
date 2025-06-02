You are working on the IntelliTrip Planner, a C# (.NET 9+) project that helps users plan trips using a multi-agent system. The main agent is `HeadAgent`, supported by specialized agents like `FlightAgent` and `AccomodationAgent`. These agents communicate with external APIs via the Model Context Protocol (MCP) using a dedicated C# SDK.

Write clean, maintainable, and well-structured C# code that strictly follows the SOLID principles:
- Keep classes and methods focused on a single responsibility (SRP).
- Favor abstractions and extension over modification (OCP).
- Ensure subclasses can fully substitute their base types (LSP).
- Design small, specific interfaces (ISP).
- Use dependency injection via interfaces (DIP).
- use one file per class

Always suggest or include unit tests (written with xUnit) for any business logic or significant method. Aim for 85% or higher code coverage. Use mocking frameworks like Moq or NSubstitute to mock dependencies.

Organize code by feature, not type. For example, place all logic for FlightAgent in a folder like `IntelliTrip.Planner.Agents.Fligh/`, using subfolders such as `Services`, `Models`, `Mappers`, and `Exceptions`. Shared logic belongs in `IntelliTrip.Planner.Core`.

Use proper C# style:
- PascalCase for types and methods, camelCase for variables and parameters.
- Keep methods short and focused.
- Use async/await for I/O-bound work and ConfigureAwait(false) where needed.
- Favor immutability when practical.
- Use LINQ fluently where it improves readability.
- Implement robust error handling, including custom exceptions where useful.

When generating or refactoring code:
- Apply the relevant SOLID principles and explain your reasoning if asked.
- Propose appropriate unit test cases.
- Suggest logical file placement based on the existing structure.
- make logical forlders and files for new classes, services, or features.

Your goal is to help developers maintain a clean, modular, testable, and production-ready codebase.