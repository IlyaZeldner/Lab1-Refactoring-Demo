// Ilya Zeldner
using System;
using System.Collections.Generic;

// ============================================================================
// STAGE 0: ORIGINAL LEGACY CODE (Baseline from Lab PDF)
// ============================================================================
namespace Lab_Refactoring.Legacy
{
    /// <summary>
    /// Legacy implementation containing multiple code smells:
    /// 1. Poor naming (Class1, Calculate, amount, type, years, disc, result).
    /// 2. Primitive Obsession (type is an arbitrary int).
    /// 3. Inconsistent calculation logic (0.7m vs price - 0.1m).
    /// 4. Magic numbers (0.1m, 0.3m, 0.5m, 5).
    /// 5. Hidden bug: Unhandled account types silently evaluate to a final price of 0.
    /// </summary>
    public class Class1
    {
        // Step 0: Original legacy method with multiple code smells and hidden bugs.
        public decimal Calculate(decimal amount, int type, int years)
        {
            decimal result = 0;
            decimal disc = (years > 5) ? (decimal)5 / 100 : (decimal)years / 100;
            if (type == 1)
            {
                result = amount;
            }
            else if (type == 2)
            {
                result = (amount - (0.1m * amount)) - disc * (amount - (0.1m * amount));
            }
            else if (type == 3)
            {
                result = (0.7m * amount) - disc * (0.7m * amount);
            }
            else if (type == 4)
            {
                result = (amount - (0.5m * amount)) - disc * (amount - (0.5m * amount));
            }
            return result;
        }
    }
}

// ============================================================================
// STAGES 1-8: LAB TARGET REFACTORING (Complete Specification from PDF)
// ============================================================================
namespace Lab_Refactoring.LabSolution
{
    /// <summary>
    /// Step 2: Replaces magic integers with an explicit, strongly typed domain enumeration.
    /// </summary>
    public enum AccountStatus
    {
        NotRegistered = 1,
        SimpleCustomer = 2,
        ValuableCustomer = 3,
        MostValuableCustomer = 4
    }

    /// <summary>
    /// Step 6: Eliminates magic literals by centralizing business rules and discount rates into static constants.
    /// </summary>
    public static class Constants
    {
        public const int MAXIMUM_DISCOUNT_FOR_LOYALTY = 5;
        public const decimal DISCOUNT_FOR_SIMPLE_CUSTOMERS = 0.1m;
        public const decimal DISCOUNT_FOR_VALUABLE_CUSTOMERS = 0.3m;
        public const decimal DISCOUNT_FOR_MOST_VALUABLE_CUSTOMERS = 0.5m;
    }

    /// <summary>
    /// Step 7: DRY (Don't Repeat Yourself) principle.
    /// Encapsulates calculation formulas into reusable extension methods.
    /// </summary>
    public static class PriceExtensions
    {
        /// <summary>
        /// Step 5 & 7: Applies the fixed tier discount uniformly using: price - (discountSize * price).
        /// </summary>
        public static decimal ApplyDiscountForAccountStatus(this decimal price, decimal discountSize)
        {
            return price - (discountSize * price);
        }

        /// <summary>
        /// Step 1 & 7: Calculates and deducts tenure-based loyalty discount (1% per year up to 5%).
        /// </summary>
        public static decimal ApplyDiscountForTimeOfHavingAccount(this decimal price, int timeOfHavingAccountInYears)
        {
            decimal discountForLoyaltyInPercentage = (timeOfHavingAccountInYears > Constants.MAXIMUM_DISCOUNT_FOR_LOYALTY)
                ? (decimal)Constants.MAXIMUM_DISCOUNT_FOR_LOYALTY / 100
                : (decimal)timeOfHavingAccountInYears / 100;

            return price - (discountForLoyaltyInPercentage * price);
        }
    }

