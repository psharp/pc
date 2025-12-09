/// <summary>
/// Loads compiled Pascal unit bytecode files (.pbu) with dependency resolution.
/// Handles circular dependency detection and caching of loaded units.
/// </summary>
using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler;

/// <summary>
/// Bytecode unit loader for compiled Pascal units.
/// Manages loading .pbu files and dependency resolution for the virtual machine.
/// </summary>
public class BytecodeUnitLoader
{
    /// <summary>Cache of already loaded bytecode units.</summary>
    private readonly Dictionary<string, BytecodeUnit> _loadedUnits = new();

    /// <summary>Set of units currently being loaded (for circular dependency detection).</summary>
    private readonly HashSet<string> _loadingUnits = new();

    /// <summary>Directory path to search for unit bytecode files.</summary>
    private readonly string _searchPath;

    /// <summary>
    /// Initializes a new bytecode unit loader.
    /// </summary>
    /// <param name="searchPath">Directory to search for .pbu files (default is current directory).</param>
    public BytecodeUnitLoader(string searchPath = ".")
    {
        _searchPath = searchPath;
    }

    /// <summary>
    /// Loads a compiled unit by name, handling dependencies recursively.
    /// Returns cached unit if already loaded.
    /// </summary>
    /// <param name="unitName">The name of the unit to load.</param>
    /// <returns>The loaded BytecodeUnit.</returns>
    /// <exception cref="Exception">Thrown if unit not found or circular dependency detected.</exception>
    public BytecodeUnit LoadUnit(string unitName)
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
            // Find the unit bytecode file
            string unitFilePath = FindUnitFile(unitName);
            if (unitFilePath == null)
            {
                throw new Exception($"Unit bytecode file not found: {unitName}.pbu");
            }

            // Load the bytecode unit
            var serializer = new BytecodeUnitSerializer();
            var unit = serializer.LoadFromFile(unitFilePath);

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
        string filename = $"{unitName}.pbu";
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

    public Dictionary<string, BytecodeUnit> GetLoadedUnits()
    {
        return new Dictionary<string, BytecodeUnit>(_loadedUnits);
    }

    public void Clear()
    {
        _loadedUnits.Clear();
        _loadingUnits.Clear();
    }
}
