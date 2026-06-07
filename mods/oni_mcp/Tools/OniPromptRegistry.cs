using System;
using System.Collections.Generic;
using System.Linq;
using OniMcp.Core;
using OniMcp.Support;

namespace OniMcp.Tools
{
    /// <summary>
    /// MCP prompt registry for reusable ONI agent workflows.
    /// </summary>
    public static class OniPromptRegistry
    {
        private static readonly List<McpPrompt> _prompts = new List<McpPrompt>
        {
            new McpPrompt
            {
                Name = "colony_triage",
                Title = "Colony Triage",
                Description = "Quickly inspect the current colony and prioritize problems that can cause death, power loss, oxygen loss, or food failure.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "focus", Title = "Focus", Description = "Optional focus, for example oxygen, food, power, or dupes.", Required = false }
                },
                Builder = args => BuildResult(
                    "Colony triage workflow",
                    "You are an Oxygen Not Included colony triage assistant. Read oni://colony/status, oni://colony/diagnostics, oni://colony/alerts, and oni://resources/food first, then rank the next actions by risk." +
                    Optional(args, "focus", " Focus: {0}.") +
                    " For actions that modify the save, recommend them only unless the user explicitly asks you to execute.")
            },
            new McpPrompt
            {
                Name = "next_cycle_plan",
                Title = "Next Cycle Plan",
                Description = "Generate a next-cycle action plan from the current colony state.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "objective", Title = "Objective", Description = "Goal, for example stabilize oxygen, expand food, or research.", Required = false },
                    new McpPromptArgument { Name = "riskTolerance", Title = "Risk Tolerance", Description = "Risk tolerance: low, medium, or high.", Required = false }
                },
                Builder = args => BuildResult(
                    "Next-cycle planning workflow",
                    "Read oni://colony/summary, oni://resources/inventory, oni://research/status, oni://schedules, and oni://dupes. Output a compact next-cycle plan grouped as immediate, queueable, and defer." +
                    Optional(args, "objective", " Objective: {0}.") +
                    Optional(args, "riskTolerance", " Risk tolerance: {0}."))
            },
            new McpPrompt
            {
                Name = "inspect_area",
                Title = "Inspect Area",
                Description = "Analyze a map area, preferring text-map resources before screenshots.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "x1", Title = "X1", Description = "Lower-left X coordinate.", Required = false },
                    new McpPromptArgument { Name = "y1", Title = "Y1", Description = "Lower-left Y coordinate.", Required = false },
                    new McpPromptArgument { Name = "x2", Title = "X2", Description = "Upper-right X coordinate.", Required = false },
                    new McpPromptArgument { Name = "y2", Title = "Y2", Description = "Upper-right Y coordinate.", Required = false }
                },
                Builder = args => BuildResult(
                    "Area inspection workflow",
                    "First read oni://world/text-map?x1=" + Arg(args, "x1", "") + "&y1=" + Arg(args, "y1", "") + "&x2=" + Arg(args, "x2", "") + "&y2=" + Arg(args, "y2", "") + "&profile=scan. Use the low-token RLE text map to scan terrain and gas/liquid/solid distribution. If you need buildings, dupes, resources, or per-cell detail, reuse the same areaId with world_text_map and enable includeBuildings, includeDupes, includeItems, includeElements, or detail=full. Use screenshots only when visual confirmation is needed.")
            },
            new McpPrompt
            {
                Name = "dupe_care_review",
                Title = "Duplicant Care Review",
                Description = "Review duplicant needs, stress, schedules, and skill setup.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "dupe", Title = "Duplicant", Description = "Optional duplicant name or ID.", Required = false }
                },
                Builder = args => BuildResult(
                    "Duplicant care workflow",
                    "Read oni://dupes and oni://schedules. If a duplicant is specified, call dupes_detail, dupes_needs, and dupes_attributes for details." +
                    Optional(args, "dupe", " Duplicant: {0}.") +
                    " Output care risks plus schedule, diet, and skill recommendations.")
            },
            new McpPrompt
            {
                Name = "power_audit",
                Title = "Power Audit",
                Description = "Check colony power health and find supply gaps, battery-drain risk, and wire overloads.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "worldId", Title = "World ID", Description = "World ID; defaults to the current world.", Required = false },
                    new McpPromptArgument { Name = "detail", Title = "Detail", Description = "Detail level: brief, summary, or full. Defaults to summary.", Required = false }
                },
                Builder = args => BuildResult(
                    "Power audit workflow",
                    "Read oni://power/summary first for overall power state. Check whether any circuit load is near or above 100%, and whether battery charge is low." +
                    Optional(args, "detail", " Detail level: {0}. If full, also read oni://buildings/configurables filtered to power-related buildings for detailed settings.") +
                    " Recommend optimizations: add generation, add batteries, reduce load, or split circuits." +
                    Optional(args, "worldId", " World ID: {0}."))
            },
            new McpPrompt
            {
                Name = "rooms_overview",
                Title = "Rooms Overview",
                Description = "Check colony room status and find missing morale rooms or unmet room criteria.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "worldId", Title = "World ID", Description = "World ID; defaults to the current world.", Required = false },
                    new McpPromptArgument { Name = "focus", Title = "Focus", Description = "Focus, for example morale, bed, food, toilet, or recreation.", Required = false }
                },
                Builder = args => BuildResult(
                    "Rooms overview workflow",
                    "Read oni://rooms/list for all rooms. Check whether key room types are missing, such as Barracks, Great Hall, Washroom, and Recreation, and whether room size and criteria are satisfied." +
                    Optional(args, "focus", " Focus: {0}.") +
                    " Provide room-planning recommendations, prioritizing morale-related rooms." +
                    Optional(args, "worldId", " World ID: {0}."))
            },
            new McpPrompt
            {
                Name = "thermal_audit",
                Title = "Thermal Audit",
                Description = "Scan colony overheat risk and find equipment or areas that are becoming too hot.",
                Arguments = new List<McpPromptArgument>
                {
                    new McpPromptArgument { Name = "worldId", Title = "World ID", Description = "World ID; defaults to the current world.", Required = false },
                    new McpPromptArgument { Name = "marginC", Title = "Risk Margin C", Description = "Risk temperature margin in Celsius; defaults to 15.", Required = false }
                },
                Builder = args => BuildResult(
                    "Thermal audit workflow",
                    "Read oni://thermal/overheat-risk?marginC=" + Arg(args, "marginC", "15") + " to scan at-risk buildings. If any equipment is overheated, handle it first." +
                    " Check element distribution in hot areas; oni://world/elements can help." +
                    " Recommend cooling, ventilation, insulated tile, or heat-source removal." +
                    Optional(args, "worldId", " World ID: {0}."))
            }
        };

        public static List<McpPromptInfo> GetPromptInfos()
        {
            return _prompts
                .Select(prompt => EnglishMetadata.PromptInfo(new McpPromptInfo
                {
                    Name = prompt.Name,
                    Title = prompt.Title,
                    Description = prompt.Description,
                    Arguments = prompt.Arguments
                }))
                .OrderBy(prompt => prompt.Name)
                .ToList();
        }

        public static GetPromptResult GetPrompt(string name, Dictionary<string, string> arguments)
        {
            var prompt = _prompts.FirstOrDefault(p => p.Name == name);
            if (prompt == null)
                return null;
            return prompt.Builder(arguments ?? new Dictionary<string, string>());
        }

        private static GetPromptResult BuildResult(string description, string text)
        {
            return EnglishMetadata.PromptResult(new GetPromptResult
            {
                Description = description,
                Messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "user",
                        Content = new ToolContent { Type = "text", Text = text }
                    }
                }
            });
        }

        private static string Arg(Dictionary<string, string> args, string key, string fallback)
        {
            string value;
            if (args != null && args.TryGetValue(key, out value) && !string.IsNullOrEmpty(value))
                return value;
            return fallback;
        }

        private static string Optional(Dictionary<string, string> args, string key, string template)
        {
            string value = Arg(args, key, "");
            return string.IsNullOrEmpty(value) ? "" : string.Format(template, value);
        }

        private class McpPrompt
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<McpPromptArgument> Arguments { get; set; }
            public Func<Dictionary<string, string>, GetPromptResult> Builder { get; set; }
        }
    }
}
