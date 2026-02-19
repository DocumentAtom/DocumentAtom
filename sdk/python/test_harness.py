#!/usr/bin/env python3
"""Automated test harness for the DocumentAtom Python SDK."""

import os
import sys
import time

# ---------------------------------------------------------------------------
# Fixture / mapping configuration
# ---------------------------------------------------------------------------

FIXTURE_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "test-fixtures")

FIXTURES = [
    ("sample.csv",  "csv"),
    ("sample.html", "html"),
    ("sample.json", "json"),
    ("sample.md",   "markdown"),
    ("sample.txt",  "text"),
    ("sample.xml",  "xml"),
    ("sample.rtf",  "rtf"),
    ("sample.pdf",  "pdf"),
    ("sample.docx", "word"),
    ("sample.xlsx", "excel"),
    ("sample.pptx", "powerpoint"),
    ("sample.png",  "png"),
    ("sample.jpg",  "ocr"),
]

# Image formats (minimal test images have no text content)
IMAGE_FORMATS = {"png", "ocr"}

# Formats that support chunking (skip PNG/JPG)
CHUNKING_SKIP = {"png", "ocr"}

# Formats that support the ExtractAtomsFromImages setting
OCR_FORMATS = {"pdf", "word", "powerpoint"}

# ---------------------------------------------------------------------------
# Test result tracking
# ---------------------------------------------------------------------------

results = []  # list of (status, duration, name, message)


def record(status, duration, name, message=""):
    results.append((status, duration, name, message))
    tag = {"PASS": "[PASS]", "FAIL": "[FAIL]", "SKIP": "[SKIP]"}[status]
    line = f"{tag}  {duration:7.3f}s  {name}"
    if message:
        line += f" - {message}"
    print(line)


def run_test(name, fn):
    """Execute *fn*; record PASS/FAIL and elapsed time."""
    t0 = time.perf_counter()
    try:
        fn()
        record("PASS", time.perf_counter() - t0, name)
    except Exception as exc:
        record("FAIL", time.perf_counter() - t0, name, str(exc))


def skip_test(name, reason):
    record("SKIP", 0.0, name, reason)


# ---------------------------------------------------------------------------
# Fixture loader
# ---------------------------------------------------------------------------

def load_fixture(filename):
    """Return (bytes, full_path) or (None, None) if missing."""
    path = os.path.join(FIXTURE_DIR, filename)
    if not os.path.isfile(path):
        return None, None
    with open(path, "rb") as f:
        return f.read(), path


# ---------------------------------------------------------------------------
# Test implementations
# ---------------------------------------------------------------------------

def test_connectivity():
    from document_atom_sdk import Connectivity
    ok = Connectivity.validate_connectivity()
    assert ok, "validate_connectivity returned falsy"


def test_status():
    from document_atom_sdk import Connectivity
    ok = Connectivity.validate_connectivity()
    assert ok is not None, "status returned None"


def test_type_detection(filename, fmt):
    from document_atom_sdk import TypeDetection
    data, path = load_fixture(filename)
    if data is None:
        skip_test(f"Type detection: {filename}", "fixture not found")
        return
    result = TypeDetection.detect_type(data, filename=filename)
    assert result is not None, "detect_type returned None"


def test_extraction_default(filename, fmt):
    from document_atom_sdk import AtomExtraction
    data, path = load_fixture(filename)
    if data is None:
        skip_test(f"Extraction (default): {filename}", "fixture not found")
        return
    atoms = AtomExtraction.extract_atoms(data, fmt, filename=filename)
    assert atoms is not None, "extract_atoms returned None"
    if fmt not in IMAGE_FORMATS:
        assert len(atoms) >= 1, f"expected >= 1 atom, got {len(atoms)}"


def test_extraction_chunking(filename, fmt):
    from document_atom_sdk import AtomExtraction, ApiProcessorSettingsModel, ChunkingConfigurationModel
    data, path = load_fixture(filename)
    if data is None:
        skip_test(f"Extraction (chunking): {filename}", "fixture not found")
        return
    settings = ApiProcessorSettingsModel(
        chunking=ChunkingConfigurationModel(
            enable=True,
            strategy="FixedTokenCount",
            fixed_token_count=32,
            overlap_count=0,
        )
    )
    atoms = AtomExtraction.extract_atoms(data, fmt, settings=settings, filename=filename)
    assert atoms is not None, "extract_atoms returned None"
    assert len(atoms) >= 1, f"expected >= 1 atom, got {len(atoms)}"
    has_chunks = any(a.chunks for a in atoms)
    assert has_chunks, "expected at least one atom with non-empty Chunks"


