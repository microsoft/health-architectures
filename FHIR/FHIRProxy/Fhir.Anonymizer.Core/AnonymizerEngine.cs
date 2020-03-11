using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Fhir.Anonymizer.Core.AnonymizerConfigurations;
using Fhir.Anonymizer.Core.Extensions;
using Fhir.Anonymizer.Core.Processors;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.FhirPath;

namespace Fhir.Anonymizer.Core
{
    public class AnonymizerEngine
    {
        private readonly FhirJsonParser _parser = new FhirJsonParser();
        private readonly PocoStructureDefinitionSummaryProvider _provider = new PocoStructureDefinitionSummaryProvider();
        private readonly AnonymizerConfigurationManager _configurationManger;
        private readonly Dictionary<string, IAnonymizerProcessor> _processors;

        public AnonymizerEngine(string configFilePath) : this(AnonymizerConfigurationManager.CreateFromConfigurationFile(configFilePath)) 
        { 
        }

        public AnonymizerEngine(AnonymizerConfigurationManager configurationManager)
        {
            _configurationManger = configurationManager;
            _processors = new Dictionary<string, IAnonymizerProcessor>();
            InitializeProcessors(_configurationManger);
        }

        public string AnonymizeJson(string json)
        {
            EnsureArg.IsNotNullOrEmpty(json, nameof(json));

            ElementNode root;
            try
            {
                root = ElementNode.FromElement(_parser.Parse(json).ToTypedElement());
            }
            catch(Exception innerException)
            {
                throw new Exception("Failed to parse json resource, please check the json content.", innerException);
            }

            return AnonymizeResourceNode(root).ToJson();
        }

        public ElementNode AnonymizeResourceNode(ElementNode root)
        {
            EnsureArg.IsNotNull(root, nameof(root));

            if (root.IsBundleNode())
            {
                var entryResources = root.GetEntryResourceChildren();
                AnonymizeInternalResourceNodes(entryResources);
            }

            if (root.HasContainedNode())
            {
                var containedResources = root.GetContainedChildren();
                AnonymizeInternalResourceNodes(containedResources);
            }

            var resourceContext = ResourceAnonymizerContext.Create(root, _configurationManger);
            foreach (var rule in resourceContext.RuleList)
            {
                var pathCompileExpression = new FhirPathCompiler().Compile($"{rule.Path}");
                var matchedNodes = pathCompileExpression(root, EvaluationContext.CreateDefault())
                    .Cast<ElementNode>();
                foreach (var node in matchedNodes)
                {
                    AnonymizeChildNode(node, rule.Method, resourceContext.PathSet);
                }
            }

            return root;
        }

        private void AnonymizeInternalResourceNodes(List<ElementNode> resourceNodes)
        {
            foreach(var resource in resourceNodes)
            {
                var newResource = AnonymizeResourceNode(GetResourceRoot(resource));
                resource.Parent.Replace(_provider, resource, newResource);
            }
        }

        private void AnonymizeChildNode(ElementNode node, string method, HashSet<string> rulePathSet)
        {
            method = method.ToUpperInvariant();

            if (node.Value != null && _processors.ContainsKey(method))
            {
                _processors[method].Process(node);
            }

            var children = node.Children().Cast<ElementNode>();
            foreach (var child in children)
            {
                if (!rulePathSet.Contains(child.GetFhirPath()))
                {
                    AnonymizeChildNode(child, method, rulePathSet);
                }
            }
        }

        private ElementNode GetResourceRoot(ElementNode node)
        {
            var content = node.ToJson();
            return ElementNode.FromElement(_parser.Parse(content).ToTypedElement());
        }

        private void InitializeProcessors(AnonymizerConfigurationManager configurationManager)
        {
            _processors[AnonymizerMethods.DateShift.ToString().ToUpperInvariant()] = DateShiftProcessor.Create(configurationManager);
            _processors[AnonymizerMethods.Redact.ToString().ToUpperInvariant()] = RedactProcessor.Create(configurationManager);
            _processors[AnonymizerMethods.Keep.ToString().ToUpperInvariant()] = new KeepProcessor();
        }
    }
}
