---
uid: sample-comparison
---

# Implementing equality comparison without boilerplate

You're likely familiar with the `Equals` method in .NET: it checks if two instances are equal. Alongside `Equals`, there's the `GetHashCode()` method, which provides a hash code typically used for dictionary keys.

## Default implementation

In .NET, the default implementation of equality comparison varies based on the type: `class`, `struct`, and `record` each have their own strategy.

For `class` and `struct` types, the CLR provides the default implementation. For `record` types, the C# compiler generates it.

- **`class`**: Two class instances are considered equal only if they are the exact same object, without checking field values. Therefore, two objects of the same class with identical fields will still be considered different by default.

- **`struct`**: The default .NET implementation compares the values of all fields and automatic properties, calling the `Equals(object)` method for both `class` and `struct` fields. However, `GetHashCode()` is called for classes but not always for structs, which can cause inconsistencies between `Equals` and `GetHashCode` in rare cases.

- **`record`**: All fields and automatic properties, whether of `class` or `struct` types, are compared using `EqualityComparer<T>.Default`, where `T` is the field type. This means `record` types perform a deep comparison by default. The strongly-typed `Equals` method and the custom `GetHashCode` method are used in both cases.

## Benefits of a custom implementation

While the default .NET implementation is generally reasonable and works in most situations, there are times you'll want to override it by implementing the `IEquatable<T>` interface.

- **For `struct`**:
    - There's a significant performance boost by providing a custom equality implementation, often by two orders of magnitude. If you frequently compare custom structs, you definitely need a custom equality implementation.
    - If you have a struct `A` with a field of type `B`, where `B` is a `struct` with a custom equality implementation and not all fields of `B` are identity members, the default CLR strategy may not be consistent in this edge case.

- **For `class` types**: Performance isn't a primary reason (comparing object identity is always faster than comparing fields), but you might want to change the behavior. For example, if you have an `Entity` class with `EntityType` and `EntityId` fields, along with other data fields, you might want the default comparison to focus only on these two fields, ignoring others. This means two distinct instances of the same entity, each with different data fields, would be considered equal.

- **For `record` types**: Overriding the default equality implementation should be done with caution. This is because the identity contract is a core feature of `record` types, unlike `class` types, and altering this behavior could lead to unexpected results, violating the rule "don't do anything unexpected." A valid case for overriding might be to ignore an irrelevant record field. For example, you might have an `ObjectId` field used solely for debugging, not stored or serialized over the network, and shouldn't factor into equality comparison. In such cases, overriding the equality implementation makes sense.

## Why not write the custom implementation by hand

Given that the default implementation can be sub-optimal, you might consider manually implementing the `IEquatable` interface, including the operators.

There are two issues with this approach:

1. It's essentially repetitive, boilerplate code, and costs you time and money.
2. The implementation must stay synchronized with the list of fields and automatic properties of the class. If you add a field to a struct, it's easy to overlook updating both the `Equals` and `GetHashCode` methods. This is an unnecessary source of human errors.

To avoid repetitive work and reduce maintenance errors, it's much better to implement the equality contract automatically during compilation.
