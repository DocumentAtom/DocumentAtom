# Hierarchical Atomization

## Overview

Hierarchical atomization is the process of organizing document atoms into a tree structure that reflects the logical structure of the source document. Instead of returning a flat list of atoms, processors can build parent-child relationships where atoms contain other atoms, creating a meaningful document hierarchy.

## Core Concepts

### Atoms and Quarks

- **Atom**: A self-contained unit of content from a document (defined in `DocumentAtom.Core.Atoms.Atom`)
- **Quark**: A child atom - quarks ARE atoms themselves
- The `List<Atom> Quarks` property on the `Atom` class establishes parent-child relationships
- This creates a recursive tree structure where any atom can contain other atoms

**Tree Structure Example:**
```
Atom (root)
├── Quark (child Atom)
│   └── Quark (grandchild Atom)
├── Quark (child Atom)
└── Quark (child Atom)
    ├── Quark (grandchild Atom)
    └── Quark (grandchild Atom)
```

### The ParentGUID Property

The `Atom` class includes a `Guid? ParentGUID` property that tracks the parent-child relationship:

- **Root-level atoms**: `ParentGUID = null`
- **Child atoms (quarks)**: `ParentGUID = parent.GUID`

This property provides bidirectional navigation through the tree structure:
- Navigate down: Use `atom.Quarks` to access children
- Navigate up: Use `atom.ParentGUID` to identify parent

## MarkdownProcessor Implementation

The `MarkdownProcessor` class in `DocumentAtom.Markdown` demonstrates hierarchical atomization based on markdown header levels.

### Document Structure in Markdown

Markdown has a natural hierarchical structure defined by header levels (`#`, `##`, `###`, etc.):

```markdown
# Document Title (level 1)
Some introductory text.

## Section 1 (level 2)
Section content with a list:
- Item 1
- Item 2

### Subsection 1.1 (level 3)
Subsection content.

## Section 2 (level 2)
More content.
```

**Resulting Hierarchy:**
```
Atom: # Document Title
├── Atom: introductory text
├── Atom: ## Section 1
│   ├── Atom: section content
│   ├── Atom: unordered list
│   └── Atom: ### Subsection 1.1
│       └── Atom: subsection content
└── Atom: ## Section 2
    └── Atom: content
```

### Algorithm

The `MarkdownProcessor` uses a **two-pass approach**:

1. **First Pass**: Parse markdown into flat list of atoms using existing stream-based logic
2. **Second Pass**: Build hierarchy from flat list

#### BuildHierarchy Method

The core algorithm uses a dictionary to track the current header at each level (1-6):

```csharp
Dictionary<int, Atom> currentHeaders = new Dictionary<int, Atom>();
List<Atom> rootAtoms = new List<Atom>();
```

**For each atom:**

