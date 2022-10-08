# Strict Init

Simple Roslyn analyzer which checks if all properties with public setter have been initialized when
creating object with object initializer.

## Quickstart

To start using __Strict Init__ install following packages:

- `Silhan.StrictInit`
  Contains attributes which are used for configuration of analyzers.
- `Silhan.StrictInit.Analyzers`
  Contains analyzers themselves along with code fixes.

After that mark a class which initialization should be checked with `StrictInit` attribute:

```c#
[StrictInit] // This attribute inidicates that all properties in class should be initialized
class MyClass
{
    public string MyProperty { get; set; }
    public string MyOtherProperty { get; set; }
    ...
}
```

now if anybody tries to initialize `MyClass` without initializing all properties, a warning will be issued. 
```
Warning SI002 : Public property MyProperty, MyOtherProperty not set
```

## Configuration

__Strict Init__ differentiates between concepts of _strict_ and _soft_ property, and _strict_ and _soft_ object.

- _Strict_ property is a property which has to be initialized in object initializer. If strict property is not
  initialized in object initializer warning is issued.
- _Soft_ property is a property which does not have to be initialized in object initializer. If soft property is
  not initialized in object initializer __Strict Init__ issues Info message about property not being initialized.
- _Strict_ object is a class or struct which has all properties strict by default. To make class strict mark it with
  `StrictInit` attribute.
- _Soft_ object is a class or struct which has all properties soft by default. All classes which __are not__ marked with 
  `StrictInit` attribute are considered soft.

### Strict Object

This is an example of strict object:

```c#
[StrictInit]
class MyClass
{
    public string Property1 { get; set; }
    public string Property2 { get; set; }
}
```

in this object both `Property1` and `Property2` have to be set in object initializer.

### Strict object with soft property

This is an example of strict object, which has one of it's properties set as soft:

```c#
[StrictInit]
class MyClass
{
    public string Property1 { get; set; }
    [SoftInit] public string Property2 { get; set; }
}
```

in this object only `Property1` has to be set in object initializer.

### Soft object with strict property

This is an example of soft object, which has one of it's properties set as strict:

```c#
class MyClass
{
    public string Property1 { get; set; }
    [StrictInit] public string Property2 { get; set; }
}
```

in this object only `Property2` has to be set in object initializer.