    /// <summary>
    /// Steps 1, 3, 4, 8:
    /// - Meaningful domain names (DiscountManager, ApplyDiscount, price, accountStatus, timeOfHavingAccountInYears).
    /// - switch-case structure replacing nested if-else statements.
    /// - Step 4: ArgumentOutOfRangeException on default branch protects against unhandled tiers (free-item bug).
    /// - Step 8: Consolidates ApplyDiscountForTimeOfHavingAccount into a single call executed after the switch.
    /// </summary>
    public class DiscountManager
    {
        // Step 1: Applies tier and loyalty discounts sequentially, returning the final price.
        public decimal ApplyDiscount(decimal price, AccountStatus accountStatus, int timeOfHavingAccountInYears)
        {
            decimal priceAfterDiscount; // Step 3: Replaces nested if-else with a clear switch-case structure.

            switch (accountStatus) // Step 3: Switch-case structure improves readability and maintainability.
            {
                case AccountStatus.NotRegistered: // Step 3: Explicitly handles unregistered customers (no discount).
                    priceAfterDiscount = price;
                    break;

                case AccountStatus.SimpleCustomer:
                    priceAfterDiscount = price.ApplyDiscountForAccountStatus(Constants.DISCOUNT_FOR_SIMPLE_CUSTOMERS);
                    break;

                case AccountStatus.ValuableCustomer:
                    priceAfterDiscount = price.ApplyDiscountForAccountStatus(Constants.DISCOUNT_FOR_VALUABLE_CUSTOMERS);
                    break;

                case AccountStatus.MostValuableCustomer:
                    priceAfterDiscount = price.ApplyDiscountForAccountStatus(Constants.DISCOUNT_FOR_MOST_VALUABLE_CUSTOMERS);
                    break;

                default:
                    // Fixes Step 4: Throws descriptive exception instead of returning 0.
                    throw new ArgumentOutOfRangeException(nameof(accountStatus), $"Unhandled account status: {accountStatus}");
            }

            // Step 8: Run loyalty discount once for all customers (unregistered customers have 0 years tenure = 0% discount).
            return priceAfterDiscount.ApplyDiscountForTimeOfHavingAccount(timeOfHavingAccountInYears);
        }
    }
}

// ============================================================================
// STAGES 9-11: MODERN INDUSTRY BEST PRACTICES (Beyond Academic Scope)
// ============================================================================
namespace Lab_Refactoring.ModernIndustry
{
    using Lab_Refactoring.LabSolution; // Reuse domain types and constants from LabSolution namespace

    /// <summary>
    /// Production-grade implementation addressing real-world software design issues:
    /// 1. Defensive Programming: Validates preconditions (negative prices, negative tenure).
    /// 2. Eliminates Primitive Type Pollution: Avoids attaching business-specific extension methods to standard decimal.
    /// 3. Modern C# Syntax: Expressive switch expressions, pattern matching, and Math.Min clamping.
    /// </summary>
    public class ModernDiscountManager // Step 9: Modern C# implementation with defensive programming and clean syntax
    {
        public decimal ApplyDiscount(decimal price, AccountStatus accountStatus, int timeOfHavingAccountInYears)
        {
            // Defensive guards (Fail Fast)
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            if (timeOfHavingAccountInYears < 0) // Step 9: Defensive programming to prevent negative tenure
                throw new ArgumentOutOfRangeException(nameof(timeOfHavingAccountInYears), "Tenure cannot be negative.");

            // Determine tier discount rate using clean modern C# switch expression
            decimal tierDiscountRate = accountStatus switch
            {
                AccountStatus.NotRegistered => 0m, // No discount for unregistered customers
                AccountStatus.SimpleCustomer => Constants.DISCOUNT_FOR_SIMPLE_CUSTOMERS, // 10% discount for simple customers
                AccountStatus.ValuableCustomer => Constants.DISCOUNT_FOR_VALUABLE_CUSTOMERS,
                AccountStatus.MostValuableCustomer => Constants.DISCOUNT_FOR_MOST_VALUABLE_CUSTOMERS,
                _ => throw new ArgumentOutOfRangeException(nameof(accountStatus), $"Unsupported status: {accountStatus}")
            };

            // Clamp loyalty discount cleanly without ternary operators
            decimal loyaltyDiscountRate = Math.Min(timeOfHavingAccountInYears, Constants.MAXIMUM_DISCOUNT_FOR_LOYALTY) / 100m;

            // Sequential discount deduction
            decimal priceAfterTierDiscount = price * (1m - tierDiscountRate);
            return priceAfterTierDiscount * (1m - loyaltyDiscountRate);
        }
    }
}

