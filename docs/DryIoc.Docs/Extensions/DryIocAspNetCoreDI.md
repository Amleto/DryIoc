# DryIoc.Microsoft.DependencyInjection

## General information

```
dotnet add package DryIoc.Microsoft.DependencyInjection
```

or the source code package

```
dotnet add package DryIoc.Microsoft.DependencyInjection.src
```


The [package](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection) provides the implementation and the replacement of [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) container with the DryIoc library.

**Ultimately you will get an access to the power and speed of DryIoc in the same time conforming to the MS.DI contract.**

The best way to learn it is to look into the [ASP .NET Core application example](https://github.com/dadhi/DryIoc/tree/master/samples/DryIoc.AspNetCore31.WebApi.Sample) in the DryIoc examples folder. There are [a lot of comments](https://github.com/dadhi/DryIoc/blob/01ee04017efb8e3cea5d666e6d92689fc5f7504e/samples/DryIoc.AspNetCore31.WebApi.Sample/Startup.cs#L28) in the example explaining how to use the DryIoc.MS.DI features.


## Conforming to the rules

To conform to the behavior of Microsoft.DependencyInjection the DryIoc applies a set of rules to the new or the **existing** container 
via `WithMicrosoftDependencyInjectionRules` method.

Those rules include the following:

- adding rule of TrackingDisposableTransients 
- adding rule of SelectLastRegisteredFactory
- adding rule of selecting ConstructorWithResolvableArguments

- removing rule of VariantGenericTypesInResolvedCollection

You may decide to add or remove other rules but be aware that the consumer may be surprised when the conventions are not in place.
