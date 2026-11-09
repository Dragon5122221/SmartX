using SmartX.Shared.Deployment;
using SmartX.Shared.Validation;
namespace SmartX.Core.Validation;

/// <summary>ALG-01: bounded recursive depth-first validation of the deployment tree.</summary>
public sealed class DeploymentTreeValidator
{
    public const int MaximumDepth = 32;
    /// <summary>TREE-01: validate root, unique identifiers, safety inheritance, depth and cycles.</summary>
    public ValidationResult Validate(DeploymentNode? root)
    {
        var result = new ValidationResult();
        // DATA STRUCTURE DS-09: hash sets for O(1)-average duplicate and active-path checks.
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var active = new HashSet<DeploymentNode>(ReferenceEqualityComparer.Instance);
        Visit(root, "Facility", 0, true, ids, active, result);
        return result;
    }
    /// <summary>TREE-02: recursive step; each branch terminates on null, depth limit, cycle or leaf.</summary>
    private static void Visit(DeploymentNode? node, string path, int depth, bool parentEnabled,
        HashSet<string> ids, HashSet<DeploymentNode> active, ValidationResult result)
    {
        // ALG-01 BASE CASES: safely terminate malformed or excessively deep branches.
        if (node is null) { result.Issues.Add(new(path, "Node cannot be null.")); return; }
        if (depth > MaximumDepth) { result.Issues.Add(new(path, "Maximum depth is 32.")); return; }
        if (result.NodesVisited >= 4096) { if (result.Issues.Count < 100) result.Issues.Add(new(path, "Tree is limited to 4096 nodes.")); return; }
        if (!active.Add(node)) { result.Issues.Add(new(path, "Cycle detected on the active ancestor path.")); return; }
        result.NodesVisited++;
        if (string.IsNullOrWhiteSpace(node.Id) || node.Id.Length > 64 || !ids.Add(node.Id))
            result.Issues.Add(new(path, "Node ID must be unique and 1–64 characters."));
        if (string.IsNullOrWhiteSpace(node.Name) || node.Name.Length > 100) result.Issues.Add(new(path, "Node name must contain 1–100 characters."));
        if (!parentEnabled && node.Enabled) result.Issues.Add(new(path, "An enabled node cannot sit beneath a disabled ancestor."));
        if (node.Children is null) result.Issues.Add(new(path, "Children must be an array, even for a leaf."));
        else
            // ALG-01 RECURSIVE CALL: empty Children is the successful leaf base case.
            foreach (var child in node.Children.Take(4097)) Visit(child, $"{path}/{child?.Id ?? "null"}", depth + 1, parentEnabled && node.Enabled, ids, active, result);
        active.Remove(node);
    }
}