1. **If atom is a header** (has `HeaderLevel` property):
   - Find parent: Search for the nearest header with level < current level
   - If parent exists:
     - Set `atom.ParentGUID = parent.GUID`
     - Add atom to `parent.Quarks`
   - If no parent exists:
     - Set `atom.ParentGUID = null`
     - Add atom to `rootAtoms` list
   - Update tracking: `currentHeaders[level] = atom`
   - Clear deeper levels from tracking (they're out of scope)

2. **If atom is non-header** (text, list, table, code, etc.):
   - Find deepest current header (highest level number in tracking)
   - If header exists:
     - Set `atom.ParentGUID = parent.GUID`
     - Add atom to `parent.Quarks`
   - If no header exists:
     - Set `atom.ParentGUID = null`
     - Add atom to `rootAtoms` list

**Return:** Only root-level atoms (they contain the full tree via `Quarks`)

### Helper Methods

**FindParentHeader(Dictionary<int, Atom> currentHeaders, int level)**
- Searches backwards from `level-1` down to `1`
- Returns the nearest header with level < current level
- Returns `null` if no parent exists

**FindDeepestHeader(Dictionary<int, Atom> currentHeaders)**
- Returns the header with the highest level number (deepest in hierarchy)
- Returns `null` if no headers exist

**ClearDeeperLevels(Dictionary<int, Atom> currentHeaders, int level)**
- Removes all tracking for levels > current level
- Ensures headers at deeper levels don't incorrectly become parents

### Edge Cases and Malformed Documents

The implementation handles various edge cases gracefully:

| Scenario | Handling |
|----------|----------|
| **Level jump** (`##` → `#####`) | Attach `#####` directly to `##` (no phantom headers created) |
| **Document starts with `##`** | Treated as root-level atom (no `#` required) |
| **Content before first header** | Root-level atoms (siblings to headers) |
| **Multiple `#` headers** | Each is a separate root atom with its own subtree |
| **No headers at all** | Returns flat list (all atoms root-level) |
| **Level regression** (`##` → `#` → `##`) | Stack naturally handles: new `##` becomes sibling of first `##` |

**Philosophy:** Be permissive - never throw exceptions, preserve all content, make reasonable assumptions.

## Settings

### MarkdownProcessorSettings

The `BuildHierarchy` property controls hierarchical atomization:

```csharp
/// <summary>
/// Enable or disable hierarchical structure building.
/// When true, atoms will be organized in a tree structure based on header levels.
/// When false, atoms will be returned as a flat list.
/// Default is true.
/// </summary>
public bool BuildHierarchy { get; set; } = true;
```

**When disabled:**
- Processor returns flat list of atoms
- All atoms have `ParentGUID = null`
- Maintains backward compatibility

## Implementation Guide for Other Processors

### Step 1: Add Settings Property

Add a `BuildHierarchy` property to your processor's settings class:

```csharp
public class YourProcessorSettings : ProcessorSettingsBase
{
    public bool BuildHierarchy { get; set; } = true;
}
```

### Step 2: Define Hierarchical Structure

Identify the logical structure in your document type:

- **Word (DOCX)**: Heading styles (Heading 1, 2, 3, etc.)
- **PowerPoint (PPTX)**: Slides → Shapes/Text boxes → Content
- **Excel (XLSX)**: Workbook → Sheets → Rows/Cells
- **PDF**: Bookmarks, headings, or page-based hierarchy
- **HTML**: DOM tree structure (`<h1>`, `<h2>`, `<section>`, etc.)

### Step 3: Extract Flat Atoms

Implement your existing extraction logic to produce a flat list of atoms. Ensure each atom has appropriate metadata:

- `Type` (AtomTypeEnum)
- `Position` (ordinal position)
- Document-specific properties (e.g., `HeaderLevel` for markdown)

### Step 4: Implement BuildHierarchy Method

Create a method that processes the flat list and builds relationships:

```csharp
private IEnumerable<Atom> BuildHierarchy(List<Atom> flatAtoms)
{
    if (flatAtoms == null || flatAtoms.Count == 0)
    {
        return Enumerable.Empty<Atom>();
    }

    // Track hierarchy state (e.g., current section at each level)
    // Process each atom and determine parent
    // Set ParentGUID and add to parent.Quarks
    // Return root-level atoms
}
```

**Key Steps:**
1. Maintain state tracking (stack, dictionary, or queue)
2. For each atom, determine its parent based on document structure
3. Set `atom.ParentGUID = parent.GUID` for child atoms
4. Set `atom.ParentGUID = null` for root atoms
5. Add children to `parent.Quarks` list (initialize if null)
6. Return only root-level atoms

### Step 5: Modify Extract Method

Update your `Extract()` method to conditionally build hierarchy:

```csharp
public override IEnumerable<Atom> Extract(string filename)
{
    List<Atom> flatAtoms = YourExtractionMethod(filename).ToList();

    if (_Settings.BuildHierarchy)
    {
        return BuildHierarchy(flatAtoms);
    }
    else
    {
        // Ensure ParentGUID is null for flat list
        foreach (Atom atom in flatAtoms)
        {
            atom.ParentGUID = null;
        }
        return flatAtoms;
    }
}
```

### Step 6: Handle Edge Cases

Consider document-specific edge cases:

- Malformed structure (missing parent elements)
- Content before first structural element
- Multiple root elements
- Empty documents
- Nested structures exceeding expected depth

**Principle:** Always be permissive - preserve all content, make reasonable assumptions, never throw exceptions for structural issues.

## Testing Considerations

When implementing hierarchical atomization, test the following scenarios:

1. **Well-formed documents**: Verify correct tree structure
2. **Malformed documents**: Ensure no exceptions, all content preserved
3. **Empty documents**: Handle gracefully
4. **Flat structure**: Documents with no hierarchy
5. **Deep nesting**: Verify performance with deeply nested structures
6. **Large documents**: Test memory and performance
7. **Settings toggle**: Verify `BuildHierarchy = false` returns flat list

## Benefits of Hierarchical Atomization

1. **Semantic Preservation**: Maintains document structure for AI/ML processing
2. **Navigation**: Easy traversal of document tree
3. **Context**: Parent-child relationships provide content context
4. **Chunking**: Natural boundaries for breaking large documents
5. **Analysis**: Structure-aware document analysis
6. **Reconstruction**: Can rebuild document from atom tree
7. **Filtering**: Can extract specific sections/subsections
8. **Backward Compatible**: Optional via settings flag

## References

- `DocumentAtom.Core.Atoms.Atom` - Core atom class with `Quarks` and `ParentGUID`
- `DocumentAtom.Markdown.MarkdownProcessor` - Reference implementation
- `DocumentAtom.Markdown.MarkdownProcessorSettings` - Settings with `BuildHierarchy` property
