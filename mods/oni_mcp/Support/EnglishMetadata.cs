using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OniMcp.Core;
using OniMcp.Tools;

namespace OniMcp.Support
{
    internal static class EnglishMetadata
    {
        public static bool ContainsCjk(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char ch in value)
            {
                int code = ch;
                if ((code >= 0x3400 && code <= 0x9FFF) ||
                    (code >= 0xF900 && code <= 0xFAFF) ||
                    (code >= 0x20000 && code <= 0x2A6DF))
                    return true;
            }
            return false;
        }

        public static string ToolDescription(McpTool tool)
        {
            if (tool == null)
                return string.Empty;
            if (!ContainsCjk(tool.Description) && !string.IsNullOrWhiteSpace(tool.Description))
                return tool.Description;

            string description = "ONI MCP tool for " + HumanizeIdentifier(tool.Name) + ".";
            string required = RequiredParameters(tool.Parameters);
            if (!string.IsNullOrEmpty(required))
                description += " Required arguments: " + required + ".";
            return description;
        }

        public static string ParameterDescription(string name, McpToolParameter parameter)
        {
            string raw = parameter != null ? parameter.Description : null;
            if (!ContainsCjk(raw) && !string.IsNullOrWhiteSpace(raw))
                return raw;

            string description = KnownParameterDescription(name);
            if (string.IsNullOrEmpty(description))
                description = "Parameter for " + HumanizeIdentifier(name) + ".";

            var details = new List<string>();
            if (parameter != null && !string.IsNullOrEmpty(parameter.Type))
                details.Add("Type: " + parameter.Type + ".");
            if (parameter != null)
                details.Add(parameter.Required ? "Required." : "Optional.");
            if (parameter != null && parameter.EnumValues != null && parameter.EnumValues.Count > 0)
                details.Add("Allowed values: " + string.Join(", ", parameter.EnumValues.Take(12).ToArray()) + ".");

            return description + (details.Count == 0 ? string.Empty : " " + string.Join(" ", details.ToArray()));
        }

        public static McpResourceInfo ResourceInfo(McpResourceInfo info)
        {
            if (info == null)
                return null;
            return new McpResourceInfo
            {
                Uri = info.Uri,
                Name = info.Name,
                Title = CleanTitle(info.Title, info.Name),
                Description = CleanResourceDescription(info.Description, info.Name, info.Uri),
                MimeType = info.MimeType
            };
        }

        public static McpResourceTemplateInfo ResourceTemplateInfo(McpResourceTemplateInfo info)
        {
            if (info == null)
                return null;
            return new McpResourceTemplateInfo
            {
                UriTemplate = info.UriTemplate,
                Name = info.Name,
                Title = CleanTitle(info.Title, info.Name),
                Description = CleanResourceDescription(info.Description, info.Name, info.UriTemplate),
                MimeType = info.MimeType
            };
        }

        public static McpPromptInfo PromptInfo(McpPromptInfo info)
        {
            if (info == null)
                return null;
            return new McpPromptInfo
            {
                Name = info.Name,
                Title = CleanTitle(info.Title, info.Name),
                Description = CleanPromptText(info.Description, info.Name),
                Arguments = info.Arguments == null ? null : info.Arguments.Select(PromptArgument).ToList()
            };
        }

        public static McpPromptArgument PromptArgument(McpPromptArgument argument)
        {
            if (argument == null)
                return null;
            return new McpPromptArgument
            {
                Name = argument.Name,
                Title = CleanTitle(argument.Title, argument.Name),
                Description = ContainsCjk(argument.Description)
                    ? ParameterDescription(argument.Name, new McpToolParameter { Type = "string", Required = argument.Required })
                    : argument.Description,
                Required = argument.Required
            };
        }

        public static GetPromptResult PromptResult(GetPromptResult result)
        {
            if (result == null)
                return null;

            return new GetPromptResult
            {
                Description = CleanPromptText(result.Description, "prompt"),
                Messages = result.Messages == null ? null : result.Messages.Select(message => new PromptMessage
                {
                    Role = message.Role,
                    Content = message.Content == null ? null : new ToolContent
                    {
                        Type = message.Content.Type,
                        Text = CleanPromptText(message.Content.Text, "prompt_message")
                    }
                }).ToList()
            };
        }

        public static List<string> Tags(IEnumerable<string> tags)
        {
            if (tags == null)
                return null;
            return tags.Where(tag => !ContainsCjk(tag)).ToList();
        }

        public static string HumanizeIdentifier(string identifier)
        {
            return HumanizeIdentifier(identifier, false);
        }

        public static string HumanizeTitle(string identifier)
        {
            return HumanizeIdentifier(identifier, true);
        }

        private static string HumanizeIdentifier(string identifier, bool titleCase)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return titleCase ? "Item" : "item";