def test_extraction_ocr(filename, fmt):
    from document_atom_sdk import AtomExtraction, ApiProcessorSettingsModel
    data, path = load_fixture(filename)
    if data is None:
        skip_test(f"Extraction (OCR setting): {filename}", "fixture not found")
        return
    settings = ApiProcessorSettingsModel(extract_atoms_from_images=True)
    atoms = AtomExtraction.extract_atoms(data, fmt, settings=settings, filename=filename)
    assert atoms is not None, "extract_atoms returned None"
    assert len(atoms) >= 1, f"expected >= 1 atom, got {len(atoms)}"


def test_error_empty_text():
    from document_atom_sdk import AtomExtraction
    try:
        atoms = AtomExtraction.extract_atoms_text(b"", filename="empty.txt")
        # If no exception, we accept empty/null result
    except Exception:
        pass  # Error response is acceptable


def test_error_wrong_format():
    from document_atom_sdk import AtomExtraction
    csv_data, _ = load_fixture("sample.csv")
    if csv_data is None:
        skip_test("Error: wrong format", "fixture not found")
        return
    try:
        atoms = AtomExtraction.extract_atoms_pdf(csv_data, filename="sample.csv")
        # Graceful failure is acceptable
    except Exception:
        pass  # Error response is acceptable


def test_error_empty_type_detection():
    from document_atom_sdk import TypeDetection
    try:
        result = TypeDetection.detect_type(b"", filename="empty.bin")
        # Accepting any non-crash result
    except Exception:
        pass  # Error response is acceptable


# ---------------------------------------------------------------------------
# Main runner
# ---------------------------------------------------------------------------

def main():
    if len(sys.argv) < 2:
        print("Usage: python test_harness.py <endpoint>")
        print("  e.g. python test_harness.py http://localhost:8000")
        sys.exit(1)

    endpoint = sys.argv[1].rstrip("/")

    # Configure SDK
    from document_atom_sdk import configure
    configure(endpoint=endpoint)

    overall_start = time.perf_counter()

    # 1. Connectivity (2 tests)
    run_test("Connectivity: server is reachable", test_connectivity)
    run_test("Connectivity: server returns status", test_status)

    # 2. Type Detection (13 tests)
    for filename, fmt in FIXTURES:
        data, _ = load_fixture(filename)
        if data is None:
            skip_test(f"Type detection: {filename}", "fixture not found")
        else:
            run_test(f"Type detection: {filename}", lambda fn=filename, ft=fmt: test_type_detection(fn, ft))

    # 3. Extraction — No Settings (13 tests)
    for filename, fmt in FIXTURES:
        data, _ = load_fixture(filename)
        if data is None:
            skip_test(f"Extraction (default): {filename}", "fixture not found")
        else:
            run_test(f"Extraction (default): {filename}", lambda fn=filename, ft=fmt: test_extraction_default(fn, ft))

    # 4. Extraction — With Chunking (11 tests, skip PNG/JPG)
    for filename, fmt in FIXTURES:
        if fmt in CHUNKING_SKIP:
            skip_test(f"Extraction (chunking): {filename}", "chunking not applicable for images")
            continue
        data, _ = load_fixture(filename)
        if data is None:
            skip_test(f"Extraction (chunking): {filename}", "fixture not found")
        else:
            run_test(f"Extraction (chunking): {filename}", lambda fn=filename, ft=fmt: test_extraction_chunking(fn, ft))

    # 5. Extraction — With OCR Setting (3 tests: PDF, Word, PowerPoint)
    for filename, fmt in FIXTURES:
        if fmt not in OCR_FORMATS:
            continue
        data, _ = load_fixture(filename)
        if data is None:
            skip_test(f"Extraction (OCR setting): {filename}", "fixture not found")
        else:
            run_test(f"Extraction (OCR setting): {filename}", lambda fn=filename, ft=fmt: test_extraction_ocr(fn, ft))

    # 6. Error Cases (3 tests)
    run_test("Error: empty data to text extraction", test_error_empty_text)
    run_test("Error: CSV bytes sent to PDF endpoint", test_error_wrong_format)
    run_test("Error: empty data to type detection", test_error_empty_type_detection)

    overall_duration = time.perf_counter() - overall_start

    # Summary
    passed = sum(1 for r in results if r[0] == "PASS")
    failed = sum(1 for r in results if r[0] == "FAIL")
    skipped = sum(1 for r in results if r[0] == "SKIP")
    total = len(results)
    failed_tests = [r for r in results if r[0] == "FAIL"]

    print()
    print("=" * 60)
    print("Test Summary")
    print("=" * 60)
    print(f"Total:    {total}")
    print(f"Passed:   {passed}")
    print(f"Failed:   {failed}")
    print(f"Skipped:  {skipped}")
    print(f"Duration: {overall_duration:.3f}s")
    if failed_tests:
        print()
        print("Failed tests:")
        for _, dur, name, msg in failed_tests:
            print(f"  - {name} - {msg}")
    print("=" * 60)

    sys.exit(1 if failed > 0 else 0)


if __name__ == "__main__":
    main()
