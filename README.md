# Lab 1: Refactoring & Architecture Evolution

This project demonstrates the step-by-step refactoring of a commercial discount calculation engine for the **Introduction to Software Testing** course.

## Architecture Stages in `Program.cs`

1. **`Lab_Refactoring.Legacy`**: Original legacy implementation (`Class1.Calculate`) containing primitive obsession, magic numbers, and the unhandled account status bug.
2. **`Lab_Refactoring.LabSolution`**: Academic solution implementing all 8 lab steps (meaningful names, `AccountStatus` enum, centralized constants, extension methods, and consolidated tenure discount).
3. **`Lab_Refactoring.ModernIndustry`**: Modern C# clean implementation featuring defensive validation (fail-fast on negative inputs), pattern-matching switch expressions, and zero primitive type pollution.
4. **`Lab_Refactoring.EnterpriseStrategy`**: Domain-Driven Design (DDD) with `ICustomerDiscountStrategy` adhering to the Open/Closed Principle (OCP).

## Verification Harness Output
Run the project (`Ctrl + F5`) to see all four architectures execute the same equivalence partitions and boundary test cases side-by-side with zero regressions.
