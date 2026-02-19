import json
import os
import sys
import time

import document_atom_sdk
from document_atom_sdk.models.processor_settings import (
    ApiProcessorSettingsModel,
    ChunkingConfigurationModel,
)


def prompt(question, default=None):
    """Prompt user for input with an optional default."""
    if default is not None:
        display = f"{question} [{default}]: "
    else:
        display = f"{question}: "
    answer = input(display).strip()
    return answer if answer else (default or "")


def prompt_yes_no(question, default=False):
    """Prompt user for a yes/no answer."""
    suffix = "[Y/n]" if default else "[y/N]"
    answer = input(f"{question} {suffix}: ").strip().lower()
    if not answer:
        return default
    return answer.startswith("y")


def show_menu():
    """Display available commands."""
    print()
    print("Available commands:")
    print("  ?               help, this menu")
    print("  q               quit")
    print("  cls             clear the screen")
    print(f"  endpoint        show current endpoint")
    print()
    print("Health & Status:")
    print("  connectivity    check if server is reachable")
    print("  detect          test type detection")
    print()
    print("Document Processing:")
    print("  csv             process CSV document")
    print("  excel           process Excel document")
    print("  html            process HTML document")
    print("  json            process JSON document")
    print("  markdown        process Markdown document")
    print("  ocr             process image with OCR")
    print("  pdf             process PDF document")
    print("  png             process PNG image")
    print("  powerpoint      process PowerPoint document")
    print("  rtf             process RTF document")
    print("  text            process text document")
    print("  word            process Word document")
    print("  xml             process XML document")
    print()


def test_connectivity():
    """Test server connectivity."""
    try:
        print("Checking server connectivity...")
        result = document_atom_sdk.Connectivity.validate_connectivity()
        print(f"Server is {'reachable' if result else 'unreachable'}")
    except Exception as e:
        print(f"Error checking connectivity: {e}")


def test_type_detection():
    """Test type detection."""
    try:
        filename = prompt("File path for type detection")
        if not filename or not os.path.isfile(filename):
            print("File not found.")
            return

        file_size = os.path.getsize(filename)
        print(f"Detecting type for {filename} ({file_size} bytes)...")

        result = document_atom_sdk.TypeDetection.detect_type(filename)
        if result:
            print("Type detection result:")
            print(f"  MIME Type: {getattr(result, 'mime_type', 'Unknown')}")
            print(f"  Extension: {getattr(result, 'extension', 'Unknown')}")
            print(f"  Document Type: {getattr(result, 'type', 'Unknown')}")
            print()
            print("Full result:")
            if hasattr(result, "model_dump"):
                print(json.dumps(result.model_dump(by_alias=True, exclude_none=True), indent=2))
            else:
                print(result)
        else:
            print("No type detection result received.")
    except Exception as e:
        print(f"Error in type detection: {e}")


def configure_settings(document_type):
    """Prompt user for processor settings."""
    configure = prompt_yes_no("Configure processor settings?", False)
    if not configure:
        return None

    kwargs = {}

    ocr_types = ["PDF", "Word", "Excel", "PowerPoint", "RTF"]
    if document_type in ocr_types:
        extract_ocr = prompt_yes_no("Extract atoms from images (OCR)?", False)
        if extract_ocr:
            kwargs["extract_atoms_from_images"] = True

    enable_chunking = prompt_yes_no("Enable chunking?", False)
    if enable_chunking:
        strategy = prompt(
            "Chunking strategy [FixedTokenCount/SentenceBased/ParagraphBased/RegexBased/"
            "WholeList/ListEntry/Row/RowWithHeaders/RowGroupWithHeaders/KeyValuePairs/WholeTable]",
            "FixedTokenCount",
        )

        fixed_token_count_str = prompt("Fixed token count", "128")
        fixed_token_count = int(fixed_token_count_str) if fixed_token_count_str.isdigit() else 128

        overlap_strategy = prompt(
            "Overlap strategy [SlidingWindow/SentenceBoundaryAware/SemanticBoundaryAware]",
            "SlidingWindow",
        )

        overlap_count_str = prompt("Overlap count", "0")
        overlap_count = int(overlap_count_str) if overlap_count_str.isdigit() else 0

        kwargs["chunking"] = ChunkingConfigurationModel(
            enable=True,
            strategy=strategy,
            fixed_token_count=fixed_token_count,
            overlap_strategy=overlap_strategy,
            overlap_count=overlap_count,
        )

    return ApiProcessorSettingsModel(**kwargs) if kwargs else None


def _atom_to_dict(atom):
    """Convert an atom (model or dict) to a dict for display."""
    if hasattr(atom, "model_dump"):
        return atom.model_dump(by_alias=True, exclude_none=True)
    return atom


