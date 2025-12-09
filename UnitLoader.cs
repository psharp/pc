/// <summary>
/// Loads and parses Pascal unit source files (.pas) with dependency resolution.
/// Handles circular dependency detection and caching of loaded units.
/// </summary>
using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler;

/// <summary>
/// Unit loader for Pascal source files.
/// Manages unit loading, parsing, and dependency resolution for the interpreter.
/// </summary>
public class UnitLoader
{
    /// <summary>Cache of already loaded and parsed units.</summary>
    private readonly Dictionary<string, UnitNode> _loadedUnits = new();

    /// <summary>Set of units currently being loaded (for circular dependency detection).</summary>
    private readonly HashSet<string> _loadingUnits = new();

    /// <summary>Directory path to search for unit files.</summary>
    private readonly string _searchPath;

    /// <summary>
    /// Initializes a new unit loader.
    /// </summary>
    /// <param name="searchPath">Directory to search for unit files (default is current directory).</param>
    public UnitLoader(string searchPath = ".")
    {
        _searchPath = searchPath;
    }

    /// <summary>
    /// Loads and parses a unit by name, handling dependencies recursively.
    /// Returns cached unit if already loaded.
    /// </summary>
    /// <param name="unitName">The name of the unit to load.</param>
    /// <returns>The parsed UnitNode.</returns>
    /// <exception cref="Exception">Thrown if unit not found or circular dependency detected.</exception>
    public UnitNode LoadUnit(string unitName)
    {
        string unitNameLower = unitName.ToLower();

        // Check if already loaded
        if (_loadedUnits.TryGetValue(unitNameLower, out var cachedUnit))
        {
            return cachedUnit;
        }

        // Check for circular dependency
        if (_loadingUnits.Contains(unitNameLower))
        {
            throw new Exception($"Circular unit dependency detected: {unitName}");
        }

        _loadingUnits.Add(unitNameLower);

        try
        {
            // Find the unit file
            string unitFilePath = FindUnitFile(unitName);
            if (unitFilePath == null)
            {
                throw new Exception($"Unit file not found: {unitName}.pas");
            }

            // Read and parse the unit
            string source = File.ReadAllText(unitFilePath);
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            var unit = parser.ParseUnit();

            // Verify unit name matches file
            if (!unit.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Unit name mismatch: file declares unit '{unit.Name}' but expected '{unitName}'");
            }

            // Recursively load units that this unit depends on
            foreach (var usedUnit in unit.UsedUnits)
            {
                LoadUnit(usedUnit);
            }

            // Cache the loaded unit
            _loadedUnits[unitNameLower] = unit;

            return unit;
        }
        finally
        {
            _loadingUnits.Remove(unitNameLower);
        }
    }

    private string? FindUnitFile(string unitName)
    {
        // Try current directory first
        string filename = $"{unitName}.pas";
        if (File.Exists(filename))
        {
            return filename;
        }

        // Try search path
        string searchPathFile = Path.Combine(_searchPath, filename);
        if (File.Exists(searchPathFile))
        {
            return searchPathFile;
        }

        return null;
    }

    public Dictionary<string, UnitNode> GetLoadedUnits()
    {
        return new Dictionary<string, UnitNode>(_loadedUnits);
    }

    public void Clear()
    {
        _loadedUnits.Clear();
        _loadingUnits.Clear();
    }
}