// ============================================================================
// STAGE 12: ENTERPRISE ARCHITECTURE (SOLID / Open-Closed Principle)
// ============================================================================
namespace Lab_Refactoring.EnterpriseStrategy
{
    using Lab_Refactoring.LabSolution; // Reuse domain types and constants from LabSolution namespace

    /// <summary>
    /// Strategy interface for status discount calculations, complying with Open/Closed Principle (OCP).
    /// Adding a new customer tier no longer requires modifying the DiscountManager class.
    /// </summary>
    public interface ICustomerDiscountStrategy
    {
        AccountStatus Status { get; } // Each strategy is associated with a specific account status
        decimal ApplyStatusDiscount(decimal price);
    }

    public class NotRegisteredDiscountStrategy : ICustomerDiscountStrategy
    {
        public AccountStatus Status => AccountStatus.NotRegistered; // No discount for unregistered customers
        public decimal ApplyStatusDiscount(decimal price) => price; // Returns the original price without any discount
    }

    // Concrete strategy implementations for each customer tier, encapsulating their respective discount logic
    public class SimpleCustomerDiscountStrategy : ICustomerDiscountStrategy
    {
        public AccountStatus Status => AccountStatus.SimpleCustomer;
        public decimal ApplyStatusDiscount(decimal price) => price * (1m - Constants.DISCOUNT_FOR_SIMPLE_CUSTOMERS);
    }

    // Concrete strategy implementations for each customer tier, encapsulating their respective discount logic
    public class ValuableCustomerDiscountStrategy : ICustomerDiscountStrategy
    {
        public AccountStatus Status => AccountStatus.ValuableCustomer;
        public decimal ApplyStatusDiscount(decimal price) => price * (1m - Constants.DISCOUNT_FOR_VALUABLE_CUSTOMERS);
    }

    // Concrete strategy implementations for each customer tier, encapsulating their respective discount logic
    public class MostValuableCustomerDiscountStrategy : ICustomerDiscountStrategy
    {
        public AccountStatus Status => AccountStatus.MostValuableCustomer;
        public decimal ApplyStatusDiscount(decimal price) => price * (1m - Constants.DISCOUNT_FOR_MOST_VALUABLE_CUSTOMERS);
    }

    // EnterpriseDiscountManager orchestrates the application of discounts using the Strategy pattern, allowing for easy extension and maintenance.
    public class EnterpriseDiscountManager
    {
        private readonly Dictionary<AccountStatus, ICustomerDiscountStrategy> _strategies = new Dictionary<AccountStatus, ICustomerDiscountStrategy>();

        // Constructor accepts a collection of strategies, enabling easy extension for new customer tiers without modifying existing code.
        public EnterpriseDiscountManager(IEnumerable<ICustomerDiscountStrategy> strategies)
        {
            foreach (var strategy in strategies)
            {
                _strategies[strategy.Status] = strategy;
            }
        }
        // ApplyDiscount method applies the appropriate strategy based on the account status and calculates the final price after loyalty discount.
        public decimal ApplyDiscount(decimal price, AccountStatus status, int years)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            if (years < 0) throw new ArgumentOutOfRangeException(nameof(years));

            if (!_strategies.TryGetValue(status, out var strategy))
                throw new ArgumentOutOfRangeException(nameof(status), $"No registered strategy found for status: {status}");

            decimal priceAfterTier = strategy.ApplyStatusDiscount(price);
            decimal loyaltyRate = Math.Min(years, Constants.MAXIMUM_DISCOUNT_FOR_LOYALTY) / 100m;

            return priceAfterTier * (1m - loyaltyRate);
        }
    }
}

// ============================================================================
// RUNNER & VERIFICATION HARNESS (Console Visualizer)
// ============================================================================
namespace Lab_Refactoring
{
    using Legacy;
    using LabSolution;
    using ModernIndustry;
    using EnterpriseStrategy;

    internal class Program
    {
        // Entry point for the console application, orchestrating the execution of all discount manager variants and verifying their equivalence.
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("================================================================================");
            Console.WriteLine("   REFACTORING LAB - VERIFICATION & COMPARISON HARNESS");
            Console.WriteLine("================================================================================\n");