def display_atoms(atoms):
    """Display extracted atoms with chunk info."""
    if not isinstance(atoms, list):
        print(f"Result: {atoms}")
        return

    atom_list = [_atom_to_dict(a) for a in atoms]

    print(f"Extracted {len(atom_list)} atoms:")
    print()

    for atom in atom_list[:5]:
        atom_type = atom.get("Type", "Unknown")
        position = atom.get("Position", "?")
        content = ""
        if atom_type == "Table":
            content = f"Table with {atom.get('Rows', '?')} rows, {atom.get('Columns', '?')} columns"
        elif atom.get("Text"):
            content = atom["Text"][:300]
        elif atom.get("UnorderedList"):
            content = f"Unordered list with {len(atom['UnorderedList'])} items"
        elif atom.get("OrderedList"):
            content = f"Ordered list with {len(atom['OrderedList'])} items"
        else:
            content = "No text content"

        print(f"  Atom [{position}]: {atom_type} - {content}")

        chunks = atom.get("Chunks")
        if chunks and len(chunks) > 0:
            print(f"    Chunks: {len(chunks)}")
            for chunk in chunks[:3]:
                chunk_text = chunk.get("Text", "No text")
                chunk_preview = chunk_text[:100] if chunk_text else "No text"
                print(f"      Chunk [{chunk.get('Position', '?')}]: {chunk.get('Length', '?')} chars - {chunk_preview}...")
            if len(chunks) > 3:
                print(f"      ... and {len(chunks) - 3} more chunks")

    if len(atom_list) > 5:
        print(f"... and {len(atom_list) - 5} more atoms")

    # Chunking summary
    atoms_with_chunks = sum(1 for a in atom_list if a.get("Chunks") and len(a["Chunks"]) > 0)
    total_chunks = sum(len(a.get("Chunks", [])) for a in atom_list)
    if atoms_with_chunks > 0:
        print()
        print(f"Chunking summary: {atoms_with_chunks} atoms with chunks, {total_chunks} total chunks")

    print()
    print("Full result:")
    print(json.dumps(atoms_data, indent=2))


def test_document_processing(document_type, extract_method):
    """Test document processing for a given type."""
    try:
        filename = prompt(f"File path for {document_type} processing")
        if not filename or not os.path.isfile(filename):
            print("File not found.")
            return

        settings = configure_settings(document_type)

        file_size = os.path.getsize(filename)
        print(f"Processing {document_type} file: {filename} ({file_size} bytes)...")

        start_time = time.time()
        result = extract_method(filename, settings=settings) if settings else extract_method(filename)
        elapsed = (time.time() - start_time) * 1000

        if result:
            print(f"Processing completed in {elapsed:.2f}ms")
            display_atoms(result)
        else:
            print("No atoms extracted.")
    except Exception as e:
        print(f"Error processing {document_type}: {e}")


def main():
    """Main interactive loop."""
    endpoint = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:8000"
    document_atom_sdk.configure(endpoint=endpoint)

    print("DocumentAtom SDK Test Application (Python)")
    print("===========================================")
    print(f"SDK initialized with endpoint: {endpoint}")
    print()

    commands = {
        "connectivity": test_connectivity,
        "detect": test_type_detection,
        "csv": lambda: test_document_processing("CSV", document_atom_sdk.AtomExtraction.extract_atoms_csv),
        "excel": lambda: test_document_processing("Excel", document_atom_sdk.AtomExtraction.extract_atoms_excel),
        "html": lambda: test_document_processing("HTML", document_atom_sdk.AtomExtraction.extract_atoms_html),
        "json": lambda: test_document_processing("JSON", document_atom_sdk.AtomExtraction.extract_atoms_json),
        "markdown": lambda: test_document_processing("Markdown", document_atom_sdk.AtomExtraction.extract_atoms_markdown),
        "ocr": lambda: test_document_processing("OCR", document_atom_sdk.AtomExtraction.extract_atoms_ocr),
        "pdf": lambda: test_document_processing("PDF", document_atom_sdk.AtomExtraction.extract_atoms_pdf),
        "png": lambda: test_document_processing("PNG", document_atom_sdk.AtomExtraction.extract_atoms_png),
        "powerpoint": lambda: test_document_processing("PowerPoint", document_atom_sdk.AtomExtraction.extract_atoms_powerpoint),
        "rtf": lambda: test_document_processing("RTF", document_atom_sdk.AtomExtraction.extract_atoms_rtf),
        "text": lambda: test_document_processing("Text", document_atom_sdk.AtomExtraction.extract_atoms_text),
        "word": lambda: test_document_processing("Word", document_atom_sdk.AtomExtraction.extract_atoms_word),
        "xml": lambda: test_document_processing("XML", document_atom_sdk.AtomExtraction.extract_atoms_xml),
    }

    while True:
        try:
            user_input = prompt("Command [? for help]")
        except (EOFError, KeyboardInterrupt):
            break

        if user_input == "?":
            show_menu()
        elif user_input == "q":
            break
        elif user_input == "cls":
            os.system("cls" if os.name == "nt" else "clear")
        elif user_input == "endpoint":
            print(f"Current endpoint: {endpoint}")
        elif user_input in commands:
            commands[user_input]()
        else:
            print("Unknown command. Type '?' for help.")

    print("Goodbye!")


if __name__ == "__main__":
    main()
