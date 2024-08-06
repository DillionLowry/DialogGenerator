using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rhizine.DialogGenerator
{
    public class DialogCommandGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
            {
                return;
            }


            foreach (var property in receiver.Properties ?? Enumerable.Empty<PropertyDeclarationSyntax>())
            {
                if (property == null) continue;

                var model = context.Compilation.GetSemanticModel(property.SyntaxTree);
                if (model == null) continue;

                if (!(model.GetDeclaredSymbol(property) is IPropertySymbol symbol)) continue;

                var attribute = symbol.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.Name == "ShowDialogResultAttribute");

                if (attribute == null) continue;

                try
                {
                    var dialogType = GetAttributeValue(attribute, "Type", "DialogType.Bubble");
                    var successMessage = GetAttributeValue(attribute, "SuccessMessage", "Success!");
                    var failureMessage = GetAttributeValue(attribute, "FailureMessage", "Failed!");
                    var duration = GetAttributeValue(attribute, "Duration", "2000");
                    var position = GetAttributeValue(attribute, "Position", "Center");
                    var theme = GetAttributeValue(attribute, "Theme", "Default");

                    var source = GenerateDecoratedProperty(property, dialogType, successMessage, failureMessage, duration, position, theme);

                    if (!string.IsNullOrWhiteSpace(source))
                    {
                        var fileName = $"{symbol.ContainingType?.Name ?? "Unknown"}_{symbol.Name}_Decorated.cs";
                        context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
                    }
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "DG001",
                            "Error generating dialog command",
                            "An error occurred while generating dialog command for {0}: {1}",
                            "DialogGenerator",
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true),
                        property.GetLocation(),
                        symbol.Name,
                        ex.Message));
                }
            }
        }

        private string GetAttributeValue(AttributeData attribute, string key, string defaultValue)
        {
            var namedArgument = attribute.NamedArguments.FirstOrDefault(kvp => kvp.Key == key);
            return namedArgument.Value.Value?.ToString() ?? defaultValue;
        }

        private string GenerateDecoratedProperty(PropertyDeclarationSyntax property, string dialogType, string successMessage, string failureMessage, string duration, string position, string theme)
        {
            if (property == null)
            {
                return string.Empty;
            }

            string className = "Unknown";
            if (property.Parent is ClassDeclarationSyntax classDeclaration)
            {
                className = classDeclaration.Identifier.Text;
            }
            else if (property.Parent?.Parent is ClassDeclarationSyntax parentClassDeclaration)
            {
                className = parentClassDeclaration.Identifier.Text;
            }

            var propertyName = property.Identifier.Text;
            var propertyType = property.Type?.ToString() ?? "object";

            className = SanitizeIdentifier(className);
            propertyName = SanitizeIdentifier(propertyName);
            propertyType = SanitizeIdentifier(propertyType);

            dialogType ??= "DialogType.Bubble";
            successMessage ??= "Success!";
            failureMessage ??= "Failed!";
            duration ??= "2000";
            position ??= "Center";
            theme ??= "Default";

            return $@"
using System.Windows;

partial class {className}
{{
    private {propertyType} _{propertyName}Decorated;
    public {propertyType} {propertyName}
    {{
        get
        {{
            if (_{propertyName}Decorated == null)
            {{
                var originalCommand = base.{propertyName};
                _{propertyName}Decorated = new DialogCommandDecorator(originalCommand, DialogService, 
                    new DialogOptions
                    {{
                        Type = {dialogType},
                        SuccessMessage = ""{successMessage}"",
                        FailureMessage = ""{failureMessage}"",
                        Duration = {duration},
                        Position = ""{position}"",
                        Theme = ""{theme}""
                    }});
            }}
            return _{propertyName}Decorated;
        }}
    }}
}}";
        }
        private string SanitizeIdentifier(string identifier)
        {
            // Remove any characters that are not valid in C# identifiers
            return new string(identifier.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        private sealed class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<PropertyDeclarationSyntax> Properties { get; } = new List<PropertyDeclarationSyntax>();

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is PropertyDeclarationSyntax propertyDeclaration &&
                    propertyDeclaration.AttributeLists.Count > 0 &&
                    propertyDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == "ShowDialogResult")))
                {
                    Properties.Add(propertyDeclaration);
                }
            }
        }
    }
}