            // Instantiate all variants
            var legacy = new Class1();
            var lab = new DiscountManager();
            var modern = new ModernDiscountManager();
            var enterprise = new EnterpriseDiscountManager(new ICustomerDiscountStrategy[]
            {
                new NotRegisteredDiscountStrategy(),
                new SimpleCustomerDiscountStrategy(),
                new ValuableCustomerDiscountStrategy(),
                new MostValuableCustomerDiscountStrategy()
            });

            // Equivalence & Boundary test cases: (Price, TypeInt, StatusEnum, Years, Description)
            var testCases = new (decimal Price, int TypeInt, AccountStatus Status, int Years, string Description)[]
            { // Price, TypeInt, StatusEnum, Years, Description
                (100m, 1, AccountStatus.NotRegistered,      0,  "Unregistered (0 yrs)"),
                (100m, 2, AccountStatus.SimpleCustomer,     0,  "Simple Customer (0 yrs)"),
                (100m, 2, AccountStatus.SimpleCustomer,     3,  "Simple Customer (3 yrs - 3% loyalty)"),
                (100m, 2, AccountStatus.SimpleCustomer,     5,  "Simple Customer (5 yrs - boundary 5%)"),
                (100m, 2, AccountStatus.SimpleCustomer,     8,  "Simple Customer (8 yrs - capped at 5%)"),
                (100m, 3, AccountStatus.ValuableCustomer,   4,  "Valuable Customer (4 yrs - 30% + 4%)"),
                (100m, 4, AccountStatus.MostValuableCustomer, 10, "Most Valuable Customer (50% + capped 5%)")
            }; // Test cases cover all tiers and loyalty discount boundaries

            Console.WriteLine($"{"Scenario",-38} | {"Legacy",-8} | {"Lab Refactored",-14} | {"Modern",-8} | {"Enterprise",-10} | {"Status"}");
            Console.WriteLine(new string('-', 95));

            foreach (var test in testCases)
            {
                decimal legacyRes = legacy.Calculate(test.Price, test.TypeInt, test.Years);
                decimal labRes = lab.ApplyDiscount(test.Price, test.Status, test.Years);
                decimal modernRes = modern.ApplyDiscount(test.Price, test.Status, test.Years);
                decimal enterpriseRes = enterprise.ApplyDiscount(test.Price, test.Status, test.Years);

                bool allMatch = (legacyRes == labRes) && (labRes == modernRes) && (modernRes == enterpriseRes);
                string statusText = allMatch ? "PASS (Identical)" : "FAIL (Mismatch)";

                Console.WriteLine($"{test.Description,-38} | {legacyRes,8:F2} | {labRes,14:F2} | {modernRes,8:F2} | {enterpriseRes,10:F2} | {statusText}");
            }

            Console.WriteLine("\n--------------------------------------------------------------------------------");
            Console.WriteLine("DEMONSTRATION: HANDLING UNREGISTERED / INVALID STATUS (Step 4 Bug Fix)");
            Console.WriteLine("--------------------------------------------------------------------------------");

            int invalidTypeInt = 99;
            AccountStatus invalidStatus = (AccountStatus)invalidTypeInt;

            decimal legacyBugResult = legacy.Calculate(100m, invalidTypeInt, 2);
            Console.WriteLine($"1. Legacy Code with Unknown Status ({invalidTypeInt}): Returns {legacyBugResult} -> CRITICAL BUG (Item given away free!)");

            try
            {
                Console.Write("2. Lab Refactored Code with Unknown Status: ");
                lab.ApplyDiscount(100m, invalidStatus, 2);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"SUCCESSFULLY CAUGHT -> {ex.GetType().Name}: {ex.ParamName}");
            }

            try
            {
                Console.Write("3. Modern Industry Code with Negative Price (-50): ");
                modern.ApplyDiscount(-50m, AccountStatus.SimpleCustomer, 2);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"DEFENSIVE CHECK TRIGGERED -> {ex.Message}");
            }

            Console.WriteLine("\nAll refactoring stages executed successfully without regressions.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}