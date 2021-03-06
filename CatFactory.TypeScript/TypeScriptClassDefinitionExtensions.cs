﻿using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.TypeScript
{
    public static class TypeScriptClassDefinitionExtensions
    {
        public static void AddConstant(this TypeScriptClassDefinition classDefinition, string type, string name, string value)
            => classDefinition.Fields.Add(new FieldDefinition(type, name)
            {
                IsStatic = true,
                IsReadOnly = true,
                Value = value
            });

        public static TypeScriptClassDefinition RefactClass(this object obj, string name = null, ICodeNamingConvention namingConvention = null)
        {
            var sourceType = obj.GetType();

            var classDefinition = new TypeScriptClassDefinition
            {
                Name = string.IsNullOrEmpty(name) ? sourceType.Name : name
            };

            if (namingConvention == null)
                namingConvention = new TypeScriptNamingConvention();

            foreach (var property in sourceType.GetProperties().Where(item => item.CanRead))
            {
                classDefinition.Fields.Add(new FieldDefinition(TypeScriptTypeResolver.Resolve(property.PropertyType.Name), namingConvention.GetFieldName(property.Name)));

                classDefinition.Properties.Add(new PropertyDefinition(TypeScriptTypeResolver.Resolve(property.PropertyType.Name), namingConvention.GetPropertyName(property.Name)));
            }

            return classDefinition;
        }

        public static TypeScriptInterfaceDefinition RefactInterface(this TypeScriptClassDefinition classDefinition, ICodeNamingConvention namingConvention = null, params string[] exclusions)
        {
            var interfaceDefinition = new TypeScriptInterfaceDefinition();

            if (namingConvention == null)
                namingConvention = new TypeScriptNamingConvention();

            interfaceDefinition.Name = namingConvention.GetInterfaceName(classDefinition.Name);

            interfaceDefinition.Namespaces = classDefinition.Namespaces;

            foreach (var @event in classDefinition.Events.Where(item => item.AccessModifier == AccessModifier.Public && !exclusions.Contains(item.Name)))
                interfaceDefinition.Events.Add(new EventDefinition(@event.Type, @event.Name));

            foreach (var property in classDefinition.Properties.Where(item => item.AccessModifier == AccessModifier.Public && !exclusions.Contains(item.Name)))
            {
                interfaceDefinition.Properties.Add(new PropertyDefinition(property.Type, property.Name)
                {
                    IsAutomatic = property.IsAutomatic,
                    IsReadOnly = property.IsReadOnly
                });
            }

            foreach (var method in classDefinition.Methods.Where(item => item.AccessModifier == AccessModifier.Public && !exclusions.Contains(item.Name)))
                interfaceDefinition.Methods.Add(new MethodDefinition(method.Type, method.Name, method.Parameters.ToArray()));

            return interfaceDefinition;
        }
    }
}
