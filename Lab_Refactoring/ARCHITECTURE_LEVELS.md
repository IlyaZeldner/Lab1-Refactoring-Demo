# Ilya Zeldner - Architectural Evolution: From Academic Lab to Enterprise

This document breaks down the progression of the discount calculation engine across three distinct engineering tiers: **Academic Lab Refactoring**, **Modern Industry Clean Code**, and **Enterprise Architecture**.

---

## 1. Academic Lab Refactoring (Tactical Clean Code)

### Focus
Code readability, structural hygiene, and eliminating academic code smells within an existing class.

### Key Refactorings Applied
* **Self-Documenting Identifiers:** Replaced obscure legacy names (`Class1`, `Calculate`, `amount`, `years`, `disc`, `result`) with domain-accurate terms (`DiscountManager`, `ApplyDiscount`, `price`, `timeOfHavingAccountInYears`, `priceAfterDiscount`).
* **Strongly-Typed Enumeration:** Replaced magic integer status codes (`1, 2, 3, 4`) with the `AccountStatus` enum.
* **Flow Simplification:** Replaced chained `if-else` blocks with a structured `switch-case`.
* **Centralized Constants:** Extracted hardcoded literals (`0.1m`, `0.3m`, `0.5m`, `5`) into a single-source-of-truth `Constants` class.
* **Critical Bug Fix:** Handled undefined status inputs in the `default:` branch by throwing an exception instead of silently returning `0` (giving items away for free).
* **DRY via Extension Methods:** Encapsulated status and tenure formulas into `PriceExtensions` and unified the loyalty discount call so it executes once after the switch.

### Trade-offs & Limitations
* **Primitive Type Pollution:** Extending `this decimal` pollutes the IntelliSense of every numeric variable across the entire project.
* **No Precondition Guards:** Missing defensive checks against invalid inputs (e.g., negative prices or negative tenure).
* **Open/Closed Principle (OCP) Violation:** Adding a new discount tier requires modifying the existing switch statement.

---

## 2. Modern Industry Standards (Defensive & Idiomatic C#)

### Focus
Robustness, language idioms, fail-fast engineering, and eliminating framework anti-patterns.

### Key Refactorings Applied
* **Defensive Programming (Fail-Fast):** Validates input arguments at the beginning of execution, rejecting negative prices or negative tenure with `ArgumentOutOfRangeException`.
* **Zero Primitive Pollution:** Eliminates public extension methods on `decimal`, encapsulating the calculation logic safely within domain boundaries.
* **Modern Switch Expressions:** Replaces verbose `switch-case` boilerplate with concise, declarative pattern matching (`switch expression`).
* **Deterministic Math:** Replaces nested ternary checks with `Math.Min(years, Constants.MAXIMUM_DISCOUNT_FOR_LOYALTY)` for clean ceiling boundaries.

### Trade-offs & Limitations
* Adding new business tiers still requires code changes within the engine class itself, making it less suitable for systems with frequently changing marketing campaigns.

---

## 3. Enterprise Architecture (Domain-Driven & Extensible Subsystem)

### Focus
Extensibility, regulatory compliance, financial auditability, and testability in large-scale distributed systems.

### Key Refactorings Applied
* **Open/Closed Principle (OCP / Strategy Pattern):** Decoupled discount logic into independent strategies implementing `IDiscountRule` / `ICustomerDiscountStrategy`. New commercial campaigns (e.g., Black Friday, VIP, Employee) can be plugged in via Dependency Injection without modifying existing core logic.
* **Financial Audit Trail:** Replaces a blind decimal return with a rich `PricingCalculationResult` object containing line-by-line deductions (`AppliedDiscount`) required by ERP and accounting systems.
* **Domain Primitives (`Money` Value Object):** Prevents currency mismatches (e.g., subtracting USD from EUR) and enforces standard banking rounding (`MidpointRounding.ToEven`).
* **Testable Time via `TimeProvider`:** Replaces arbitrary integer years with an actual `Customer.RegisteredAt` timestamp, utilizing .NET's `TimeProvider` for fully deterministic unit testing of tenure boundaries across any point in time.

---

## Comparison Matrix

| Aspect | Academic Lab | Modern Industry | Enterprise Architecture |
| :--- | :--- | :--- | :--- |
| **Design Pattern** | Procedural / Extensions | Functional / Declarative | Strategy / Rules Pipeline |
| **New Tier Support** | Modify existing switch | Modify switch expression | Add new strategy class (OCP) |
| **Input Validation** | None | Fail-Fast guards | Domain-level value validation |
| **Output Type** | Single `decimal` | Single `decimal` | `PricingCalculationResult` (Audit breakdown) |
| **Currency Safety** | None (Raw primitive) | None (Raw primitive) | Strong `Money` Value Object |
| **Time Handling** | Raw `int years` | Raw `int years` | `DateTimeOffset` + `TimeProvider` |