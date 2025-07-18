---
uid: sample-comparison
---
# Implementing Equality Comparison Without Boilerplate

As a .NET developer, you are likely well-acquainted with the `Equals` method in .NET, which is employed to ascertain if two instances are equivalent. In conjunction with `Equals`, there is the `GetHashCode()` method, which generates a hash code typically utilized for dictionary keys.

## Default Implementation

In .NET, the default implementation of equality comparison differs based on the type: `class`, `struct`, and `record` each adopt their own strategy.

- **`class`**: Two class instances _never_ deemed equal by default. When you compare two `class` instances, only the _references_ are compared, which is equivalent to calling the `ReferenceEqual` method.

- **`struct`**: The default .NET implementation evaluates the values of all fields and automatic properties, invoking the `Equals(object)` method for both `class` and `struct` fields. However, `GetHashCode()` is invoked for classes but not invariably for structs. This behavior can lead to inconsistencies between `Equals` and `GetHashCode` in the edge case where a struct `A` has a field of type `B`,  `B` is a `struct` with a custom equality implementation, and not all fields of `B` are identity members. 

- **`record`**: All fields and automatic properties, whether of `class` or `struct` types, are compared using `EqualityComparer<T>.Default`, where `T` is the field type. This implies that `record` types perform a deep comparison by default. The strongly-typed `Equals` method and the custom `GetHashCode` method are employed in both cases.

## Advantages of a Custom Implementation

While the default .NET implementation is generally reasonable and suffices in most scenarios, there are cases where overriding it by implementing the `IEquatable<T>` interface is advantageous.

- **For `struct` types**:
    - **Performance.** A substantial performance enhancement can be achieved by providing a custom equality implementation, often by two orders of magnitude. If you frequently compare custom structs, a custom equality implementation is essential.
    - **Fixing edge cases.** As mentioned above, there is a slight chance you might be impacted with the edge case mentioned above.
    - **Different string comparison** The default comparison mode for strings is `StringComparison.Ordinal`. If you need a different mode, you will need to supply a custom equality comparison implementation.

- **For `class` types**: 
    - **Different equality behaviors.** Altering the default behavior of equality comparison might be desirable for some families of objects. For instance, if you have an `Entity` class with `EntityType` and `EntityId` fields, alongside other data fields, you might prefer the default comparison to take only these two fields into account, disregarding others. This implies that two distinct instances that share the same `EntityType` and `EntityId`, each with different data fields, would be considered equal.

- **For `record` types**:
    -  In general, overriding the default equality implementation should be approached with caution. Indeed, the identity contract is a fundamental feature of `record` types, unlike `class` types, and modifying this behavior contravene the principle of least surprise.
    - **Ignoring irrelevant fields.** A legitimate scenario for overriding might be to disregard an irrelevant record field. For example, you might have an `ObjectId` field used exclusively for debugging, not stored or serialized over the network, and should not influence equality comparison. In such cases, overriding the equality implementation is justified.
    - 

## Why Not Manually Implement the Custom Implementation

Given that the default implementation can be suboptimal, you might contemplate manually implementing the `IEquatable` interface, including the operators.

There are two issues with this approach:

1. It is essentially repetitive, boilerplate code, and incurs time and financial costs.
2. The implementation must remain synchronized with the list of fields and automatic properties of the class. If you add a field to a struct, it is easy to overlook updating both the `Equals` and `GetHashCode` methods. This is an unnecessary source of human errors.

To circumvent repetitive work and minimize maintenance errors, it is far more advantageous to implement the equality contract automatically during compilation.

Two technologies can help you implement equality patterns:

- raw Roslyn source generators are suitable because this pattern does not require to modify any hand-written members, only adding new ones. However, Roslyn source generates are low-level APIs and can require a lot of code.
- Metalama aspects are easier to build.


1. Step 1. Basic implementation.
2. Step 2. Inheritance.
3. Step 3. Member-level attributes.
4. Step 4. Customization and optimizatons.