            var parts = identifier
                .Replace('-', '_')
                .Replace('/', '_')
                .Replace(':', '_')
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            var words = new List<string>();
            foreach (string part in parts)
            {
                string word = NormalizeWord(part);
                words.Add(titleCase ? TitleCase(word) : word);
            }
            return string.Join(" ", words.ToArray());
        }

        private static string NormalizeWord(string value)
        {
            string lower = (value ?? string.Empty).ToLowerInvariant();
            switch (lower)
            {
                case "mcp": return "MCP";
                case "oni": return "ONI";
                case "ui": return "UI";
                case "dlc": return "DLC";
                case "id": return "ID";
                case "ids": return "IDs";
                case "db": return "database";
                case "dupe": return "duplicant";
                case "dupes": return "duplicants";
                case "kcal": return "kcal";
                case "hp": return "HP";
                case "rle": return "RLE";
                default: return lower;
            }
        }

        private static string TitleCase(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;
            if (word.All(ch => !char.IsLetter(ch) || char.IsUpper(ch)))
                return word;
            return char.ToUpperInvariant(word[0]) + word.Substring(1);
        }

        private static string CleanTitle(string title, string fallbackName)
        {
            if (!ContainsCjk(title) && !string.IsNullOrWhiteSpace(title))
                return title;
            return HumanizeTitle(fallbackName);
        }

        private static string CleanResourceDescription(string description, string name, string uri)
        {
            if (!ContainsCjk(description) && !string.IsNullOrWhiteSpace(description))
                return description;
            string result = "ONI resource for " + HumanizeIdentifier(name) + ".";
            if (!string.IsNullOrEmpty(uri))
                result += " URI: " + uri + ".";
            return result;
        }

        private static string CleanPromptText(string text, string name)
        {
            if (!ContainsCjk(text) && !string.IsNullOrWhiteSpace(text))
                return text;
            return "Use ONI MCP resources and tools to inspect the live colony state, plan any save-changing action first, use dry-run or validation when available, and verify with read-only resources after execution.";
        }

        private static string RequiredParameters(Dictionary<string, McpToolParameter> parameters)
        {
            if (parameters == null)
                return string.Empty;
            var required = parameters
                .Where(kv => kv.Value != null && kv.Value.Required)
                .Select(kv => kv.Key)
                .OrderBy(name => name)
                .ToList();
            return required.Count == 0 ? string.Empty : string.Join(", ", required.ToArray());
        }

        private static string KnownParameterDescription(string name)
        {
            switch ((name ?? string.Empty).ToLowerInvariant())
            {
                case "id": return "Target instance ID.";
                case "targetid": return "Target instance ID.";
                case "objectid": return "Target object ID.";
                case "buildingid": return "Target building ID.";
                case "dupeid": return "Target duplicant ID.";
                case "name": return "Target name.";
                case "newname": return "New name to apply.";
                case "dupe": return "Duplicant name or ID.";
                case "query": return "Search text or intent filter.";
                case "group": return "Tool group filter.";
                case "mode": return "Tool mode filter.";
                case "risk": return "Tool risk filter.";
                case "detail": return "Result detail level.";
                case "limit": return "Maximum number of results.";
                case "worldid": return "World ID; defaults to the current world when omitted.";
                case "areaid": return "Saved area handle from a previous map or resource result.";
                case "x": return "World cell X coordinate.";
                case "y": return "World cell Y coordinate.";
                case "x1": return "Lower-left or first-corner X coordinate.";
                case "y1": return "Lower-left or first-corner Y coordinate.";
                case "x2": return "Upper-right or second-corner X coordinate.";
                case "y2": return "Upper-right or second-corner Y coordinate.";
                case "confirm": return "Set true to confirm this save-changing action.";
                case "dryrun": return "Preview only; do not change the save.";
                case "validateonly": return "Validate only; do not execute the action.";
                case "priority": return "Priority value.";
                case "tool": return "Tool name.";
                case "arguments": return "Arguments to pass to the tool.";
                case "tools": return "Tool call list.";
                case "prefabid": return "ONI prefab ID.";
                case "material": return "Build material or element ID.";
                case "facade": return "Facade or skin ID.";
                case "direction": return "Direction value.";
                case "length": return "Number of cells to cover.";
                case "hour": return "Schedule hour.";
                case "blocktype": return "Schedule block type.";
                case "enabled": return "Whether the option should be enabled.";
                case "apply": return "Whether to apply the proposed change.";
                case "style": return "Naming or output style.";
                case "profile": return "Output profile.";
                case "format": return "Output format.";
                case "includehidden": return "Include hidden entries when true.";
                case "agentid": return "Stable visible agent pointer ID.";
                case "displaytext": return "Short player-facing status text shown near the visible pointer.";
                default: return null;
            }
        }
    }
}
