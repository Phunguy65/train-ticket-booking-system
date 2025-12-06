# Contributing to Train Ticket Booking System

First off, thank you for considering contributing to the Train Ticket Booking
System! It's people like you that make this project better for everyone. This
document provides guidelines for contributing to the project.

## Table of Contents

*   [Code of Conduct](#code-of-conduct)
*   [What We're Looking For](#what-were-looking-for)
*   [What We're NOT Looking For](#what-were-not-looking-for)
*   [Ground Rules](#ground-rules)
*   [Your First Contribution](#your-first-contribution)
*   [Getting Started](#getting-started)
*   [Development Process](#development-process)
*   [Code Review Process](#code-review-process)
*   [Community](#community)

## Code of Conduct

This project and everyone participating in it is governed by our commitment to
providing a welcoming and inclusive environment. By participating, you are
expected to uphold this standard. Please report unacceptable behavior to the
project maintainers.

### Our Standards

*   **Be respectful**: Treat everyone with respect and consideration
*   **Be collaborative**: Work together and help others
*   **Be patient**: Everyone was a beginner once
*   **Be constructive**: Focus on what is best for the project
*   **Be open**: Welcome newcomers and encourage diverse contributions

## What We're Looking For

We welcome many types of contributions, including:

*   **Bug reports and fixes**: Help us identify and resolve issues
*   **Feature implementations**: Add new functionality to the system
*   **Documentation improvements**: Better docs help everyone
*   **Code quality improvements**: Refactoring, optimization, and cleanup
*   **Test coverage**: Add unit tests and integration tests
*   **Performance enhancements**: Make the system faster and more efficient
*   **Security improvements**: Help us keep the system secure
*   **UI/UX improvements**: Enhance the Windows Forms interfaces
*   **Database optimizations**: Improve queries and schema design
*   **Translation/Localization**: Help make the system accessible to more users

## What We're NOT Looking For

Please **do not** use the issue tracker for:

*   **Support questions**: Use GitHub Discussions or Stack Overflow instead
*   **Security vulnerabilities**: Report these privately to the maintainers
*   **Duplicate issues**: Search existing issues before creating a new one
*   **Off-topic discussions**: Keep discussions relevant to the project

## Ground Rules

### Technical Responsibilities

*   **Cross-platform compatibility**: Ensure changes work on Windows (primary),
  and consider Linux/Mac for backend components
*   **Testing**: Write unit tests for new features and bug fixes
*   **Code quality**: Follow the project's coding standards and best practices
*   **Documentation**: Update relevant documentation for your changes
*   **Small PRs**: Keep pull requests focused on a single feature or fix
*   **Atomic commits**: Make commits logical and well-described
*   **Breaking changes**: Discuss breaking changes in an issue before implementing

### Code Standards

*   **C# Backend (.NET 9)**:
    *   Follow Microsoft C# coding conventions
    *   Use async/await for I/O operations
    *   Enable nullable reference types
    *   Use dependency injection
    *   Document public APIs with XML comments

*   **C# Frontend (WinForms .NET Framework 4.8.1)**:
    *   Follow event-driven patterns
    *   Handle exceptions gracefully with user-friendly messages
    *   Implement proper resource disposal
    *   Use async operations for network calls

*   **Database (SQL Server)**:
    *   Use parameterized queries (Dapper handles this)
    *   Write migrations for schema changes
    *   Optimize queries and add appropriate indexes
    *   Document complex queries

*   **JavaScript/TypeScript (Dev Tools)**:
    *   Follow Biome formatter rules
    *   Use single quotes, no semicolons

### Commit Message Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```txt
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:

*   `feat`: New feature
*   `fix`: Bug fix
*   `docs`: Documentation changes
*   `style`: Code style changes (formatting, etc.)
*   `refactor`: Code refactoring
*   `test`: Adding or updating tests
*   `chore`: Maintenance tasks
*   `perf`: Performance improvements

**Examples**:

```txt
feat(booking): add seat selection with optimistic locking
fix(auth): resolve password hashing issue
docs(api): update TCP protocol documentation
test(booking): add concurrent booking tests
```

## Your First Contribution

Unsure where to begin? Look for issues labeled:

*   `good first issue`: Simple issues perfect for newcomers
*   `help wanted`: Issues that need attention
*   `documentation`: Documentation improvements
*   `bug`: Bug fixes needed

**New to open source?** Check out these resources:

*   [How to Contribute to an Open Source Project on GitHub](https://egghead.io/series/how-to-contribute-to-an-open-source-project-on-github)
*   [First Timers Only](https://www.firsttimersonly.com/)
*   [Make a Pull Request](http://makeapullrequest.com/)

## Getting Started

### Prerequisites

Before you begin, ensure you have:

1. **.NET 9 SDK** - For backend development
2. **.NET Framework 4.8.1 Developer Pack** - For WinForms development
3. **SQL Server 2022** - Database (Docker recommended)
4. **Docker Desktop** - For containerized development
5. **Node.js 18+** and **pnpm 10.23.0** - For development tools
6. **Git** - Version control

See [AGENTS.md](../AGENTS.md) for detailed setup instructions.

### Development Environment Setup

1. **Fork the repository** on GitHub

2. **Clone your fork**:

    ```powershell
    git clone https://github.com/YOUR-USERNAME/train-ticket-booking-system.git
    cd train-ticket-booking-system
    ```

3. **Add upstream remote**:

    ```powershell
    git remote add upstream https://github.com/Phunguy65/train-ticket-booking-system.git
    ```

4. **Install dependencies**:

    ```powershell
    # Install Node.js dev dependencies
    pnpm install

    # Restore .NET dependencies
    dotnet restore train-ticket-booking-system.slnx
    ```

5. **Set up the database**:

    ```powershell
    cd database
    docker-compose up -d
    Start-Sleep -Seconds 30
    docker exec -i ttbs-database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P user -C -i /docker-entrypoint-initdb.d/01__schema.sql
    ```

6. **Build the solution**:

    ```powershell
    dotnet build train-ticket-booking-system.slnx
    ```

7. **Verify everything works**:

    ```powershell
    # Run backend
    cd backend
    dotnet run

    # In another terminal, run tests
    dotnet test
    ```

### Making Changes

1. **Create a feature branch**:

    ```powershell
    git checkout -b feat/your-feature-name
    # or
    git checkout -b fix/issue-number-description
    ```

2. **Make your changes**:
    *   Write code following the project's standards
    *   Add tests for new functionality
    *   Update documentation as needed

3. **Format and lint your code**:

    ```powershell
    # Format C# code
    dotnet format train-ticket-booking-system.slnx

    # Format JS/TS/JSON/YAML/SQL
    pnpm biome check --fix "**/*.{js,ts,jsx,tsx}"
    pnpm prettier --write "**/*.{json,yaml,yml,sql}"
    ```

4. **Run tests**:

    ```powershell
    dotnet test
    ```

5. **Commit your changes**:

    ```powershell
    git add .
    git commit -m "feat: add awesome feature"
    ```

    Note: Pre-commit hooks will automatically format and lint your code.

6. **Keep your branch updated**:

    ```powershell
    git fetch upstream
    git rebase upstream/main
    ```

7. **Push to your fork**:

    ```powershell
    git push origin feat/your-feature-name
    ```

## Development Process

### Pull Request Process

1. **Update documentation**: Ensure README, AGENTS.md, or API.md are updated if
   needed

2. **Add tests**: New features must include tests

3. **Ensure CI passes**: All automated checks must pass

4. **Link related issues**: Reference issues in your PR description using
   `Fixes #123` or `Closes #456`

5. **Describe your changes**: Provide a clear description of:
    *   What changed and why
    *   How to test the changes
    *   Screenshots (for UI changes)
    *   Breaking changes (if any)

6. **Request review**: Tag maintainers or relevant contributors

7. **Address feedback**: Respond to review comments and make requested changes

8. **Squash commits (optional)**: Maintainers may ask you to squash commits
   before merging

### PR Template

```markdown
## Description

Brief description of changes

## Related Issues

Fixes #123

## Type of Change

- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing

- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist

- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No new warnings generated
```

### Small/Obvious Fixes

Small contributions such as:

*   Fixing typos
*   Correcting spelling errors
*   Updating comments
*   Formatting code
*   Adding logging messages

Can be submitted directly without creating an issue first.

## How to Report a Bug

### Security Vulnerabilities

**If you find a security vulnerability, do NOT open an issue.** Email the
maintainers directly instead.

Security issues include:

*   Unauthorized access to data or functionality
*   SQL injection vulnerabilities
*   Authentication/authorization bypasses
*   Denial of service vulnerabilities

### Bug Report Guidelines

When filing a bug report, please include:

1. **Environment details**:
    *   Operating System (Windows version)
    *   .NET version (`dotnet --version`)
    *   SQL Server version
    *   Browser (if applicable)

2. **Steps to reproduce**:
    *   What actions did you take?
    *   What input data was used?

3. **Expected behavior**: What should have happened?

4. **Actual behavior**: What actually happened?

5. **Screenshots/logs**: Include any relevant error messages or screenshots

6. **Additional context**: Any other information that might be helpful

## How to Suggest a Feature

### Feature Request Process

1. **Search existing issues**: Check if the feature has already been requested

2. **Create a new issue**: Use the feature request template

3. **Describe the feature**:
    *   What problem does it solve?
    *   How should it work?
    *   Are there any alternatives you've considered?

4. **Provide use cases**: Give concrete examples of how the feature would be
   used

5. **Be patient**: Maintainers will review and discuss the feature request

### Project Goals

This project aims to:

*   Simulate a realistic train ticket booking system
*   Demonstrate client-server architecture with TCP/Socket communication
*   Handle concurrent bookings with proper synchronization
*   Provide a clean, user-friendly Windows Forms interface
*   Maintain code quality and best practices

Features should align with these goals and the educational purpose of the
project.

## Code Review Process

### For Contributors

*   **Be patient**: Maintainers review PRs regularly but may take time
*   **Be responsive**: Address feedback promptly
*   **Be open**: Accept constructive criticism gracefully
*   **Be thorough**: Test your changes thoroughly before requesting review

### For Reviewers

*   **Be constructive**: Provide helpful, actionable feedback
*   **Be timely**: Review PRs within a reasonable timeframe
*   **Be thorough**: Check code quality, tests, and documentation
*   **Be respectful**: Remember contributors are volunteers

### Review Criteria

PRs are evaluated on:

*   Code quality and maintainability
*   Test coverage and quality
*   Documentation completeness
*   Adherence to project standards
*   Performance impact
*   Security considerations
*   Backward compatibility

### Timeline

*   **Initial review**: Within 1 week of submission
*   **Follow-up reviews**: Within 3-5 days after updates
*   **Merge decision**: After all reviewers approve

## Community

### Communication Channels

*   **GitHub Issues**: Bug reports and feature requests
*   **GitHub Discussions**: General questions and discussions
*   **Pull Requests**: Code contributions and reviews

### Getting Help

If you need help:

1. Check existing documentation (README, AGENTS.md, API.md)
2. Search existing issues and discussions
3. Create a new discussion for questions
4. Ask maintainers for guidance on complex issues

### Recognition

Contributors are recognized through:

*   GitHub contributor graphs
*   Release notes mentioning significant contributions
*   Project documentation acknowledgments

## Coding Conventions

### C# Style Guide

*   **Indentation**: Use tabs (4 spaces)
*   **Naming**:
    *   PascalCase for public members, types, namespaces
    *   camelCase for private fields, parameters, local variables
    *   Prefix private fields with underscore: `_fieldName`
*   **Braces**: Opening brace on new line
*   **Null checks**: Use null-conditional operators (`?.`, `??`)
*   **LINQ**: Prefer LINQ for collection operations
*   **Comments**: Use XML documentation for public APIs

### SQL Style Guide

*   **Keywords**: UPPERCASE (SELECT, FROM, WHERE)
*   **Identifiers**: PascalCase for tables and columns
*   **Indentation**: 2 spaces
*   **Aliases**: Use meaningful aliases
*   **Joins**: Explicit JOIN syntax over WHERE clause joins

### Git Commit Messages

*   Use imperative mood ("Add feature" not "Added feature")
*   First line: 50 characters or less
*   Body: Wrap at 72 characters
*   Reference issues: "Fixes #123" or "Closes #456"
*   Explain "why" not "what" in the body

## Additional Resources

*   [AGENTS.md](../AGENTS.md) - Comprehensive development guide
*   [API.md](../API.md) - TCP protocol and API documentation
*   [README.md](../README.md) - Project overview and quick start
*   [.NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
*   [Conventional Commits](https://www.conventionalcommits.org/)

## License

By contributing, you agree that your contributions will be licensed under the
same license as the project.

---

**Thank you for contributing to Train Ticket Booking System!** 🚄

Your contributions help make this project better for everyone. Whether you're
fixing a bug, adding a feature, or improving documentation, every contribution
matters.
