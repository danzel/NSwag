using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag.CodeGeneration.ClientGenerators.Models;

namespace NSwag.CodeGeneration.ClientGenerators
{
    /// <summary>The client generator base.</summary>
    public abstract class ClientGeneratorBase : GeneratorBase
    {
        /// <summary>Gets or sets a value indicating whether [generate multiple web API clients].</summary>
        public OperationGenerationMode OperationGenerationMode { get; set; }

        internal string GenerateFile<TGenerator>(SwaggerService service, TypeResolverBase<TGenerator> resolver)
            where TGenerator : GeneratorBase
        {
            var operations = GetOperations(service, resolver);
            var clients = string.Empty;

            if (OperationGenerationMode == OperationGenerationMode.MultipleClientsFromPathSegments)
            {
                foreach (var controllerOperations in operations.GroupBy(o => o.MvcControllerName))
                    clients += RenderClientCode(controllerOperations.Key, controllerOperations);
            }
            else
                clients = RenderClientCode(string.Empty, operations);

            return RenderFile(clients)
                .Replace("\r", string.Empty)
                .Replace("\n\n\n\n", "\n\n")
                .Replace("\n\n\n", "\n\n");
        }
        
        internal List<OperationModel> GetOperations<TGenerator>(SwaggerService service, TypeResolverBase<TGenerator> resolver) 
            where TGenerator : GeneratorBase
        {
            service.GenerateOperationIds();

            var operations = service.Paths
                .SelectMany(pair => pair.Value.Select(p => new { Path = pair.Key.Trim('/'), HttpMethod = p.Key, Operation = p.Value }))
                .Select(tuple =>
                {
                    var pathSegments = tuple.Path.Split('/').Where(p => !p.Contains("{")).Reverse().ToArray();

                    var mvcControllerName = pathSegments.Length >= 2 ? pathSegments[1] : "Unknown";
                    var mvcActionName = pathSegments.Length >= 1 ? pathSegments[0] : "Unknown";

                    var operation = tuple.Operation;
                    var responses = operation.Responses.Select(r => new ResponseModel
                    {
                        StatusCode = r.Key,
                        IsSuccess = r.Key == "200",
                        Type = GetType(r.Value.Schema, "Response"),
                        TypeIsDate = GetType(r.Value.Schema, "Response") == "Date"
                    }).ToList();

                    var defaultResponse = responses.SingleOrDefault(r => r.StatusCode == "default");
                    if (defaultResponse != null)
                        responses.Remove(defaultResponse);

                    return new OperationModel
                    {
                        Id = operation.OperationId,

                        Path = tuple.Path, 

                        HttpMethodUpper = ConvertToUpperStartIdentifier(tuple.HttpMethod.ToString()),
                        HttpMethodLower = ConvertToLowerStartIdentifier(tuple.HttpMethod.ToString()),

                        IsGetOrDelete = tuple.HttpMethod == SwaggerOperationMethod.get || tuple.HttpMethod == SwaggerOperationMethod.delete,

                        MvcActionName = mvcActionName,
                        MvcControllerName = mvcControllerName,

                        OperationNameLower =
                            ConvertToLowerStartIdentifier(OperationGenerationMode == OperationGenerationMode.MultipleClientsFromPathSegments
                                ? mvcActionName
                                : operation.OperationId),
                        OperationNameUpper =
                            ConvertToUpperStartIdentifier(OperationGenerationMode == OperationGenerationMode.MultipleClientsFromPathSegments
                                ? mvcActionName
                                : operation.OperationId),

                        ResultType = GetResultType(operation),
                        ExceptionType = GetExceptionType(operation),

                        Responses = responses,
                        DefaultResponse = defaultResponse,

                        Parameters = operation.Parameters.Select(parameter => new ParameterModel
                        {
                            Name = parameter.Name,
                            Type = resolver.Resolve(parameter.ActualSchema, parameter.IsRequired, parameter.Name),
                            IsLast = operation.Parameters.LastOrDefault() == parameter
                        }).ToList(),

                        ContentParameter =
                            operation.Parameters.Where(p => p.Kind == SwaggerParameterKind.body)
                                .Select(p => new ParameterModel { Name = p.Name })
                                .SingleOrDefault(),

                        PlaceholderParameters =
                            operation.Parameters.Where(p => p.Kind == SwaggerParameterKind.path).Select(p => new ParameterModel
                            {
                                Name = p.Name,
                                IsDate = p.Format == JsonFormatStrings.DateTime
                            }),

                        QueryParameters =
                            operation.Parameters.Where(p => p.Kind == SwaggerParameterKind.query).Select(p => new ParameterModel
                            {
                                Name = p.Name,
                                IsDate = p.Format == JsonFormatStrings.DateTime
                            }).ToList(),
                    };
                }).ToList();
            return operations;
        }

        internal abstract string RenderFile(string clientCode);

        internal abstract string RenderClientCode(string controllerName, IEnumerable<OperationModel> operations);

        internal abstract string GetType(JsonSchema4 schema, string typeNameHint);

        internal abstract string GetExceptionType(SwaggerOperation operation);

        internal abstract string GetResultType(SwaggerOperation operation);
    }
}