using System.Reflection;
using CleanArchitectureShowcase.Application.Common.Events;
using CleanArchitectureShowcase.Application.Common.Messaging;
using CleanArchitectureShowcase.Domain.Common;
using CleanArchitectureShowcase.Domain.Entities;
using CleanArchitectureShowcase.Infrastructure.Persistence;
using CleanArchitectureShowcase.WebAPI.Controllers;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace CleanArchitectureShowcase.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    private static readonly Assembly _domainAssembly = typeof(Product).Assembly;
    private static readonly Assembly _applicationAssembly = typeof(IDispatcher).Assembly;
    private static readonly Assembly _infrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly _webApiAssembly = typeof(ProductsController).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOnOtherProjectsOrExternalFrameworks()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(_domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "CleanArchitectureShowcase.Application",
                "CleanArchitectureShowcase.Infrastructure",
                "CleanArchitectureShowcase.WebAPI",
                "Microsoft.EntityFrameworkCore",
                "FluentValidation")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain layer violation: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureWebAPIOrEFCore()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(_applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "CleanArchitectureShowcase.Infrastructure",
                "CleanArchitectureShowcase.WebAPI",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application layer violation: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnWebAPI()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(_infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("CleanArchitectureShowcase.WebAPI")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Infrastructure layer violation: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void WebAPIControllers_ShouldNotDependOnInfrastructure()
    {
        NetArchTest.Rules.TestResult result = Types.InAssembly(_webApiAssembly)
            .That()
            .Inherit<ControllerBase>()
            .ShouldNot()
            .HaveDependencyOnAny("CleanArchitectureShowcase.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"WebAPI controller layer violation: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Fact]
    public void Handlers_MustBeSealedAndEndWithHandler()
    {
        var handlerInterfaceDefinitions = new HashSet<Type>
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IDomainEventHandler<>)
        };

        var handlerTypes = _applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetInterfaces().Any(i => i.IsGenericType && handlerInterfaceDefinitions.Contains(i.GetGenericTypeDefinition())))
            .ToList();

        handlerTypes.ShouldNotBeEmpty();

        foreach (Type handlerType in handlerTypes)
        {
            handlerType.IsSealed.ShouldBeTrue($"Handler '{handlerType.Name}' must be sealed.");
            handlerType.Name.EndsWith("Handler", StringComparison.Ordinal)
                .ShouldBeTrue($"Handler '{handlerType.Name}' must end with 'Handler'.");
        }
    }

    [Fact]
    public void ValidatorsAndBehaviors_MustBeSealed()
    {
        var validatorTypes = _applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IValidator).IsAssignableFrom(t))
            .ToList();

        validatorTypes.ShouldNotBeEmpty();
        foreach (Type validatorType in validatorTypes)
        {
            validatorType.IsSealed.ShouldBeTrue($"Validator '{validatorType.Name}' must be sealed.");
        }

        var behaviorTypes = _applicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
            .ToList();

        behaviorTypes.ShouldNotBeEmpty();
        foreach (Type behaviorType in behaviorTypes)
        {
            behaviorType.IsSealed.ShouldBeTrue($"Pipeline behavior '{behaviorType.Name}' must be sealed.");
        }
    }

    [Fact]
    public void Entities_MustNotHavePublicPropertySetters()
    {
        var entityTypes = _domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.BaseType is { IsGenericType: true } &&
                        t.BaseType.GetGenericTypeDefinition() == typeof(BaseEntity<>))
            .ToList();

        entityTypes.ShouldNotBeEmpty();

        foreach (Type entityType in entityTypes)
        {
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                MethodInfo? setter = property.GetSetMethod(nonPublic: false);
                setter.ShouldBeNull($"Entity '{entityType.Name}' property '{property.Name}' has a public setter. Invariants must be protected by encapsulation.");
            }
        }
    }
}
